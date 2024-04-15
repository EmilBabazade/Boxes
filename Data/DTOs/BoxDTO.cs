using Domain;

namespace Data.DTOs;
public class BoxDTO : IBox
{
    public BoxDTO(string supplierId, string Id)
    {
        SupplierId = supplierId;
        this.Identifier = Id;
        Items = [];
    }
    public BoxDTO(string supplierId, string Id, ICollection<ItemDTO> items)
    {
        SupplierId = supplierId;
        this.Identifier = Id;
        Items = items;
    }

    public string SupplierId { get; set; }
    public string Identifier { get; set; }

    public ICollection<ItemDTO> Items { get; set; }
    public int Id { get; set; }

    // just to make logging easier
    public override string ToString()
    {
        var str = $"Box\tId: {Identifier}\tSupplierId: {SupplierId}\tItems:";
        foreach (var item in Items)
        {
            str += "\n" + item;
        }
        return str;
    }
}
