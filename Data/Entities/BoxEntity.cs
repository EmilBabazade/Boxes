using Domain;

namespace Data.Entities;
public class BoxEntity : IBox
{
    public int RowId { get; set; }
    public string SupplierId { get; set; }
    public string Id { get; set; }
    public ICollection<ItemEntity> Items { get; set; }
}
