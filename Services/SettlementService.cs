using System;
using System.Collections.Generic;
using System.Linq;
using SplitBill.Data.Entities;

namespace SplitBill.Services;

public class ParticipantSummary
{
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public decimal FoodSubtotal { get; set; }   // Porsi harga dasar (pre-tax)
    public decimal TaxShare { get; set; }        // Porsi pajak (informasi saja)
    public decimal ServiceShare { get; set; }    // Porsi service (informasi saja)
    public decimal TotalOwed { get; set; }       // = porsi dari Item.Price (sudah include pajak)
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }         // Paid - Owed
}

public class TransferInstruction
{
    public int DebtorId { get; set; }
    public string DebtorName { get; set; } = string.Empty;
    public int CreditorId { get; set; }
    public string CreditorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SettlementResult
{
    public decimal FoodSubtotal { get; set; }        // Total harga dasar (Price dikurangi pajak)
    public decimal TaxTotal { get; set; }            // Total pajak (informasi breakdown)
    public decimal ServiceChargeTotal { get; set; }  // Total service (informasi breakdown)
    public decimal GrandTotal { get; set; }          // = sum of Item.Price (sudah include pajak)
    public decimal TotalPaidToCashier { get; set; }
    public decimal PaymentDifference { get; set; }
    public bool HasPaymentDifferenceWarning => Math.Abs(PaymentDifference) >= 1.0m;

    public List<ParticipantSummary> ParticipantSummaries { get; set; } = new();
    public List<TransferInstruction> TransferInstructions { get; set; } = new();
}

public class SettlementService
{
    /// <summary>
    /// Item.Price = harga TOTAL yang sudah dibayar ke kasir, SUDAH INCLUDE pajak & service.
    /// TaxValue & ServiceChargeValue hanya digunakan untuk menampilkan breakdown informasi.
    ///
    /// Contoh: Makan malam Rp 100.000 (include pajak 10%)
    ///   → GrandTotal item = Rp 100.000
    ///   → FoodSubtotal (breakdown) = Rp 90.909
    ///   → Tax (breakdown)          = Rp  9.091
    ///   → Dibagi 5 orang           = Rp 20.000 / orang (dari 100k, bukan 90k)
    /// </summary>
    public SettlementResult CalculateSettlement(Bill bill)
    {
        var result = new SettlementResult();

        var activeItems = bill.Items.Where(i => !i.IsDeleted).ToList();
        var activeParticipants = bill.Participants.Where(p => !p.IsDeleted).ToList();

        // --- Hitung total & breakdown informatif ---
        // Item.Price sudah include pajak, jadi kita balik-hitung (back-calculate) komponen pajak
        foreach (var item in activeItems)
        {
            // Back-calculate: berapa porsi pajak dari harga total?
            // Misal Price=100k, Tax=10% (inclusive) → tax = 100k * 10/110 = 9.091
            // Misal Price=100k, Tax=10k fixed (inclusive) → tax = 10k
            decimal taxPortion = CalculateInclusivePortion(item.Price, item.TaxValue > 0 ? item.TaxType : bill.TaxType, item.TaxValue > 0 ? item.TaxValue : bill.TaxValue);
            decimal servicePortion = CalculateInclusivePortion(item.Price, item.ServiceChargeValue > 0 ? item.ServiceChargeType : bill.ServiceChargeType, item.ServiceChargeValue > 0 ? item.ServiceChargeValue : bill.ServiceChargeValue);

            // FoodSubtotal = Price dikurangi tax dan service (hanya untuk breakdown tampilan)
            decimal foodPortion = item.Price - taxPortion - servicePortion;

            result.FoodSubtotal += foodPortion;
            result.TaxTotal += taxPortion;
            result.ServiceChargeTotal += servicePortion;

            // GrandTotal = sum of Item.Price (karena Price sudah include segalanya)
            result.GrandTotal += item.Price;
        }

        result.TotalPaidToCashier = bill.Payers.Sum(p => p.AmountPaid);
        result.PaymentDifference = result.TotalPaidToCashier - result.GrandTotal;

        if (!activeParticipants.Any()) return result;

        // --- Inisialisasi ringkasan per peserta ---
        var summaries = activeParticipants.ToDictionary(
            p => p.Id,
            p => new ParticipantSummary
            {
                ParticipantId = p.Id,
                ParticipantName = p.Name,
                TotalPaid = bill.Payers
                    .Where(pa => pa.ParticipantId == p.Id)
                    .Sum(pa => pa.AmountPaid)
            });

        // --- Distribusi beban per item ---
        // Pembagian langsung dari Item.Price (sudah include pajak)
        // Tidak ada penambahan pajak lagi di atas porsi masing-masing orang
        foreach (var item in activeItems)
        {
            var assignees = item.Assignees
                .Where(a => summaries.ContainsKey(a.ParticipantId))
                .ToList();

            if (!assignees.Any()) continue;

            // Back-calculate porsi pajak & service dari harga total (untuk breakdown per orang)
            decimal taxPortion = CalculateInclusivePortion(item.Price, item.TaxValue > 0 ? item.TaxType : bill.TaxType, item.TaxValue > 0 ? item.TaxValue : bill.TaxValue);
            decimal servicePortion = CalculateInclusivePortion(item.Price, item.ServiceChargeValue > 0 ? item.ServiceChargeType : bill.ServiceChargeType, item.ServiceChargeValue > 0 ? item.ServiceChargeValue : bill.ServiceChargeValue);
            decimal foodPortion = item.Price - taxPortion - servicePortion;

            // Multiplier untuk breakdown informatif per orang
            // (bukan untuk menambah beban — hanya untuk menampilkan berapa porsi pajak mereka)
            decimal taxRatio     = item.Price > 0 ? taxPortion / item.Price : 0m;
            decimal serviceRatio = item.Price > 0 ? servicePortion / item.Price : 0m;
            decimal foodRatio    = item.Price > 0 ? foodPortion / item.Price : 0m;

            // Hitung porsi dari Item.Price per assignee
            decimal totalNominal = assignees
                .Where(a => a.InputType == AssigneeInputType.Nominal)
                .Sum(a => a.NominalAmount);

            // Sisa dari total price (include pajak) setelah nominal dikurangi
            decimal remainingForRatio = Math.Max(0, item.Price - totalNominal);

            var ratioAssignees = assignees.Where(a => a.InputType == AssigneeInputType.Ratio).ToList();
            decimal totalRatio = ratioAssignees.Sum(a => a.ShareRatio);

            foreach (var assignee in assignees)
            {
                // shareOfTotal = porsi dari Item.Price (sudah include pajak)
                decimal shareOfTotal;

                if (assignee.InputType == AssigneeInputType.Nominal)
                {
                    // Nominal yang diisi user = langsung porsi dari Item.Price (include pajak)
                    shareOfTotal = assignee.NominalAmount;
                }
                else
                {
                    shareOfTotal = totalRatio > 0
                        ? remainingForRatio * (assignee.ShareRatio / totalRatio)
                        : 0m;
                }

                // Breakdown informatif: berapa dari shareOfTotal itu pajak & makanan
                summaries[assignee.ParticipantId].FoodSubtotal  += shareOfTotal * foodRatio;
                summaries[assignee.ParticipantId].TaxShare      += shareOfTotal * taxRatio;
                summaries[assignee.ParticipantId].ServiceShare  += shareOfTotal * serviceRatio;

                // TotalOwed langsung dari shareOfTotal — tidak ada tambahan pajak
                summaries[assignee.ParticipantId].TotalOwed += shareOfTotal;
            }
        }

        // --- FrontedBy: peserta yang bayar item ini ke kasir ---
        foreach (var item in activeItems)
        {
            if (item.FrontedByParticipantId.HasValue
                && summaries.ContainsKey(item.FrontedByParticipantId.Value))
            {
                // Item.Price sudah include pajak, jadi langsung tambahkan
                summaries[item.FrontedByParticipantId.Value].TotalPaid += item.Price;
            }
        }

        // --- Finalisasi: bulatkan dan hitung Balance ---
        foreach (var summary in summaries.Values)
        {
            // Round TotalOwed directly from the original unrounded value
            // This ensures TotalOwed is accurate and consistent
            summary.TotalOwed = Math.Round(summary.TotalOwed, 0);

            // Breakdown components are rounded separately for display only
            // They are informational and don't need to sum exactly to TotalOwed
            summary.FoodSubtotal = Math.Round(summary.FoodSubtotal, 0);
            summary.TaxShare     = Math.Round(summary.TaxShare, 0);
            summary.ServiceShare = Math.Round(summary.ServiceShare, 0);

            summary.Balance = summary.TotalPaid - summary.TotalOwed;
        }

        result.ParticipantSummaries = summaries.Values.ToList();

        // --- Transfer instructions ---
        if (bill.CollectorParticipantId.HasValue
            && summaries.ContainsKey(bill.CollectorParticipantId.Value))
        {
            var collectorId   = bill.CollectorParticipantId.Value;
            var collectorName = summaries[collectorId].ParticipantName;
            var instructions  = new List<TransferInstruction>();

            foreach (var s in summaries.Values)
            {
                if (s.ParticipantId == collectorId) continue;

                if (s.Balance < -0.5m)
                {
                    instructions.Add(new TransferInstruction
                    {
                        DebtorId     = s.ParticipantId,
                        DebtorName   = s.ParticipantName,
                        CreditorId   = collectorId,
                        CreditorName = collectorName,
                        Amount       = Math.Round(-s.Balance, 0)
                    });
                }
                else if (s.Balance > 0.5m)
                {
                    instructions.Add(new TransferInstruction
                    {
                        DebtorId     = collectorId,
                        DebtorName   = collectorName,
                        CreditorId   = s.ParticipantId,
                        CreditorName = s.ParticipantName,
                        Amount       = Math.Round(s.Balance, 0)
                    });
                }
            }

            result.TransferInstructions = instructions;
        }
        else
        {
            result.TransferInstructions = MinimizeCashFlow(result.ParticipantSummaries);
        }

        return result;
    }

    /// <summary>
    /// Hitung porsi pajak/service dari harga TOTAL yang sudah include pajak (inclusive).
    ///
    /// Percentage inclusive: tax = total * rate / (100 + rate)
    ///   Contoh: total=100k, rate=10% → tax = 100k * 10/110 = 9.091
    ///
    /// Fixed inclusive: tax = nilai fixed itu sendiri (langsung dikurangi dari total)
    ///   Contoh: total=100k, tax fixed=10k → tax = 10k, food = 90k
    /// </summary>
    private decimal CalculateInclusivePortion(decimal totalPrice, ChargeType type, decimal value)
    {
        if (value <= 0) return 0m;

        if (type == ChargeType.Percentage)
        {
            // Back-calculate dari inclusive price
            // totalPrice = foodBase * (1 + rate/100)
            // taxPortion = totalPrice * rate / (100 + rate)
            return totalPrice * value / (100m + value);
        }
        else
        {
            // Fixed amount langsung
            return Math.Min(value, totalPrice);
        }
    }

    private List<TransferInstruction> MinimizeCashFlow(List<ParticipantSummary> summaries)
    {
        var instructions = new List<TransferInstruction>();

        var creditors = summaries
            .Where(s => s.Balance > 0.5m)
            .Select(s => new TempBalance { Id = s.ParticipantId, Name = s.ParticipantName, Amount = s.Balance })
            .OrderByDescending(c => c.Amount)
            .ToList();

        var debtors = summaries
            .Where(s => s.Balance < -0.5m)
            .Select(s => new TempBalance { Id = s.ParticipantId, Name = s.ParticipantName, Amount = s.Balance })
            .OrderBy(d => d.Amount)
            .ToList();

        int cIdx = 0, dIdx = 0;

        while (cIdx < creditors.Count && dIdx < debtors.Count)
        {
            var creditor = creditors[cIdx];
            var debtor   = debtors[dIdx];

            decimal transfer = Math.Round(Math.Min(creditor.Amount, -debtor.Amount), 0);

            if (transfer > 0.5m)
            {
                instructions.Add(new TransferInstruction
                {
                    DebtorId     = debtor.Id,
                    DebtorName   = debtor.Name,
                    CreditorId   = creditor.Id,
                    CreditorName = creditor.Name,
                    Amount       = transfer
                });
            }

            creditor.Amount -= transfer;
            debtor.Amount   += transfer;

            if (creditor.Amount < 0.5m) cIdx++;
            if (debtor.Amount > -0.5m)  dIdx++;
        }

        return instructions;
    }

    private class TempBalance
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}