using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlobFu
{
    public class SerializedObjectGetRequest<T> : BlobFuRequest
    {
        public Action<T> ObjectFoundCallback { get; set; }
        public string Filename { get; set; }
    }
}
