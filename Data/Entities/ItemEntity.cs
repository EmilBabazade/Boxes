using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;
public class ItemEntity : IItem
{
    public int RowId { get; set; }
    public long ISBN { get; set; }
    public string PO { get; set; }
    public int Qty { get; set; }
    public int BoxId { get; set; }
    public BoxEntity Box { get; set; }
}
