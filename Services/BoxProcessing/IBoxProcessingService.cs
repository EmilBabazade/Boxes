namespace Services.BoxProcessing;

public interface IBoxProcessingService
{
    Task<bool> ProcessBoxes(string fileDir, CancellationToken cancellationToken = default);
}
