using Services.BoxProcessing;

namespace BackgroundJob;

public class Worker : BackgroundService
{
    private readonly string _toProcessDirname;
    private readonly string _processedDirname;
    private readonly string _invalidFilesDirname;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _toProcessDirname = configuration["TO_PROCESS_DIRNAME"] ?? "toProcess";
        _processedDirname = configuration["PROCESSED_DIRNAME"] ?? "processed";
        _toProcessDirname = configuration["INVALID_FILES_DIRNAME"] ?? "invalidFiles";
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
        var toProcessDir = new DirectoryInfo(Path.Join(currentDir, _toProcessDirname));
        var fileList = toProcessDir.GetFiles("*.*", SearchOption.AllDirectories);

        var invalidFiles = fileList.Where(f => f.Extension != ".txt");
        if (invalidFiles.Any())
        {
            foreach(var invalidFile in invalidFiles)
            {
                MoveFileToProcessingDirs(invalidFile, _invalidFilesDirname);
            }
            throw new ApplicationException($"Invalid files in {toProcessDir}. Moved them to {Path.Join(currentDir + _invalidFilesDirname)}");
        }

        var fileToProcess = fileList.Where(f => f.Extension == ".txt").FirstOrDefault();
        
        if(fileToProcess != null)
        {
            Console.WriteLine($"******************Processing {fileToProcess.FullName}******************");
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBoxProcessingService>();
            var fileIsValid = await service.TryProcessBoxes(fileToProcess.FullName, cancellationToken);
            _logger.LogInformation($"Finished Processing {fileToProcess.FullName}.");
            if(!fileIsValid)
            {
                // move invalid files to invalidFiles folder
                // Keep invalid files so later it can be looked at in UI or something
                MoveFileToProcessingDirs(fileToProcess, _invalidFilesDirname);
                _logger.LogWarning($"file {fileToProcess.Name} had invalid lines that were not imported. The file was moved to {_invalidFilesDirname}");
            }
            else
            {
                // move the file to processed folder after finishing work
                // I just copy them so i can re-use them again
                // irl i wouldn't keep copy of the processed files unless specified that we want to keep them
                MoveFileToProcessingDirs(fileToProcess, _processedDirname);
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
