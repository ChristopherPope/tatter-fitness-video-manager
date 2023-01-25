using MetadataExtractor;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using TatterFitness.Bll.Interfaces.Services;
using TatterFitness.Dal.Entities;
using TatterFitness.VideoManager.Utils;
using Directory = System.IO.Directory;

namespace TatterFitness.VideoManager.Operators
{
    internal class VideoImporter
    {
        private readonly IVideoService videoSvc;
        private readonly TatterFitConfiguration config;
        static string[] months = {
            "xxx",
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sep",
            "Oct",
            "Nov",
            "Dec"
        };

        public VideoImporter(IVideoService videoSvc, IOptions<TatterFitConfiguration> options)
        {
            this.videoSvc = videoSvc;
            config = options.Value;
        }

        public void ImportExportedVideos()
        {
            var videoFilePaths = Directory.GetFiles(config.ExportedVideosDirectory);
            var videoNum = 1;
            foreach (var videoFilePath in videoFilePaths)
            {
                Console.WriteLine($"Importing video {videoNum++} of {videoFilePaths.Length}...");
                var workoutExerciseId = GetWorkoutExerciseId(videoFilePath);
                var videoData = File.ReadAllBytes(videoFilePath);
                var video = new VideoEntity
                {
                    WorkoutExerciseId = workoutExerciseId,
                    VideoData = videoData,
                    Hash = MD5.Create().ComputeHash(videoData)
                };

                if (videoSvc.DoesVideoExist(video.Hash))
                {
                    continue;
                }

                videoSvc.Create(video);
            }
        }

        public DateTime? GetCreatedDate(string videoFilePath) // todo: this is a duplicate of what is in VideoFile
        {
            DateTime? created = null;
            var directories = ImageMetadataReader.ReadMetadata(videoFilePath);
            var movieHeader = directories.FirstOrDefault(d => d.Name == "QuickTime Movie Header");
            if (movieHeader == null)
            {
                return created;
            }

            var createdTag = movieHeader.Tags.FirstOrDefault(t => t.Name == "Created");
            if (createdTag == null)
            {
                return created;
            }

            var createdText = createdTag.Description;
            if (createdText == null)
            {
                return created;
            }

            return ParseCreatedText(createdText);
        }

        private int GetWorkoutExerciseId(string videoFilePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(videoFilePath);
            var idx = fileName.IndexOf('-');
            var weIdText = fileName.Substring(0, idx);

            return int.Parse(weIdText);
        }

        private DateTime ParseCreatedText(string createdText)
        {
            var monText = createdText.Substring(4, 3);
            var month = Array.IndexOf(months, monText);

            var dayText = createdText.Substring(8, 2);
            var day = int.Parse(dayText);

            var yearText = createdText.Substring(20, 4);
            var year = int.Parse(yearText);

            return new DateTime(year, month, day);
        }
    }
}
