using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using DrHeinekamp_Project.Infrastructure;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class StorageServiceUploadTests
{
    private readonly Mock<IFileUploader> _mockFileUploader;
    private readonly Mock<IFormFile> _mockFile;
    private readonly StorageUploadService _storageService;
    private readonly string _bucketName = "test-bucket";

    public StorageServiceUploadTests()
    {
        _mockFileUploader = new Mock<IFileUploader>();
        _mockFile = new Mock<IFormFile>();

        _storageService = new StorageUploadService(
            _mockFileUploader.Object,
            _bucketName);
    }


    [Fact]
    public async Task UploadFilesAsync_ThrowsArgumentException_WhenFileAndPreviewCountMismatch()
    {
        // Arrange
        var files = new List<IFormFile> { _mockFile.Object };
        var previews = new List<IFormFile>(); // No preview files

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _storageService.UploadFilesAsync(files, previews));
        Assert.Equal("The number of files and previews must match.", exception.Message);
    }

    [Fact]
    public async Task UploadFilesAsync_UploadsFilesAndPreviewsSuccessfully()
    {
        // Arrange
        var fileContent = "File";
        var fileName = "testfile.pdf";
        var previewContent = "Preview";
        var previewFileName = "testfile_preview.png";

        var files = new List<IFormFile> { SetupMockFormFile(fileName, fileContent) };
        var previews = new List<IFormFile> { SetupMockFormFile(previewFileName, previewContent) };

        _mockFileUploader.Setup(tu => tu.UploadAsync(It.IsAny<TransferUtilityUploadRequest>()))
                         .Returns(Task.CompletedTask)
                         .Verifiable();

        // Act
        var result = await _storageService.UploadFilesAsync(files, previews);

        // Assert
        Assert.Single(result);
        Assert.Equal($"https://{_bucketName}.s3.amazonaws.com/{Uri.EscapeDataString(fileName)}", result[0]);
        _mockFileUploader.Verify(tu => tu.UploadAsync(It.Is<TransferUtilityUploadRequest>(r => r.Key == fileName)), Times.Once);
        _mockFileUploader.Verify(tu => tu.UploadAsync(It.Is<TransferUtilityUploadRequest>(r => r.Key == previewFileName)), Times.Once);
    }

    [Fact]
    public async Task UploadFilesAsync_ReturnsEmptyList_WhenNoFilesProvided()
    {
        // Arrange
        var files = new List<IFormFile>();
        var previews = new List<IFormFile>();

        // Act
        var result = await _storageService.UploadFilesAsync(files, previews);

        // Assert
        Assert.Empty(result);
        _mockFileUploader.Verify(tu => tu.UploadAsync(It.IsAny<TransferUtilityUploadRequest>()), Times.Never);
    }

    // Helper method to set up mock IFormFile
    private IFormFile SetupMockFormFile(string fileName, string content)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(stream.Length);

        return mockFile.Object;
    }
}
