using System;
using System.Collections.Generic;

namespace SplitBill.Data.Entities;

public enum ChargeType
{
    Percentage,
    Fixed
}

public class Bill
{
    public int Id { get; set; }
    public string Uuid { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ChargeType TaxType { get; set; } = ChargeType.Percentage;
    public decimal TaxValue { get; set; }
    public ChargeType ServiceChargeType { get; set; } = ChargeType.Percentage;
    public decimal ServiceChargeValue { get; set; }
    public string CreatorToken { get; set; } = string.Empty;
    public int? CollectorParticipantId { get; set; }
    public Participant? Collector { get; set; }
    public bool IsDeleted { get; set; }

    // Redesign schema extensions
    public string Description { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "ONGOING";

    // Navigation properties
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<Payer> Payers { get; set; } = new List<Payer>();
}
