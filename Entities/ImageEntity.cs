using Survivor.Models;
using System.ComponentModel.DataAnnotations;

namespace Survivor.Entities
{
    public class ImageEntity
    {
        public string UserName { get; internal set; }

        public string Id { get; internal set; }

        [MinLength(3)]
        public string Name { get; set; }

        public string BlobUrl { get; internal set; }

        public string UploadSasToken { get; internal set; }

        public string DownloadUrl { get; internal set; }

        public ImageModel ToModel()
        {
            return new ImageModel()
            {
                Name = this.Name
            };
        }
    }
}
