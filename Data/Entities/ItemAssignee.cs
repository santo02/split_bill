namespace SplitBill.Data.Entities;

public enum AssigneeInputType
{
    Ratio,
    Nominal
}

public class ItemAssignee
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int ParticipantId { get; set; }
    public AssigneeInputType InputType { get; set; } = AssigneeInputType.Ratio;
    public decimal ShareRatio { get; set; } = 1.0m;
    public decimal NominalAmount { get; set; } = 0m;

    // Navigation properties
    public Item Item { get; set; } = null!;
    public Participant Participant { get; set; } = null!;
}
