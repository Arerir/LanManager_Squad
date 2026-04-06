namespace LanManager.Data.Models;

public enum EquipmentType { Computer, Keyboard, Mouse, Mousemat, VrHeadset, Other }

public class Equipment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EquipmentType Type { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ICollection<EquipmentLoan> Loans { get; set; } = new List<EquipmentLoan>();
}
