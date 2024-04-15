namespace Domain;
public interface IItem : IUnique
{
    public long ISBN { get; set; }
    public string PO { get; set; }
    public int Qty { get; set; }
    public int BoxId { get; set; }
}
