using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using TatterFitness.Bll.Interfaces.Services;
using TatterFitness.Dal.Interfaces.Persistance;
using TatterFitness.VideoManager.Utils;

namespace TatterFitness.VideoManager.Operators
{
    internal class DupChecker
    {
        private readonly IVideoService videoSvc;
        private readonly TatterFitConfiguration config;

        public DupChecker(IVideoService videoSvc, IOptions<TatterFitConfiguration> options)
        {
            this.videoSvc = videoSvc;
            this.config = options.Value;
        }

        public void CheckForDups()
        {
            if (!Initialize())
            {
                return;
            }

            var videoPaths = Directory.GetFiles(config.NewVideosSourceDirectory);
            foreach (var videoPath in videoPaths)
            {
                var videoInfo = new FileInfo(videoPath);
                Console.Write($"Checking {videoInfo.Name}...");
                if (IsDup(videoInfo.Name))
                {
                    MoveDup(videoInfo.Name);
                }
                else
                {
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }

        private bool Initialize()
        {
            if (!Directory.Exists(config.NewVideosSourceDirectory))
            {
                Console.WriteLine($"The video path {config.NewVideosSourceDirectory} does not exist.");
                return false;
            }

            Directory.CreateDirectory(config.NewVideosDuplicateDirectory);

            return true;
        }

        private void MoveDup(string dupFileName)
        {
            Console.WriteLine("Is a DUP, moving it.");
            var sourcePath = Path.Combine(config.NewVideosSourceDirectory, dupFileName);
            var destPath = Path.Combine(config.NewVideosDuplicateDirectory, dupFileName);
            File.Move(sourcePath, destPath);
        }

        private bool IsDup(string videoFileName)
        {
            var videoPath = Path.Combine(config.NewVideosSourceDirectory, videoFileName);
            var videoData = File.ReadAllBytes(videoPath);
            var videoHash = MD5.Create().ComputeHash(videoData);
            return videoSvc.DoesVideoExist(videoHash);
        }
    }
}
