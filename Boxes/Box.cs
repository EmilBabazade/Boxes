namespace Boxes;
internal class Box
{
    public required string SupplierId { get; set; }
    public required string Id { get; set; }

    public required ICollection<Item> Items { get; set; } = [];

    // just to make logging easier
    public override string ToString()
    {
        var str = $"Box\tId: {Id}\tSupplierId: {SupplierId}\tItems:";
        foreach(var item in Items )
        {
            str += "\n" + item;
        }
        return str;
    }
}

internal class Item
{
    public required long ISBN { get; set; }
    public required string PO { get; set; }

    public required int Qty { get; set; }

    // just to make logging easier
    public override string ToString()
    {
        return $"\tItem\tISBN: {ISBN}\tPO {PO}\tQuantity {Qty}";
    }
}
