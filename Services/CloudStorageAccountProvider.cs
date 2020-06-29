using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlobCloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;
using TableCloudStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount;

namespace Survivor.Services
{
    public class CloudStorageAccountProvider : ICloudStorageAccountProvider
    {
        private string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=webprogramming;AccountKey=RJfJLANw7vkbug8t1i+Alf7pQuubhsuTHgX8MVPnkJ/AgSBtksP8FBR3eKMwU28dBteNmHzmUnNZcrPF+QWRdw==;EndpointSuffix=core.windows.net";

        public BlobCloudStorageAccount BlobStorageAccount => BlobCloudStorageAccount.Parse(ConnectionString);

        public TableCloudStorageAccount TableStorageAccount => TableCloudStorageAccount.Parse(ConnectionString);
    }
}
