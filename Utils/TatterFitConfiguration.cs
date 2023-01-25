namespace TatterFitness.VideoManager.Utils
{
    internal class TatterFitConfiguration
    {
        public const string TatterFitConfigKey = "TatterFitConfig";

        public string RootVideoDirectory { get; set; } = string.Empty;
        public string NewVidsSourceFolderName { get; set; } = string.Empty;
        public string NewVidsDuplicateFolderName { get; set; } = string.Empty;
        public string NewVidsFailureFolderName { get; set; } = string.Empty;
        public string NewVidsStageFolderName { get; set; } = string.Empty;
        public string NewVidsUploadedFolderName { get; set; } = string.Empty;
        public string ExportedFolderName { get; set; } = string.Empty;

        public string ExportedVideosDirectory => Path.Combine(RootVideoDirectory, ExportedFolderName);
        public string NewVideosSourceDirectory => Path.Combine(RootVideoDirectory, NewVidsSourceFolderName);
        public string NewVideosDuplicateDirectory => Path.Combine(RootVideoDirectory, NewVidsSourceFolderName, NewVidsDuplicateFolderName);
        public string NewVideosFailureDirectory => Path.Combine(RootVideoDirectory, NewVidsSourceFolderName, NewVidsFailureFolderName);
        public string NewVideosStageDirectory => Path.Combine(RootVideoDirectory, NewVidsSourceFolderName, NewVidsStageFolderName);
        public string NewVideosUploadedDirectory => Path.Combine(RootVideoDirectory, NewVidsSourceFolderName, NewVidsUploadedFolderName);
    }
}



