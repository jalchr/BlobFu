using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BlobFu
{
    public class BlobStorageRequest : BlobFuRequest
	{
		public string Filename { get; set; }
		public string Container { get; set; }
		public byte[] DataToStore { get; set; }
		public Stream StreamOfDataToStore { get; set; }
		public Action<Uri> BlobSavedCallback { get; set; }
	}
}
