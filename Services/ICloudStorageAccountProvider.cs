using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlobCloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;
using TableCloudStorageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount;

namespace Survivor.Services
{
    public interface ICloudStorageAccountProvider
    {
        BlobCloudStorageAccount BlobStorageAccount { get; }

        TableCloudStorageAccount TableStorageAccount { get; }
    }
}
