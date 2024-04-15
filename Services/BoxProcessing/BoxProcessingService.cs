using AutoMapper;
using Data;
using Data.DTOs;
using Data.Entities;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;

namespace Services.BoxProcessing;

public class BoxProcessingService : IBoxProcessingService
{
    private readonly ILogger<BoxProcessingService> _logger;
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly int _chunkSize;
    private List<string>? _cutUpLine = null;
    private bool _isFileValid = true;

    public BoxProcessingService(ILogger<BoxProcessingService> logger, DataContext dataContext, IMapper mapper, IConfiguration config)
    {
        _logger = logger;
        _dataContext = dataContext;
        _mapper = mapper;
        var chunkSize = config["ChunkSize"] ?? "1024";
        _chunkSize = int.Parse(chunkSize);
    }
    public async Task<bool> TryProcessBoxes(string fileDir, CancellationToken cancellationToken = default)
    {
        await foreach(var boxes in ProcessFileAsync(fileDir))
        {
            var boxEntities = _mapper.Map<List<BoxEntity>>(boxes);
            var boxesAlreadyInDb = await _dataContext.Boxes.Where(b => boxEntities.Select(be => be.Identifier).Contains(b.Identifier)).AsNoTracking().ToListAsync();
            if (boxesAlreadyInDb != null && boxesAlreadyInDb.Count != 0)
            {
                // There is no way update based on an alternate key -_- 
                var boxesToUpdate = boxEntities.Where(b => boxesAlreadyInDb.Select(b => b.Identifier).Contains(b.Identifier)).ToList();
                foreach(var b in boxesToUpdate)
                {
                    b.Id = boxesAlreadyInDb.First(bx => bx.Identifier == b.Identifier).Id;
                }
                _dataContext.UpdateRange(boxesToUpdate);
                boxEntities = boxEntities.Where(b => !boxesToUpdate.Select(bx => bx.Identifier).Contains(b.Identifier)).ToList();
            }
            if (boxEntities != null && boxEntities.Count != 0)
            {
                _dataContext.AddRange(boxEntities);
            }
            await _dataContext.SaveChangesAsync();
            _dataContext.ChangeTracker.Clear(); // if the file is big then we ll have lot of elements
        }

        return _isFileValid;
    }

    /// <summary>
    /// Process the file in chunks and returns all the boxes and their lines in each chunk.
    /// Ignores the last box in the chunk, in case the lines continue in the next chunk
    /// If we get a 1TB file with 1 box and lot of elements, this method will fill all that in the memory since the last box gets transferred to the next iteration.
    /// But 
    ///     1). I don't think that would happen realistically 
    ///     2). if it did, then each line in the document should have a box identifier
    ///     3). PO number is probably that identifier but I already assumed that it is not and wrote all this code :D
    /// </summary>
    /// <param name="fileDir"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async IAsyncEnumerable<List<IUnique>> ProcessFileAsync(string fileDir)
    {
        using var fs = new FileStream(fileDir, FileMode.Open);
        var buffer = new byte[_chunkSize];
        BoxDTO? box = null;
        while (await fs.ReadAsync(buffer, 0, buffer.Length) > 0)
        {
            var data = Encoding.Default.GetString(buffer).TrimEnd('\0');
            var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var boxes = new List<IUnique>();
            // last box from last chunk
            if (box != null)
                boxes.Add(box);
            foreach (var line in lines)
            {
                var elements = line.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                try
                {
                    var elem = ProcessLine(elements);
                    if (elem is BoxDTO be)
                    {
                        box = be;
                        boxes.Add(box);
                    }
                    if (box == null)
                    {
                        // this can only happen if the very first line in the document is a line, which should not happen
                        _isFileValid = false;
                        _logger.LogError("something went wrong, first line in the document is probably not a box");
                        yield break;
                    }
                    if (elem is ItemDTO ie)
                    {
                        box.Items.Add(ie);
                    }
                }
                catch (FormatException ex)
                {
                    // print the error and process rest of the lines
                    // but only if a known exception occurs, if something random happens and we skip it, lines might end up in wrong boxes
                    _isFileValid = false;
                    _logger.LogError(ex.Message);
                    continue;
                }
            }
            // From the file it seems like each box has seperate PO but in the explanation it wasn't specified so i went for something that can handle the cases where PO is different
            // for each item (e.g. one box contains items for two different orders)
            // So, save last box for the next iteration, for the cases where items of one box gets split between two chunks.
            boxes.Remove(box);
            yield return boxes;
        }
        yield return new List<IUnique>() { box };
    }
    private IUnique? ProcessLine(string[] lineElements)
    {
        if (lineElements[0] == "HDR" && lineElements.Length == 3)
        {
            return new BoxDTO(lineElements[1], lineElements[2]);
        }
        
        if (lineElements[0] == "LINE" && lineElements.Length == 4)
        {
            return new ItemDTO(long.Parse(lineElements[2]), lineElements[1], int.Parse(lineElements[3]));
        }

        // after processing CHUNKS_TO_READ sized chunk from the file, sometimes a line gets cut in half
        // e.g. LINE P000001661         9781465121550 --> next chunk -->         12
        // cutUpLine will saves cut up line
        // if there is no cut up line from last chunk and our current line isn't a full box or item, then the current line is just invalid
        if (_cutUpLine == null && !(lineElements[0] != "HDR" && lineElements.Length <= 3) && !(lineElements[0] != "LINE" && lineElements.Length <= 4))
        {
            throw new FormatException($"Line is invalid: {string.Join(" ", lineElements)}");
        }
        
        // if cutUpLine is null, then we don't have a cut up line from previous chunk, so we just save it to continue on the next line
        if (_cutUpLine == null)
        {
            _cutUpLine = [.. lineElements];
            return null;
        }

        // cutUpLine exists, so we are processing rest of the last line from previous line
        _cutUpLine.AddRange(lineElements);
        IUnique? result = null;
        if (_cutUpLine[0] == "HDR")
        {
            result = new BoxDTO(_cutUpLine[1], _cutUpLine[2]);
        }

        if (_cutUpLine[0] == "LINE")
        {
            result = new ItemDTO(long.Parse(_cutUpLine[2]), _cutUpLine[1], int.Parse(_cutUpLine[3]));
        }
        _cutUpLine = null; // reset the cutup line
        return result;
    }
}
