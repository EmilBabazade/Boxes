using Domain;

namespace Data.Entities;
public class BoxEntity : IBox
{
    public string SupplierId { get; set; }
    public string Id { get; set; }
    public ICollection<ItemEntity> Items { get; set; }
}
