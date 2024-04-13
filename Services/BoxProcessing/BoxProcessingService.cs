using Microsoft.Extensions.Logging;

namespace Services.BoxProcessing;

public class BoxProcessingService : IBoxProcessingService
{
    private readonly ILogger<BoxProcessingService> _logger;

    public BoxProcessingService(ILogger<BoxProcessingService> logger)
    {
        _logger = logger;
    }
    public async Task<bool> ProcessBoxes(string fileDir, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Eat shit");
        return false;
        //throw new NotImplementedException();
    }
}
