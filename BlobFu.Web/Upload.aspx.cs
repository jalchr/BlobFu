using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BlobFu.Web
{
    public partial class Upload : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack && Request.Files.Keys.Count > 0)
            {
                BlobFuHelper.SaveFileToBlobStorage(
                    new BlobStorageRequest
                    {
                        ConnectionStringName = "Blobs",
                        StreamOfDataToStore = Request.Files[0].InputStream,
                        Container = "FileUploads",
                        BlobSavedCallback = (x) =>
                            {
                                link.Text =
                                    string.Format("File uploaded to {0}", 
                                        x.AbsoluteUri);
                                link.NavigateUrl = x.AbsoluteUri;
                                new BlobFuService("Blobs").BlobExists(x.AbsoluteUri).ToString();
                            },
                        Filename = Path.GetFileName(Request.Files[0].FileName)
                    });
            }
        }
    }
}