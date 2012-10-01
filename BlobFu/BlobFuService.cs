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

        public void VerifyContainer(string container)
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

            // http://wely-lau.net/2012/02/26/uploading-big-files-in-windows-azure-blob-storage-with-putlistblock/
            CloudBlockBlob blob = this._container.GetBlockBlobReference(request.Filename.ToLower());
            //blob.Properties.CacheControl = "";
            //blob.SetProperties();

            int maxSize = 32 * 1024 * 1024; // 32 MB
            //int maxSize = 1 * 1024 * 1024; // 1 MB

            if (request.StreamOfDataToStore.Length > maxSize)
            {
                int id = 0;
                long byteslength = request.StreamOfDataToStore.Length;
                int bytesread = 0;
                int index = 0;
                List<string> blocklist = new List<string>();
                int numBytesPerChunk = 1 * 1024 * 1024; //1MB per block
                //int numBytesPerChunk = 50 * 1024; //50KB per block
                byte[] buffer = new byte[numBytesPerChunk];

                do
                {
                    bytesread += request.StreamOfDataToStore.Read(buffer, 0, buffer.Length);

                    string blockIdBase64 = Convert.ToBase64String(System.BitConverter.GetBytes(id));

                    blob.PutBlock(blockIdBase64, new MemoryStream(buffer, true), null);
                    blocklist.Add(blockIdBase64);
                    id++;
                } while (byteslength - bytesread > numBytesPerChunk);

                long final = byteslength - bytesread;
                byte[] finalbuffer = new byte[final];
                bytesread += request.StreamOfDataToStore.Read(finalbuffer, 0, finalbuffer.Length);

                string blockId = Convert.ToBase64String(System.BitConverter.GetBytes(id));
                blob.PutBlock(blockId, new MemoryStream(finalbuffer, true), null);
                blocklist.Add(blockId);

                blob.PutBlockList(blocklist);
            }
            else
                blob.UploadFromStream(request.StreamOfDataToStore);

            if (request.BlobSavedCallback != null)
                request.BlobSavedCallback(blob.Uri);

            return this;
        }

        public bool DeleteBlob(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            var uri = new Uri(fileName);
            var container = uri.Segments[1].TrimEnd('/');
            return DeleteBlob(container, fileName);
        }

        public bool DeleteBlob(string container, string fileName)
        {
            if (!ContainerExists(container)) return false;
            VerifyContainer(container);
            CloudBlob blob = this._container.GetBlobReference(fileName);
            blob.Delete();
            return true;
        }

        public bool BlobExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            var uri = new Uri(fileName);
            var container = uri.Segments[1].TrimEnd('/');
            return BlobExists(container, fileName);
        }
        /// <summary>
        /// Tells whether a file exists on azure storage
        /// </summary>
        /// <param name="container">The container at azure blobs</param>
        /// <param name="fileName">The AbsoluteUri of the file path</param>
        /// <returns></returns>
        public bool BlobExists(string container, string fileName)
        {
            if (!ContainerExists(container)) return false;
            VerifyContainer(container);
            CloudBlob blob = _container.GetBlobReference(fileName);
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// Tells whether a file exists on azure storage
        /// </summary>
        /// <param name="container">The container at azure blobs</param>
        /// <param name="fileName">The AbsoluteUri of the file path</param>
        /// <returns></returns>
        public long GetLength(string container, string fileName)
        {
            if (!ContainerExists(container)) return 0;
            VerifyContainer(container);
            CloudBlob blob = _container.GetBlobReference(fileName);
            try
            {
                blob.FetchAttributes();
                return blob.Properties.Length;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return 0;
                }
                else
                {
                    throw;
                }
            }
        }

        public bool ContainerExists(string containerName)
        {
            var container =
                CloudStorageAccount
                    .Parse(_connectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(containerName);
            try
            {
                container.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
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
