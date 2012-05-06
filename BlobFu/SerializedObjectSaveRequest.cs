using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlobFu
{
    public class SerializedObjectSaveRequest<T> : BlobFuRequest
    {
        public T ObjectToSave { get; set; }
        public string Filename { get; set; }
    }
}
