# TatterFitness Video Manager
This .Net Core command line app allows me to:
- Import new videos and associate them with workout exercises.
- Export videos to the file system.
- Import the exported videos back into the ***Videos*** table.
- Check for duplicate videos.

<br>
 
- [TatterFitness Video Manager](#tatterfitness-video-manager)
- [appsettings.json](#appsettingsjson)
- [Import New Videos](#import-new-videos)

<br>

# appsettings.json
    "TatterFitConfig": {
        "RootVideoDirectory": "C:\\Users\\Chris\\OneDrive\\Videos\\TatterFitness",
        "NewVidsSourceFolderName": "NewVideos",
        "NewVidsDuplicateFolderName": "Duplicate",
        "NewVidsFailureFolderName": "Failure",
        "NewVidsStageFolderName": "Stage",
        "NewVidsUploadedFolderName": "Uploaded",
        "ExportedFolderName": "Exported"
    },


# Import New Videos
Scans a folder (from the appsettings.json)