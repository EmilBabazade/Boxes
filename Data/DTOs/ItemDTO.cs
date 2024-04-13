using Domain;

namespace Data.DTOs;

public class ItemDTO : IItem
{
    public ItemDTO(long isbn, string po, int qty)
    {
        ISBN = isbn;
        PO = po;
        Qty = qty;
    }
    public long ISBN { get; set; }
    public string PO { get; set; }

    public int Qty { get; set; }
    public int BoxId { get; set; }

    // just to make logging easier
    public override string ToString()
    {
        return $"\tItem\tISBN: {ISBN}\tPO {PO}\tQuantity {Qty}";
    }
}
