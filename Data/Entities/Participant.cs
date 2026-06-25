using System.Collections.Generic;

namespace SplitBill.Data.Entities;

public class Participant
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    // Redesign schema extensions
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;

    // Navigation properties
    public Bill Bill { get; set; } = null!;
    public ICollection<ItemAssignee> AssignedItems { get; set; } = new List<ItemAssignee>();
    public ICollection<Payer> Payments { get; set; } = new List<Payer>();
}
