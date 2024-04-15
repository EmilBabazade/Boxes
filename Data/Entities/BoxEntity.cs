using Domain;

namespace Data.Entities;
public class BoxEntity : IBox
{
    public int Id { get; set; }
    public string SupplierId { get; set; }
    public string Identifier { get; set; }
    public ICollection<ItemEntity> Items { get; set; }
}
