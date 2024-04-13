using Data.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Services.BoxProcessing;

public class BoxProcessingService : IBoxProcessingService
{
    private readonly ILogger<BoxProcessingService> _logger;
    private const int CHUNKS_TO_READ = 1024;

    public BoxProcessingService(ILogger<BoxProcessingService> logger)
    {
        _logger = logger;
    }
    public async Task<bool> ProcessBoxes(string fileDir, CancellationToken cancellationToken = default)
    {
        using var fs = new FileStream(fileDir, FileMode.Open);
        var buffer = new byte[CHUNKS_TO_READ];
        var boxes = new List<BoxDTO>();
        var isLineValid = true;
        BoxDTO? box = null;
        List<string>? cutUpLine = null;
        while (await fs.ReadAsync(buffer, 0, buffer.Length) > 0)
        {
            var data = Encoding.Default.GetString(buffer);
            var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var line in lines)
            {
                var elements = line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                try
                {
                    ProcessLine(boxes, ref box, ref cutUpLine, elements);
                    // TODO: push boxes and lines to db, ditch boxes list
                }
                catch(Exception ex)
                {
                    isLineValid = false;
                    _logger.LogError(ex.Message);
                    continue;
                }
            }
        }

        // TODO: REMOVE THIS
        foreach (var b in boxes)
            _logger.LogInformation(b.ToString());

        return isLineValid;
    }

    

    private void ProcessLine(List<BoxDTO> boxes, ref BoxDTO box, ref List<string>? cutUpLine, string[] lineElements)
    {
        if (lineElements[0] == "HDR" && lineElements.Count() == 3)
        {
            box = new BoxDTO(lineElements[1], lineElements[2]);
            boxes.Add(box);
        }
        else if (lineElements[0] == "LINE" && lineElements.Count() == 4)
        {
            var item = new ItemDTO(long.Parse(lineElements[2]), lineElements[1], int.Parse(lineElements[3]));
            box.Items.Add(item);
        }
        // after processing CHUNKS_TO_READ sized chunk from the file, sometimes a line gets cut in half
        // e.g. LINE P000001661         9781465121550 --> next chunk -->         12
        // cutUpLine will saves cut up line
        // if there is no cut up line from last chunk and our current line isn't a full box or item, then the line is just invalid
        else if (cutUpLine == null && !(lineElements[0] != "HDR" && lineElements.Count() <= 3) && !(lineElements[0] != "LINE" && lineElements.Count() <= 4))
        {
            throw new FormatException($"Line is invalid: {string.Join(" ", lineElements)}");
        }
        // if cutUpLine is null, then we don't have a cut up line from previous chunk, so we just save it to continue on the next line
        else if (cutUpLine == null)
        {
            cutUpLine = [.. lineElements];
        }
        // cutUpLine exists, so we are processing rest of the last line from previous line
        else
        {
            cutUpLine.AddRange(lineElements);
            if (cutUpLine[0] == "HDR")
            {
                box = new BoxDTO(cutUpLine[1], cutUpLine[2]);
                boxes.Add(box);
            }
            else if (cutUpLine[0] == "LINE")
            {
                var item = new ItemDTO(long.Parse(cutUpLine[2]), cutUpLine[1], int.Parse(cutUpLine[3]));
                box.Items.Add(item);
            }
            cutUpLine = null;
        }
    }

    //TODO: UPDATE THE BOX IF IT ALREADY EXISTS
}
