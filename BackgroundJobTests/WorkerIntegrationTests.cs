using AutoMapper;
using BackgroundJob;
using Data;
using Data.AutomapperProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Services.BoxProcessing;

namespace BackgroundJobTests;

public class WorkerIntegrationTests
{
    private readonly DataContext _dataContext;
    private readonly ServiceProvider _serviceProvider;
    private Worker _worker;

    public WorkerIntegrationTests()
    {
        //var mockLog = new Mock<ILogger<Worker>>();
        //var serviceScopFactory = new IServiceScopeFactory() ?????? I got stuck here, i have no idea how to instantiate a ServiceScopeFactory without doing that whole builder host stuff in Program.cs
        //var worker = new Worker(mockLog.Object, );
    }

    // WorkerProcessesFiles
    //      run the worker
    //      sleep 5 seconds
    //      stop the worker
    //      check the db to see if its updated

    // TestProcessBoxesProcessesOnlyOneBox

    // TestProcessBoxesProcessesDuplicateBoxes

    // TestProcessBoxesDoesntProcessesInvalidFiles
}