namespace Domain;

public interface IBox : IUnique
{
    public string SupplierId { get; set; }
    public string Identifier { get; set; }
}
