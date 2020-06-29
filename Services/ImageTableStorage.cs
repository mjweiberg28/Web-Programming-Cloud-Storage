using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Survivor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Survivor.Services
{
    public class ImageTableStorage : IImageTableStorage
    {
        private readonly ICloudStorageAccountProvider cloudStorageAccountProvider;

        private readonly IUserNameProvider userNameProvider;

        private readonly CloudBlobContainer cloudBlobContainer;

        private readonly CloudTable cloudTable;

        public ImageTableStorage(ICloudStorageAccountProvider cloudStorageAccountProvider, IUserNameProvider userNameProvider)
        {
            this.cloudStorageAccountProvider = cloudStorageAccountProvider;
            this.userNameProvider = userNameProvider;

            var blobClient = this.cloudStorageAccountProvider.BlobStorageAccount.CreateCloudBlobClient();
            this.cloudBlobContainer = blobClient.GetContainerReference(this.userNameProvider.UserName);

            var tableClient = this.cloudStorageAccountProvider.TableStorageAccount.CreateCloudTableClient();
            this.cloudTable = tableClient.GetTableReference(this.userNameProvider.UserName);
        }

        public async Task StartupAsync()
        {
            await this.cloudBlobContainer.CreateIfNotExistsAsync();
            await this.cloudTable.CreateIfNotExistsAsync();
        }

        public string GetStorageAccountBlobUrl()
        {
            return this.cloudStorageAccountProvider.BlobStorageAccount.BlobStorageUri.PrimaryUri.ToString();
        }

        public async Task<ImageModel> GetAsync(string id)
        {
            TableResult tableResult = await cloudTable.ExecuteAsync(TableOperation.Retrieve<ImageModel>(this.userNameProvider.UserName, id));
            return (ImageModel)tableResult.Result;
        }

        public async Task<ImageModel> AddOrUpdateAsync(ImageModel imageModel)
        {
            if (string.IsNullOrWhiteSpace(imageModel.Id))
            {
                imageModel.Id = Guid.NewGuid().ToString();
                imageModel.UserName = this.userNameProvider.UserName;
            }

            await cloudTable.ExecuteAsync(TableOperation.InsertOrReplace(imageModel));
            return imageModel;
        }

        public string GetUploadSas(string id)
        {
            return this.cloudBlobContainer.GetBlobReference(id).GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(8),
                Permissions = SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
            });
        }

        public string GetDownloadSas(string id)
        { 
            return this.cloudBlobContainer.GetBlobReference(id).GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(8),
                Permissions = SharedAccessBlobPermissions.Read
            });
        }

        public string GetDownloadUrl(string id)
        {
            return this.cloudBlobContainer.GetBlobReference(id).StorageUri.PrimaryUri.ToString();
        }

        public async Task<List<ImageModel>> GetAllImagesAsync()
        {
            var imageTableResults = new List<ImageModel>();

            TableQuery<ImageModel> tableQuery = new TableQuery<ImageModel>();

            TableContinuationToken continuationToken = null;

            do
            {
                TableQuerySegment<ImageModel> tableQueryResult =
                    await this.cloudTable.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);

                continuationToken = tableQueryResult.ContinuationToken;

                imageTableResults.AddRange(tableQueryResult.Results.Where(result => result.UploadComplete));
            } while (continuationToken != null);

            return imageTableResults;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var imageTableEntity = await GetAsync(id);
            if (imageTableEntity == null)
            {
                return false;
            }
            await this.cloudTable.ExecuteAsync(TableOperation.Delete(imageTableEntity));
            return true;
        }

        public async Task PurgeAsync()
        {
            var imageTableResults = new List<ImageModel>();

            TableQuery<ImageModel> tableQuery = new TableQuery<ImageModel>();
            TableContinuationToken tableContinuationToken = null;

            do
            {
                TableQuerySegment<ImageModel> tableQueryResult =
                    await this.cloudTable.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken);

                tableContinuationToken = tableQueryResult.ContinuationToken;

                var tasks = tableQueryResult.Results.Select(async result => await this.cloudTable.ExecuteAsync(TableOperation.Delete(result)));
                await Task.WhenAll(tasks);
            } while (tableContinuationToken != null);

            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var blobs = await this.cloudBlobContainer.ListBlobsSegmentedAsync(blobContinuationToken);

                blobContinuationToken = blobs.ContinuationToken;

                var tasks = blobs.Results.Select(async result => await ((CloudBlob)result).DeleteAsync());
                await Task.WhenAll(tasks);
            } while (blobContinuationToken != null);
        }
    }
}
