using Boxes;
using System.Data.SqlTypes;
using System.Diagnostics;
using System;
using System.Text;

// TODO: scan dir for files

// TODO: read file in chunks
using var fs = new FileStream("data.txt", FileMode.Open);
var buffer = new byte[1024];
var boxes = new List<Box>();
Box box = null;
List<string> cutUpLine = null;
while(await fs.ReadAsync(buffer, 0, buffer.Length) > 0)
{
    // TODO: from string to box objects
    var data = Encoding.Default.GetString(buffer);
    var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    foreach (var line in lines)
    {
        var elements = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // TODO: VALIDATE TEXT!!!!!!
        ParseLines(boxes, ref box, ref cutUpLine, elements);

        // TODO: insert box to db
    }
    // TODO: insert/update db
}

// TODO: delete file or move to processed directory

foreach (var b in boxes)
    Console.WriteLine(b);

static void ParseLines(List<Box> boxes, ref Box box, ref List<string> cutUpLine, string[] elements)
{
    if (elements[0] == "HDR" && elements.Count() == 3)
    {
        box = new Box()
        {
            SupplierId = elements[1],
            Id = elements[2],
            Items = new List<Item>()
        };
        boxes.Add(box);
    }
    else if (elements[0] == "LINE" && elements.Count() == 4)
    {
        var item = new Item
        {
            PO = elements[1],
            ISBN = long.Parse(elements[2]), // TODO: Error invalid PO -> log and continue
            Qty = int.Parse(elements[3]) // TODO: Error invalid Quantity -> log and continue
        };
        box.Items.Add(item);
    }
    else if (cutUpLine == null) // handle cut up line
    {
        cutUpLine = [.. elements];
    }
    else
    {
        cutUpLine.AddRange(elements);
        if (cutUpLine[0] == "HDR")
        {
            box = new Box
            {
                SupplierId = cutUpLine[1],
                Id = cutUpLine[2],
                Items = new List<Item>()
            };
            boxes.Add(box);
        }
        else if (elements[0] == "LINE")
        {
            var item = new Item
            {
                PO = cutUpLine[1],
                ISBN = long.Parse(cutUpLine[2]), // TODO: Error invalid PO -> log and continue
                Qty = int.Parse(cutUpLine[3]) // TODO: Error invalid Quantity -> log and continue
            };
            box.Items.Add(item);
        }
        cutUpLine = null;
    }
}

//TODO: background job
//TODO: logging
//TODO: validation(skip and log if invalid)
//TODO: scan the toProcess folder
//TODO: process and move the file to processedFolder
//TODO: sqlite db
//TODO: UPDATE THE BOX IF IT ALREADY EXISTS
//TODO: check for duplicate boxes