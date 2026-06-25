namespace SplitBill.Data.Entities;

public class Payer
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public int ParticipantId { get; set; }
    public decimal AmountPaid { get; set; }

    // Removed: public Bill Bill { get; set; } = null!;
    public Participant Participant { get; set; } = null!;
}