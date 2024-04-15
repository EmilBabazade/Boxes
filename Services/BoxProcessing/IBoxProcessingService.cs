namespace Services.BoxProcessing;

public interface IBoxProcessingService
{
    Task<bool> TryProcessBoxes(string fileDir, CancellationToken cancellationToken = default);
}
