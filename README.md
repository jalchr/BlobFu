<a name="blobfu" />
# BlobFu #

<a name="windows-azure-blob-storage-fluent-wrapper" />
## Windows Azure Blob Storage Fluent Wrapper ##

A library that makes it easy via a Fluent interface, to interact with Windows Blob storage. It gives you a very basic start to storing binary blobs in the Windows Azure cloud. 

<a name="what-does-blobfu-do" />
### What does BlobFu Do? ###

Here's the current set of functionality, demonstrated by an NUnit output of the unit tests used to design BlobFu. Note, more tests may be added as the project evolves. 

![BlobFu Unit Test Run](https://github.com/bradygaster/BlobFu/blob/master/Images/blobfu-unit-test-run.png?raw=true "BlobFu Unit Test Run")

<a name="using-blobfu-within-aspnet" />
### Using BlobFu Within ASP.NET ###
Here's the Hello World example to demonstrate one of the best uses for Windows Azure Blob Storage - capturing file uploads. BlobFu makes this pretty simple. 

<a name="step-1" />
#### Step 1 - Configure the Windows Azure Blob Storage Connection String####
Add an application or web configuration setting with the connection string you'll be using that points to your Windows Azure storage account, as shown below. 

![Configuring a site or app with the blob connection string](https://github.com/bradygaster/BlobFu/blob/master/Images/configuring-a-site-or-app-with-the-blob-conne.png?raw=true "Configuring a site or app with the blob connection string")

Note: In this, the local storage account will be used, so make sure you're running your local storage emulator in this example.

![running the storage emulator](https://github.com/bradygaster/BlobFu/blob/master/Images/running-the-storage-emulator.png?raw=true "running the storage emulator")

<a name="step-2" />
### Step 2 - Create an ASPX Page to Upload Files###

Don't forget the _enctype_ attribute. I always forget that, and then the files won't be uploaded. Just sayin'.

![HTML form for uploading](https://github.com/bradygaster/BlobFu/blob/master/Images/html-form-for-uploading.png?raw=true "HTML form for uploading")

<a name="step-3---collect-the-data" />
### Step 3 - Collect the Data ###
The code below simply collects the file upload and slams it into Windows Azure Blob Storage. 

![saving blobs to blob storage](https://github.com/bradygaster/BlobFu/blob/master/Images/saving-blobs-to-blob-storage.png?raw=true "saving blobs to blob storage")

<a name="anchor-name-here" />
### Really? ###
Yes, really. Looking at the Windows Azure blob storage account in ClumsyLeaf's CloudXPlorer, you'll see images that are uploaded during testing. 

![checking the blob account using CloudXPlorer](https://github.com/bradygaster/BlobFu/blob/master/Images/checking-the-blob-account-using-cloudxplorer.png?raw=true)

<a name="anchor-name-here" />
### Have Fun! ###
Give BlobFu a try. Hopefully it'll ease the process of learning how to get your blobs into Windows Azure. These helper methods can also be used as WebMatrix 2 helpers, so give that a spin (and watch this space for more on this) if you get a moment. 

Please let me know of any issues or enhancements you observe (or think about) using the Issues link for this project.