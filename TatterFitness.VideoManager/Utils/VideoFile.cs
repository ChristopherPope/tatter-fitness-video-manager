using MetadataExtractor;

namespace TatterFitness.VideoManager.Utils
{
    internal class VideoFile
    {
        private readonly string filePath;
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

        public VideoFile(string filePath)
        {
            this.filePath = filePath;
        }

        public DateTime? GetCreatedDate()
        {
            DateTime? noCreationDate = null;
            var directories = ImageMetadataReader.ReadMetadata(filePath);
            var movieHeader = directories.FirstOrDefault(d => d.Name == "QuickTime Movie Header");
            if (movieHeader == null)
            {
                return noCreationDate;
            }

            var createdTag = movieHeader.Tags.FirstOrDefault(t => t.Name == "Created");
            if (createdTag == null || createdTag.Description == null)
            {
                return noCreationDate;
            }

            var createdDate = ParseCreatedText(createdTag.Description);
            if (createdDate.Year < 2020)
            {
                return noCreationDate;
            }

            return createdDate;
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
