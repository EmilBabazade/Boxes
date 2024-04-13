namespace Data;
public class Box
{
    public Box(string supplierId, string Id)
    {
        SupplierId = supplierId;
        this.Id = Id;
        Items = [];
    }
    public Box(string supplierId, string Id, ICollection<Item> items)
    {
        SupplierId = supplierId;
        this.Id = Id;
        Items = items;
    }

    public string SupplierId { get; set; }
    public string Id { get; set; }

    public ICollection<Item> Items { get; set; }

    // just to make logging easier
    public override string ToString()
    {
        var str = $"Box\tId: {Id}\tSupplierId: {SupplierId}\tItems:";
        foreach (var item in Items)
        {
            str += "\n" + item;
        }
        return str;
    }
}
