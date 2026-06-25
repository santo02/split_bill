using System.Collections.Generic;

namespace SplitBill.Data.Entities;

public class Item
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ChargeType TaxType { get; set; } = ChargeType.Percentage;
    public decimal TaxValue { get; set; } = 0m;
    public ChargeType ServiceChargeType { get; set; } = ChargeType.Percentage;
    public decimal ServiceChargeValue { get; set; } = 0m;
    public int? FrontedByParticipantId { get; set; } // participant who paid for this item
    public bool IsDeleted { get; set; }

    // Redesign schema extensions
    public string Category { get; set; } = "Food";
    public DateTime Date { get; set; } = DateTime.Today;
    public string Notes { get; set; } = string.Empty;

    // Navigation properties
    public Bill Bill { get; set; } = null!;
    public Participant? FrontedBy { get; set; }
    public ICollection<ItemAssignee> Assignees { get; set; } = new List<ItemAssignee>();
}
