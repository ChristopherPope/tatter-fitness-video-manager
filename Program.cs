using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TatterFitness.Bll.Interfaces.Services;
using TatterFitness.Bll.Mapping;
using TatterFitness.Bll.Services;
using TatterFitness.Dal.Interfaces.Persistance;
using TatterFitness.Dal.Persistence;
using TatterFitness.VideoManager.Operators;
using TatterFitness.VideoManager.Utils;
using VideoManager;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
hostBuilder
    .ConfigureAppConfiguration(configBuilder =>
    {
        configBuilder
            .AddJsonFile($"appsettings.json", true, true)
#if DEBUG
            .AddJsonFile("appsettings.development.json", true, true)
#else
            .AddJsonFile("appsettings.production.json", true, true)
#endif
            .AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;
        _ = services
            .AddAutoMapper(typeof(ModelMapping))
            .AddDbContext<TatterDb>(options => options.UseSqlServer(config.GetConnectionString("TatterFitnessDb"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)))
            .Configure<TatterFitConfiguration>(config.GetSection(TatterFitConfiguration.TatterFitConfigKey))
            .AddScoped<NewVideoImporter>()
            .AddScoped<VideoExporter>()
            .AddScoped<VideoImporter>()
            .AddScoped<DupChecker>()
            .AddScoped<IVideoService, VideoService>()
            .AddScoped<IHistoriesService, HistoriesService>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddHostedService<OperationDirector>();
    })
    .Build()
    .Run();



