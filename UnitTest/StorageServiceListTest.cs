using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using DrHeinekamp_Project.Services;
using DrHeinekamp_Project.DTOs;
using System;
using DrHeinekamp_Project.Helper;
using Microsoft.Extensions.Options;

namespace DrHeinekamp_Project.Tests
{
    public class StorageServiceTests
    {
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly Mock<IUrlGeneratorService> _mockUrlGeneratorService;
        private readonly IStorageService _storageService;

        public StorageServiceTests()
        {
            _mockS3Client = new Mock<IAmazonS3>();
            _mockUrlGeneratorService = new Mock<IUrlGeneratorService>();

            var awsOptions = Options.Create(new AWSOptions
            {
                AccessKey = "test-access-key",
                SecretKey = "test-secret-key",
                Region = "us-west-2",
                BucketName = "test-bucket"
            });

            _storageService = new StorageService(
                awsBucketClient: _mockS3Client.Object,
                options:awsOptions,
                urlGeneratorService:_mockUrlGeneratorService.Object);
        }

        [Fact]
        public async Task GetList_ReturnsCorrectDocumentList()
        {
            // Arrange
            var s3Objects = new List<S3Object>
            {
                new S3Object { Key = "document1.pdf", LastModified = DateTime.UtcNow.AddDays(-1) },
                new S3Object { Key = "document2.docx", LastModified = DateTime.UtcNow.AddDays(-2) }
            };

            _mockS3Client.Setup(s3 => s3.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                         .ReturnsAsync(new ListObjectsV2Response { S3Objects = s3Objects });

            _mockUrlGeneratorService.Setup(s => s.GeneratePermanentUrl(It.IsAny<string>()))
                                    .Returns((string key) => $"https://mock-bucket.s3.amazonaws.com/{key}");

            _mockUrlGeneratorService.Setup(s => s.GenerateTemporaryUrl(It.IsAny<string>(), It.IsAny<DateTime>()))
                                    .Returns((string key, DateTime expirationTime) => $"https://mock-bucket.s3.amazonaws.com/{key}?expires={expirationTime}");

            // Act
            var result = await _storageService.GetList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.DocumentsCount);
            Assert.Equal("document1.pdf", result.Documents[0].Name);
            Assert.Equal("document2.docx", result.Documents[1].Name);
            _mockUrlGeneratorService.Verify(s => s.GeneratePermanentUrl(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetList_ReturnsEmptyDocumentListWhenNoDocuments()
        {
            // Arrange
            _mockS3Client.Setup(s3 => s3.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                         .ReturnsAsync(new ListObjectsV2Response { S3Objects = new List<S3Object>() });

            // Act
            var result = await _storageService.GetList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Documents);
            Assert.Equal(0, result.DocumentsCount);
        }

        [Fact]
        public async Task GetList_ExcludesPreviewFilesFromDocumentList()
        {
            // Arrange
            var s3Objects = new List<S3Object>
            {
                new S3Object { Key = "document1.pdf", LastModified = DateTime.UtcNow.AddDays(-1) },
                new S3Object { Key = "document1_preview.png", LastModified = DateTime.UtcNow.AddDays(-1) }
            };

            _mockS3Client.Setup(s3 => s3.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                         .ReturnsAsync(new ListObjectsV2Response { S3Objects = s3Objects });

            _mockUrlGeneratorService.Setup(s => s.GeneratePermanentUrl(It.IsAny<string>()))
                                    .Returns((string key) => $"https://mock-bucket.s3.amazonaws.com/{key}");

            _mockUrlGeneratorService.Setup(s => s.GenerateTemporaryUrl(It.IsAny<string>(), It.IsAny<DateTime>()))
                                    .Returns((string key, DateTime expirationTime) => $"https://mock-bucket.s3.amazonaws.com/{key}?expires={expirationTime}");

            // Act
            var result = await _storageService.GetList();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Documents);
            Assert.Equal("document1.pdf", result.Documents[0].Name);
        }

    }
}
