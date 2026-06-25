using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SplitBill.Data.Entities;
using SplitBill.Services;

namespace split_bill.Tests;

public class SettlementServiceTests
{
    private readonly SettlementService _service;

    public SettlementServiceTests()
    {
        _service = new SettlementService();
    }

    [Fact]
    public void CalculateSettlement_WithNoParticipants_ReturnsEmptyResult()
    {
        // Arrange
        var bill = new Bill
        {
            Id = 1,
            Uuid = "test-uuid",
            EventName = "Lunch",
            TaxType = ChargeType.Percentage,
            TaxValue = 10,
            ServiceChargeType = ChargeType.Percentage,
            ServiceChargeValue = 5,
            Items = new List<Item>
            {
                new Item { Id = 1, Name = "Burger", Price = 50000 },
                new Item { Id = 2, Name = "Soda", Price = 20000 }
            }
        };

        // Act
        var result = _service.CalculateSettlement(bill);

        // Assert
        Assert.Equal(60303.03m, Math.Round(result.FoodSubtotal, 2));
        Assert.Equal(6363.64m, Math.Round(result.TaxTotal, 2)); // 10% tax (inclusive)
        Assert.Equal(3333.33m, Math.Round(result.ServiceChargeTotal, 2)); // 5% service charge (inclusive)
        Assert.Equal(70000m, result.GrandTotal);
        Assert.Empty(result.ParticipantSummaries);
        Assert.Empty(result.TransferInstructions);
        Assert.True(result.HasPaymentDifferenceWarning);
        Assert.Equal(-70000m, result.PaymentDifference);
    }

    [Fact]
    public void CalculateSettlement_WithEqualSplits_ComputesCorrectBalancesAndTransfers()
    {
        // Arrange
        var bill = new Bill
        {
            Id = 1,
            Uuid = "test-uuid-2",
            EventName = "Dinner",
            TaxType = ChargeType.Percentage,
            TaxValue = 10, // 10% tax
            ServiceChargeType = ChargeType.Fixed,
            ServiceChargeValue = 10000, // Fixed Rp 10.000 service charge
            Participants = new List<Participant>
            {
                new Participant { Id = 1, Name = "Alice" },
                new Participant { Id = 2, Name = "Bob" },
                new Participant { Id = 3, Name = "Charlie" }
            },
            Items = new List<Item>
            {
                new Item { Id = 1, Name = "Steak", Price = 150000 }, // Alice only
                new Item { Id = 2, Name = "Pizza", Price = 90000 },  // Bob & Charlie (shared equally)
                new Item { Id = 3, Name = "Drinks", Price = 30000 }   // Shared by all three (Alice, Bob, Charlie)
            },
            Payers = new List<Payer>
            {
                new Payer { Id = 1, ParticipantId = 1, AmountPaid = 310000 } // Alice paid everything
            }
        };

        // Item Assignees
        // Steak -> Alice
        bill.Items.ElementAt(0).Assignees = new List<ItemAssignee>
        {
            new ItemAssignee { ItemId = 1, ParticipantId = 1, ShareRatio = 1.0m }
        };

        // Pizza -> Bob & Charlie (Shared equally)
        bill.Items.ElementAt(1).Assignees = new List<ItemAssignee>
        {
            new ItemAssignee { ItemId = 2, ParticipantId = 2, ShareRatio = 1.0m },
            new ItemAssignee { ItemId = 2, ParticipantId = 3, ShareRatio = 1.0m }
        };

        // Drinks -> Alice, Bob, Charlie (Shared equally)
        bill.Items.ElementAt(2).Assignees = new List<ItemAssignee>
        {
            new ItemAssignee { ItemId = 3, ParticipantId = 1, ShareRatio = 1.0m },
            new ItemAssignee { ItemId = 3, ParticipantId = 2, ShareRatio = 1.0m },
            new ItemAssignee { ItemId = 3, ParticipantId = 3, ShareRatio = 1.0m }
        };

        // Act
        var result = _service.CalculateSettlement(bill);

        // Assert
        // Total food subtotal: 215454.55m
        Assert.Equal(215454.55m, Math.Round(result.FoodSubtotal, 2));
        // Tax = 24545.45m
        Assert.Equal(24545.45m, Math.Round(result.TaxTotal, 2));
        // Service charge = 30000m
        Assert.Equal(30000m, Math.Round(result.ServiceChargeTotal, 2));
        // Grand total = 270000m
        Assert.Equal(270000m, result.GrandTotal);

        var aliceSum = result.ParticipantSummaries.First(p => p.ParticipantId == 1);
        var bobSum = result.ParticipantSummaries.First(p => p.ParticipantId == 2);
        var charlieSum = result.ParticipantSummaries.First(p => p.ParticipantId == 3);

        Assert.Equal(132121m, aliceSum.FoodSubtotal);
        Assert.Equal(41667m, bobSum.FoodSubtotal);
        Assert.Equal(41667m, charlieSum.FoodSubtotal);

        // Tax share
        Assert.Equal(14545m, aliceSum.TaxShare);
        Assert.Equal(5000m, bobSum.TaxShare);
        Assert.Equal(5000m, charlieSum.TaxShare);

        // Service share
        Assert.Equal(13333m, aliceSum.ServiceShare);
        Assert.Equal(8333m, bobSum.ServiceShare);
        Assert.Equal(8333m, charlieSum.ServiceShare);

        // Total Owed
        Assert.Equal(159999m, aliceSum.TotalOwed);
        Assert.Equal(55000m, bobSum.TotalOwed);
        Assert.Equal(55000m, charlieSum.TotalOwed);

        // Balances:
        Assert.Equal(150001m, aliceSum.Balance);
        Assert.Equal(-55000m, bobSum.Balance);
        Assert.Equal(-55000m, charlieSum.Balance);

        // Transfers:
        Assert.Equal(2, result.TransferInstructions.Count);
        
        var transferBob = result.TransferInstructions.First(t => t.DebtorId == 2);
        Assert.Equal(1, transferBob.CreditorId);
        Assert.Equal(55000m, transferBob.Amount);

        var transferCharlie = result.TransferInstructions.First(t => t.DebtorId == 3);
        Assert.Equal(1, transferCharlie.CreditorId);
        Assert.Equal(55000m, transferCharlie.Amount);
    }
}
