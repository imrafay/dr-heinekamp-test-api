using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Xunit;

namespace DrHeinekamp_Project.Tests.Services
{
    public class StorageDownloadServiceTests
    {
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly DocumentDownloadService _service;
        private readonly string _bucketName = "test-bucket";

        public StorageDownloadServiceTests()
        {
            _mockS3Client = new Mock<IAmazonS3>();
            _service = new DocumentDownloadService(_mockS3Client.Object, _bucketName);
        }

        [Fact]
        public async Task DownloadFilesAsync_ReturnsZip_WithCorrectFiles()
        {
            // Arrange
            var files = new List<string> { "file1.txt", "file2.txt" };
            var mockstreams = new Dictionary<string, MemoryStream>
            {
                { "file1.txt", CreateMockStream("file1 content") },
                { "file2.txt", CreateMockStream("file2 content") }
            };
            foreach (var fileName in files)
            {
                _mockS3Client.Setup(s => s.GetObjectAsync(It.Is<GetObjectRequest>(r => r.Key == fileName), default))
                             .ReturnsAsync(new GetObjectResponse
                             {
                                 ResponseStream = mockstreams[fileName],
                                 ContentLength = mockstreams[fileName].Length
                             });
            }

            // Act
            var result = await _service.DownloadFilesAsync(files);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MemoryStream>(result);

            using (var zip = new ZipArchive(result, ZipArchiveMode.Read))
            {
                Assert.NotEmpty(zip.Entries);
                Assert.Equal(files.Count, zip.Entries.Count);
                foreach (var fileName in files)
                {
                    var entry = zip.GetEntry(fileName);
                    Assert.NotNull(entry);

                    using (var entryStream = entry.Open())
                    using (var reader = new StreamReader(entryStream))
                    {
                        var content = reader.ReadToEnd();
                        Assert.Contains(fileName.Contains("file1") ? "file1 content" : "file2 content", content);
                    }
                }
            }
        }

        [Fact]
        public async Task DownloadFilesAsync_ReturnsEmptyZip_WithNoFiles()
        {
            // Arrange
            var files = new List<string>();

            // Act
            var result = await _service.DownloadFilesAsync(files);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MemoryStream>(result);

            using (var zip = new ZipArchive(result, ZipArchiveMode.Read))
            {
                Assert.Empty(zip.Entries);
            }
        }

        private MemoryStream CreateMockStream(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
