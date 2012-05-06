using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;

namespace BlobFu.Tests
{
    [TestFixture]
    public class UnitTests
    {
        string _connectionString = "Blobs";
        string _container = "UnitTests";
        private string _filename = @"Samples\nuget.gif";

        [Test]
        [ExpectedException(exceptionType: typeof(ApplicationException))]
        public void blob_fu_disallows_passing_of_byte_array_and_stream()
        {
            using (var fs = File.Open(_filename, FileMode.Open))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                new BlobFuService(_connectionString)
                    .SaveToBlobStorage(
                        new BlobStorageRequest
                        {
                            BlobSavedCallback = (x)
                                => Assert.That(!string.IsNullOrEmpty(x.AbsoluteUri)),
                            Container = _container,
                            Filename = _filename,
                            StreamOfDataToStore = fs,
                            DataToStore = bytes
                        })
                    .DeleteBlob(_filename);

            }
        }

        [Test]
        [ExpectedException(exceptionType: typeof(ApplicationException))]
        public void blob_fu_requires_data_be_saved()
        {
            using (var fs = File.Open(_filename, FileMode.Open))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                new BlobFuService(_connectionString)
                    .SaveToBlobStorage(
                        new BlobStorageRequest
                        {
                            BlobSavedCallback = (x)
                                => Assert.That(!string.IsNullOrEmpty(x.AbsoluteUri)),
                            Container = _container,
                            Filename = _filename,
                            StreamOfDataToStore = fs,
                            DataToStore = bytes
                        })
                    .DeleteBlob(_filename);

            }
        }

        [Test]
        public void stream_can_be_uploaded_to_blob_storage()
        {
            try
            {
                using (var fs = File.Open(_filename, FileMode.Open))
                {
                    new BlobFuService(_connectionString).SaveToBlobStorage(
                        new BlobStorageRequest
                        {
                            BlobSavedCallback = (x)
                                => Assert.That(!string.IsNullOrEmpty(x.AbsoluteUri)),
                            Container = _container,
                            Filename = _filename,
                            StreamOfDataToStore = fs
                        });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        [Test]
        public void byte_arry_can_be_uploaded_to_blob_storage()
        {
            using (var fs = File.Open(_filename, FileMode.Open))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                new BlobFuService(_connectionString)
                    .SaveToBlobStorage(
                        new BlobStorageRequest
                        {
                            BlobSavedCallback = (x)
                                =>
                                {
                                    Assert.That(!string.IsNullOrEmpty(x.AbsoluteUri));
                                    Console.WriteLine(x.AbsoluteUri);
                                },
                            Container = _container,
                            Filename = _filename,
                            DataToStore = bytes
                        })
                    .DeleteBlob(_filename);
            }
        }

        [Test]
        public void blob_list_in_container_can_be_obtained()
        {
            using (var fs = File.Open(_filename, FileMode.Open))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                new BlobFuService(_connectionString)
                    .SaveToBlobStorage(
                        new BlobStorageRequest
                        {
                            BlobSavedCallback = (x) =>
                            {
                                Console.WriteLine(x.AbsoluteUri);
                            },
                            Container = _container,
                            Filename = _filename,
                            DataToStore = bytes
                        })
                    .ListBlobs(new BlobListRequest
                    {
                        Container = _container,
                        ListReceivedCallback = (x) => Assert.That(x.Any())
                    })
                    .DeleteBlob(_filename);
            }
        }

        [Test]
        public void blob_containers_can_be_created()
        {
            using (var fs = File.Open(_filename, FileMode.Open))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                new BlobFuService(_connectionString)
                    .SaveToBlobStorage(
                        new BlobStorageRequest
                        {
                            BlobSavedCallback = (x) =>
                            {
                                Console.WriteLine(x.AbsoluteUri);
                            },
                            Container = _container,
                            Filename = _filename,
                            DataToStore = bytes
                        })
                    .ListContainers((x) =>
                        {
                            Assert.That(x.Any());
                            x.ForEach(s => Console.WriteLine(s));
                        })
                    .DeleteBlob(_filename);
            }
        }

        [Test]
        public void blob_can_be_deleted()
        {
            using (var fs = File.Open(_filename, FileMode.Open))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                new BlobFuService(_connectionString)
                    .SaveToBlobStorage(
                        new BlobStorageRequest
                        {
                            BlobSavedCallback = (x) =>
                            {
                                Console.WriteLine(x.AbsoluteUri);
                            },
                            Container = _container,
                            Filename = _filename,
                            DataToStore = bytes
                        })
                    .ListBlobs(new BlobListRequest
                    {
                        Container = _container,
                        ListReceivedCallback = (x) => Assert.That(x.Any())
                    })
                    .DeleteBlob(_filename)
                    .ListBlobs(new BlobListRequest
                    {
                        Container = _container,
                        ListReceivedCallback = (x) =>
                            {
                                Assert.That(!x.Any(y => y == _filename));
                            }
                    });
            }
        }

        [Test]
        public void objects_can_be_serialized_to_blob_storage()
        {
            var p = new Person { Name = "brady", Id = 1 };

            new BlobFuService(_connectionString)
                .Post<Person>(new SerializedObjectSaveRequest<Person>
                {
                    ConnectionStringName = _connectionString,
                    Container = _container,
                    Filename = string.Format("{0}.prsn",
                        Path.GetFileNameWithoutExtension(Path.GetRandomFileName())),
                    ObjectToSave = p
                });
        }

        [Test]
        public void serialized_objects_can_be_re_read_via_generic_argument()
        {
            var nm = "brady";
            var fn = string.Format("{0}.prsn",
                        Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var p = new Person { Name = nm, Id = 1 };

            new BlobFuService(_connectionString)
                .Post<Person>(new SerializedObjectSaveRequest<Person>
                {
                    ConnectionStringName = _connectionString,
                    Container = _container,
                    Filename = fn,
                    ObjectToSave = p
                })
                .Get<Person>(new SerializedObjectGetRequest<Person>
                {
                    ConnectionStringName = _connectionString,
                    Container = _container,
                    Filename = fn,
                    ObjectFoundCallback = (r) =>
                    {
                        Assert.That(r != null && r.Name.Equals(nm));
                        Console.WriteLine(r.Name);
                    }
                });
        }
    }
}
