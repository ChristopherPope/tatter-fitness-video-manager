using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using TatterFitness.Bll.Interfaces.Services;
using TatterFitness.Dal.Entities;
using TatterFitness.Models.Workouts;
using TatterFitness.VideoManager.Utils;

namespace TatterFitness.VideoManager.Operators
{
    internal class NewVideoImporter
    {
        private readonly IVideoService videosSvc;
        private readonly IHistoriesService historiesSvc;
        private readonly TatterFitConfiguration config;
        private const string ExitDisposition = "X";
        private const string DisposeDisposition = "D";
        private string? previousDateText = string.Empty;

        public NewVideoImporter(IVideoService videosSvc, IHistoriesService historiesSvc, IOptions<TatterFitConfiguration> options)
        {
            config = options.Value;
            this.videosSvc = videosSvc;
            this.historiesSvc = historiesSvc;
        }

        public void BeginImport()
        {
            if (!Initialize())
            {
                return;
            }

            var videoPaths = Directory.GetFiles(config.NewVideosSourceDirectory);
            var videoNum = 0;
            var totalVideos = videoPaths.Length;
            foreach (var path in videoPaths)
            {
                videoNum++;
                EnsureStageFolderIsEmpty();
                var videoStagePath = MoveVideoFile(path, config.NewVideosStageDirectory);
                var video = CreateNewVideo(videoStagePath);
                if (!VerifyVideoIsNotADuplicate(video, videoStagePath))
                {
                    continue;
                }

                var mediaCreatedDate = GetVideoCreatedDate(videoStagePath);
                var videoWorkoutExercises = ReadWorkoutExercisesOnCreatedDate(mediaCreatedDate).ToList();
                var dispositionOption = ChooseVideoStageDisposition(videoWorkoutExercises, videoNum, totalVideos);
                if (IsChosenOptionToExit(dispositionOption))
                {
                    return;
                }

                if (IsChosenOptionToDispose(dispositionOption))
                {
                    File.Delete(videoStagePath);
                    continue;
                }

                var weIdx = int.Parse(dispositionOption);
                video.WorkoutExerciseId = videoWorkoutExercises[weIdx].Id;
                videosSvc.Create(video);
                MoveVideoFile(videoStagePath, config.NewVideosUploadedDirectory);
            }

            Console.WriteLine("Completed.");
        }

        private IEnumerable<WorkoutExercise> ReadWorkoutExercisesOnCreatedDate(DateTime videoCreatedDate)
        {
            var dateRange = new WorkoutDateRange { InclusiveFrom = videoCreatedDate, InclusiveTo = videoCreatedDate };
            var workouts = historiesSvc.ReadWorkouts(dateRange);

            return workouts.SelectMany(w => w.Exercises);
        }

        private void EnsureStageFolderIsEmpty()
        {
            while (Directory.GetFiles(config.NewVideosStageDirectory).Length > 0)
            {
                Console.WriteLine("Please empty the stage folder and then press ENTER to continue...");
                Console.ReadLine();
            }
        }

        private void EnsureSatteliteFoldersArePresent()
        {
            Directory.CreateDirectory(config.NewVideosUploadedDirectory);
            Directory.CreateDirectory(config.NewVideosStageDirectory);
            Directory.CreateDirectory(config.NewVideosDuplicateDirectory);
            Directory.CreateDirectory(config.NewVideosFailureDirectory);
        }

        private bool Initialize()
        {
            if (!Directory.Exists(config.NewVideosSourceDirectory))
            {
                Console.WriteLine($"The folder {config.NewVideosSourceDirectory} does not exist.");
                return false;
            }

            EnsureSatteliteFoldersArePresent();

            return true;
        }

        private string MoveVideoFile(string videoPath, string destFolderPath)
        {
            var videoFile = new FileInfo(videoPath);
            var destPath = Path.Combine(destFolderPath, videoFile.Name);
            File.Move(videoFile.FullName, destPath, true);

            return destPath;
        }

        private string ChooseVideoStageDisposition(IEnumerable<WorkoutExercise> videoStageWorkoutExercises, int videoNum, int totalVideos)
        {
            var chosenOption = string.Empty;
            while (!IsValidateDispositionOption(chosenOption, videoStageWorkoutExercises.Count()))
            {
                Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}{'='.ToString().PadRight(80, '=')}{Environment.NewLine}Choose what you want to do with the video {videoNum} of {totalVideos}:");

                var optionNum = 1;
                foreach (var videoWorkoutExercise in videoStageWorkoutExercises)
                {
                    var dateText = videoWorkoutExercise.WorkoutDate.ToString("yyyy-MM-dd HH:mm");
                    Console.WriteLine($"{optionNum++} - {videoWorkoutExercise.ExerciseName} - {dateText}");
                }

                Console.WriteLine($"{Environment.NewLine}{DisposeDisposition} - Dispose");
                Console.WriteLine($"{Environment.NewLine}{ExitDisposition} - Exit");
                Console.WriteLine("");
                Console.Write("Please choose: ");

                chosenOption = Console.ReadKey().KeyChar.ToString();

                if (int.TryParse(chosenOption, out int option))
                {
                    option--;
                    chosenOption = option.ToString();
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            return chosenOption;
        }

        private bool IsChosenOptionToExit(string chosenOption)
        {
            if (string.Equals(chosenOption, ExitDisposition, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private bool IsChosenOptionToDispose(string chosenOption)
        {
            if (string.Equals(chosenOption, DisposeDisposition, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private bool IsValidateDispositionOption(string chosenOption, int numExercises)
        {
            if (IsChosenOptionToExit(chosenOption) || IsChosenOptionToDispose(chosenOption))
            {
                return true;
            }

            if (!int.TryParse(chosenOption, out var chosenNumber))
            {
                return false;
            }

            var idx = chosenNumber--;
            if (idx >= 0 && idx <= numExercises)
            {
                return true;
            }

            return false;
        }

        private bool VerifyVideoIsNotADuplicate(VideoEntity video, string videoStagePath)
        {
            if (! videosSvc.DoesVideoExist(video.Hash))
            {
                return true;
            }

            Console.WriteLine($"This video is a duplicate and will be moved to {config.NewVideosDuplicateDirectory}... ");
            MoveVideoFile(videoStagePath, config.NewVideosDuplicateDirectory);

            return false;
        }

        private DateTime GetVideoCreatedDate(string videoStagePath)
        {
            var videoFile = new VideoFile(videoStagePath);
            var createdDate = videoFile.GetCreatedDate();
            if (createdDate == null)
            {
                createdDate = RequestVideoCreatedDate();
            }

            return createdDate.Value.Date;
        }

        public VideoEntity CreateNewVideo(string videoStagePath)
        {
            byte[] videoData = File.ReadAllBytes(videoStagePath);
            var videoHash = MD5.Create().ComputeHash(videoData);
            var newVideo = new VideoEntity()
            {
                VideoData = videoData,
                Hash = MD5.Create().ComputeHash(videoData)
            };

            return newVideo;
        }

        private DateTime RequestVideoCreatedDate()
        {
            var createdDate = DateTime.MinValue;
            while (createdDate == DateTime.MinValue)
            {
                Console.Write($"Enter the video create date: {previousDateText}");
                var input = Console.ReadLine();
                if (! string.IsNullOrEmpty(input))
                {
                    previousDateText = input;
                }

                if (DateTime.TryParse(previousDateText, out createdDate))
                {
                    break;
                }
            }

            return createdDate;
        }
    }
}
