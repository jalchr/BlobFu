using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using Microsoft.WindowsAzure;
using System.Configuration;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Runtime.Serialization.Formatters.Binary;

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

        public BlobFuService Post<T>(SerializedObjectSaveRequest<T> request)
        {
            VerifyContainer(request.Container);

            using (var m = new MemoryStream())
            {
                new BinaryFormatter().Serialize(m, request.ObjectToSave);
                m.Position = 0;

                SaveToBlobStorage(new BlobStorageRequest
                    {
                        ConnectionStringName = request.ConnectionStringName,
                        Container = request.Container,
                        StreamOfDataToStore = m,
                        Filename = request.Filename
                    });
            }

            return this;
        }

        public BlobFuService Get<T>(SerializedObjectGetRequest<T> request)
        {
            VerifyContainer(request.Container);
            var b = _container.GetBlobReference(request.Filename);
            using (var m = new MemoryStream())
            {
                b.DownloadToStream(m);
                m.Position = 0;
                var f = new BinaryFormatter();
                var r = f.Deserialize(m);
                if (r.GetType().Equals(typeof(T)))
                {
                    if (request.ObjectFoundCallback != null)
                        request.ObjectFoundCallback((T)r);
                }
            }

            return this;
        }
    }
}
