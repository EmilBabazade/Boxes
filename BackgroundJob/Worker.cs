namespace BackgroundJob;

public class Worker : BackgroundService
{
    private const string TO_PROCESS_DIRNAME = "/toProcess";
    private const string PROCESSED_DIRNAME = "/processed";
    private const string INVALID_FILES_DIRNAME = "/invalidFiles";
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                processFiles();
                await Task.Delay(1000, stoppingToken);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }

    private void processFiles()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var toProcessDir = new DirectoryInfo(currentDir  + TO_PROCESS_DIRNAME);
        var fileList = toProcessDir.GetFiles("*.*", SearchOption.AllDirectories);

        var invalidFiles = fileList.Where(f => f.Extension != ".txt");
        if (invalidFiles.Any())
        {
            foreach(var invalidFile in invalidFiles)
            {
                File.Move(invalidFile.FullName, currentDir + INVALID_FILES_DIRNAME + "/" + invalidFile.Name);
            }
            throw new ApplicationException($"Invalid files in {toProcessDir}. Moved them to {currentDir + INVALID_FILES_DIRNAME}");
        }

        var fileToProcess = fileList.Where(f => f.Extension == ".txt").FirstOrDefault();
        
        if(fileToProcess != null)
        {
            // TODO: Do Box stuff
            Console.WriteLine($"Processing {fileToProcess.FullName}...");

            // TODO: move the file to processed folder after finishing work
            // irl i wouldn't keep copy of the processed files unless specified that we want to keep them
            // I just copy them so i can re-use them again
            File.Move(fileToProcess.FullName, currentDir + PROCESSED_DIRNAME + "/" +  fileToProcess.Name);

            // TODO: move invalid files to invalidFiles folder
            // Keep invalid files so later it can be looked at (in UI or something)
        }
    }
}
