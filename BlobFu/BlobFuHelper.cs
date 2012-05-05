using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlobFu
{
    public static class BlobFuHelper
    {
        public static void SaveFileToBlobStorage(
            BlobStorageRequest request)
        {
            new BlobFuService(request.ConnectionStringName)
                .SaveToBlobStorage(request);
        }

        public static void ListBlobs(
            BlobListRequest request)
        {
            new BlobFuService(request.ConnectionStringName)
                .ListBlobs(request);
        }
    }
}
