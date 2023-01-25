using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TatterFitness.VideoManager.Operators;

namespace VideoManager
{
    internal class OperationDirector : IHostedService
    {
        private const string MenuOptionImportNewVideos = "A";
        private const string MenuOptionExportVideos = "B";
        private const string MenuOptionImportVideos = "C";
        private const string MenuOptionCheckDups = "D";
        private const string MenuOptionExit = "X";

        private readonly NewVideoImporter newVideoImporter;
        private readonly VideoExporter videoExporter;
        private readonly VideoImporter videoImporter;
        private readonly DupChecker dupChecker;
        private readonly IConfiguration config;

        public OperationDirector(NewVideoImporter newVideoImporter, VideoExporter videoExporter, VideoImporter videoImporter, DupChecker dupChecker, IConfiguration config)
        {
            this.newVideoImporter = newVideoImporter;
            this.videoExporter = videoExporter;
            this.videoImporter = videoImporter;
            this.dupChecker = dupChecker;
            this.config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var dbcs = config.GetConnectionString("TatterFitnessDb");
            Console.WriteLine($"{dbcs} {Environment.NewLine}");
            string menuOption = string.Empty;
            while (menuOption != MenuOptionExit)
            {
                menuOption = ChooseOperation();
                switch (menuOption)
                {
                    case MenuOptionImportNewVideos:
                        newVideoImporter.BeginImport();
                        break;

                    case MenuOptionExportVideos:
                        videoExporter.ExportAllVideos();
                        break;

                    case MenuOptionImportVideos:
                        videoImporter.ImportExportedVideos();
                        break;

                    case MenuOptionCheckDups:
                        dupChecker.CheckForDups();
                        break;
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private string ChooseOperation()
        {
            string menuOption = string.Empty;
            while (! IsValidMenuOption(menuOption))
            {
                Console.WriteLine($@"
====================================================
{MenuOptionImportNewVideos}...Import new videos
{MenuOptionExportVideos}...Export videos
{MenuOptionImportVideos}...Import exported videos
{MenuOptionCheckDups}...Check for duplicate videos

{MenuOptionExit}...Exit
");

                menuOption = Console.ReadKey().KeyChar.ToString();
                Console.WriteLine();
            }

            return menuOption.ToUpper();
        }

        private bool IsValidMenuOption(string menuOption)
        {
            return (menuOption.Equals(MenuOptionImportNewVideos, StringComparison.OrdinalIgnoreCase) ||
                menuOption.Equals(MenuOptionExportVideos, StringComparison.OrdinalIgnoreCase) ||
                menuOption.Equals(MenuOptionImportVideos, StringComparison.OrdinalIgnoreCase) ||
                menuOption.Equals(MenuOptionCheckDups, StringComparison.OrdinalIgnoreCase) ||
                menuOption.Equals(MenuOptionExit, StringComparison.OrdinalIgnoreCase));
        }
    }
}
