using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlobFu
{
    public class BlobListRequest : BlobFuRequest
	{
		public string Container { get; set; }
		public Action<IEnumerable<string>> ListReceivedCallback { get; set; }
	}
}
