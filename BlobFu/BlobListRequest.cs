using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlobFu
{
    public class BlobListRequest : BlobFuRequest
	{
		public Action<IEnumerable<string>> ListReceivedCallback { get; set; }
	}
}
