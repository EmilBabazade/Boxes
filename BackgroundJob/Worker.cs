using Services.BoxProcessing;

namespace BackgroundJob;

public class Worker : BackgroundService
{
    private const string TO_PROCESS_DIRNAME = "toProcess";
    private const string PROCESSED_DIRNAME = "processed";
    private const string INVALID_FILES_DIRNAME = "invalidFiles";
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessFiles(stoppingToken);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }

    private async Task ProcessFiles(CancellationToken cancellationToken = default)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var toProcessDir = new DirectoryInfo(Path.Join(currentDir, TO_PROCESS_DIRNAME));
        var fileList = toProcessDir.GetFiles("*.*", SearchOption.AllDirectories);

        var invalidFiles = fileList.Where(f => f.Extension != ".txt");
        if (invalidFiles.Any())
        {
            foreach(var invalidFile in invalidFiles)
            {
                MoveFileToProcessingDirs(invalidFile, INVALID_FILES_DIRNAME);
            }
            throw new ApplicationException($"Invalid files in {toProcessDir}. Moved them to {Path.Join(currentDir + INVALID_FILES_DIRNAME)}");
        }

        var fileToProcess = fileList.Where(f => f.Extension == ".txt").FirstOrDefault();
        
        if(fileToProcess != null)
        {
            Console.WriteLine($"********************************Processing {fileToProcess.FullName}********************************");
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBoxProcessingService>();
            var fileIsInvalid = await service.ProcessBoxes(fileToProcess.FullName, cancellationToken);
            if(fileIsInvalid)
            {
                // move invalid files to invalidFiles folder
                // Keep invalid files so later it can be looked at in UI or something
                MoveFileToProcessingDirs(fileToProcess, INVALID_FILES_DIRNAME);
            }
            else
            {
                // move the file to processed folder after finishing work
                // I just copy them so i can re-use them again
                // irl i wouldn't keep copy of the processed files unless specified that we want to keep them
                MoveFileToProcessingDirs(fileToProcess, PROCESSED_DIRNAME);
            }            
        }
    }

    private static void MoveFileToProcessingDirs(FileInfo? invalidFile, string dirName)
    {
        ArgumentNullException.ThrowIfNull(invalidFile);
        var currentDir = Directory.GetCurrentDirectory();
        File.Move(invalidFile.FullName, Path.Join(currentDir, dirName, invalidFile.Name + DateTimeOffset.Now.ToUnixTimeSeconds()));
    }
}
