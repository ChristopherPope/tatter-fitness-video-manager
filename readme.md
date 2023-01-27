# TatterFitness Video Manager
This .Net Core command line app allows me to:
- Import new videos and associate them with workout exercises.
- Export videos to the file system.
- Import the exported videos back into the ***Videos*** table.
- Check for duplicate videos.

<br>
 
- [TatterFitness Video Manager](#tatterfitness-video-manager)
- [Configuration](#configuration)
  - [appsettings.json](#appsettingsjson)
  - [TatterFitConfiguration class](#tatterfitconfiguration-class)
- [Import New Videos](#import-new-videos)
- [Export Videos](#export-videos)
  - [Filename Convention](#filename-convention)

<br>

# Configuration

## appsettings.json
```
    "TatterFitConfig": {
        "RootVideoDirectory": "C:\\Users\\Chris\\OneDrive\\Videos\\TatterFitness",
        "NewVidsSourceFolderName": "NewVideos",
        "NewVidsDuplicateFolderName": "Duplicate",
        "NewVidsFailureFolderName": "Failure",
        "NewVidsStageFolderName": "Stage",
        "NewVidsUploadedFolderName": "Uploaded",
        "ExportedFolderName": "Exported"
    },
```

## TatterFitConfiguration class
Follows the [Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
```
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
```


# Import New Videos
Scans the **NewVideosSourceDirectory** folder for videos and performs the following actions on each file:
1. Move the file to the **NewVideosStageDirectory**.
2. Verify the video is not a duplicate and if so, move it to the **NewVideosDuplicateDirectory**.
3. Query which workout exercises (e.g. Squat, Bench Press) were created on the video's creation date.
   1. Prompt the user to select the workout exercise.
   2. Insert the ***Videos*** table row.
   3. Move the video file to the **NewVideosUploadedDirectory**.

# Export Videos
Exports the videos in the ***Videos*** table to a file in the **ExportedVideosDirectory**.

## Filename Convention
The videos file names contain the WorkoutExerciseId from the ***WorkoutExercises*** table. However, since multiple videos can be associated with a single WorkoutExercise, a sequence number is added to the filename as well.

For example:
- 6031-1.mp4
- 6032-2.mp4
- 6032-3.mp4