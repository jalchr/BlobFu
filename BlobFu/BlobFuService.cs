using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using Microsoft.WindowsAzure;
using System.Configuration;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace BlobFu
{
    public class BlobFuService
    {
        private CloudBlobContainer _container;
        private string _connectionString = "";

        public BlobFuService(string connectionStringName)
        {
            _connectionString = RoleEnvironment.IsAvailable
                ? RoleEnvironment.GetConfigurationSettingValue(connectionStringName)
                : ConfigurationManager.AppSettings[connectionStringName];
        }

        private void VerifyContainer(string container)
        {
            container = container.ToLower();

            _container =
                CloudStorageAccount
                    .Parse(_connectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(container);

            _container.CreateIfNotExist();

            _container.SetPermissions(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
        }

        public BlobFuService ListBlobs(BlobListRequest request)
        {
            VerifyContainer(request.Container);

            var blobs = new List<string>();

            blobs.AddRange(
                this._container.ListBlobs().Select(x =>
                    x.Uri.AbsoluteUri));

            if (request.ListReceivedCallback != null)
                request.ListReceivedCallback(blobs);

            return this;
        }

        public BlobFuService SaveToBlobStorage(BlobStorageRequest request)
        {
            VerifyContainer(request.Container);

            if ((request.StreamOfDataToStore != null && request.StreamOfDataToStore.Length > 0)
                && (request.DataToStore != null && request.DataToStore.Length > 0))
                throw new ApplicationException("Don't pass in a stream AND a byte array, I don't know which one to save");

            if ((request.DataToStore != null && request.DataToStore.Length > 0) &&
                (request.StreamOfDataToStore == null || request.StreamOfDataToStore.Length == 0))
            {
                request.StreamOfDataToStore = new MemoryStream(request.DataToStore);
            }

            CloudBlob blob = this._container.GetBlobReference(request.Filename);
            blob.UploadFromStream(request.StreamOfDataToStore);

            if (request.BlobSavedCallback != null)
                request.BlobSavedCallback(blob.Uri);

            return this;
        }

        public BlobFuService DeleteBlob(string fileName)
        {
            CloudBlob blob = this._container.GetBlobReference(fileName);
            blob.Delete();
            return this;
        }

        public BlobFuService ListContainers(Action<List<string>> containersFoundCallback)
        {
            if (containersFoundCallback != null)
                containersFoundCallback(
                    CloudStorageAccount
                        .Parse(_connectionString)
                        .CreateCloudBlobClient()
                        .ListContainers()
                        .Select(x => x.Name)
                            .ToList());

            return this;
        }
    }
}
