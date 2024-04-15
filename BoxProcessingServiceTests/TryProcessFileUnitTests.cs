using AutoMapper;
using Data;
using Data.AutomapperProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Services.BoxProcessing;

namespace BoxProcessingServiceTests;

public class TryProcessFileUnitTests
{
    private readonly IBoxProcessingService _boxProcessingService;
    private readonly DataContext _dataContext;
    private FileInfo? _fileToProcess;

    public TryProcessFileUnitTests()
    {
        var testMapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new BoxMapperProfile());
            cfg.AddProfile(new itemMapperProfile());
        });
        var mapper = testMapperConfig.CreateMapper();

        var testContextOptions = new DbContextOptionsBuilder<DataContext>()
                                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                                    .Options;
        _dataContext = new DataContext(testContextOptions);
        var mockLog = new Mock<ILogger<BoxProcessingService>>();
        var mockConfig = new Mock<IConfiguration>();
        _boxProcessingService = new BoxProcessingService(mockLog.Object, _dataContext, mapper, mockConfig.Object);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _dataContext.Dispose();
    }

    [SetUp]
    public void Setup()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var toProcessDir = new DirectoryInfo(Path.Join(currentDir, "toProcess"));
        var fileList = toProcessDir.GetFiles("*.*", SearchOption.AllDirectories);
        _fileToProcess = fileList.Where(f => f.Extension == ".txt").FirstOrDefault();
    }

    [Test]
    public async Task TestProcesBoxesProcesses()
    {
        Assert.That(_fileToProcess, Is.Not.Null);
        
        var fileIsValid = await _boxProcessingService.TryProcessBoxes(_fileToProcess.FullName);
        Assert.That(fileIsValid, Is.True);

        var boxIdentifiers = await _dataContext.Boxes.Select(b => b.Identifier).ToListAsync();
        Assert.That(boxIdentifiers, Is.EqualTo(new List<string>() { "6874453I", "6874454I" }));

        var isbns = await _dataContext.Items.Select(i => i.ISBN).ToListAsync();
        Assert.That(isbns, Is.EqualTo(new List<long>() { 9781473663800, 9781473662179 }));
    }

    // TestProcessBoxesProcessesOnlyOneBox

    // TestProcessBoxesProcessesDuplicateBoxes

    // TestProcessBoxesDoesntProcessesInvalidFiles
}