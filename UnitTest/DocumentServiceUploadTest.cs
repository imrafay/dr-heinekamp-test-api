using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using DrHeinekamp_Project.Infrastructure;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class DocumentServiceUploadTest
{
    private readonly Mock<IFileUploader> _mockFileUploader;
    private readonly Mock<IFormFile> _mockFile;
    private readonly DocumentUploadService _storageService;
    private readonly string _bucketName = "test-bucket";

    public DocumentServiceUploadTest()
    {
        _mockFileUploader = new Mock<IFileUploader>();
        _mockFile = new Mock<IFormFile>();

        _storageService = new DocumentUploadService(
            _mockFileUploader.Object,
            _bucketName);
    }


    [Fact]
    public async Task UploadFilesAsync_WhenFileAndPreviewCountMismatch()
    {
        // Arrange
        var files = new List<IFormFile> { _mockFile.Object };
        var previews = new List<IFormFile>(); 

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _storageService.UploadFilesAsync(files, previews));
        Assert.Equal("Files and previews count must match.", exception.Message);
    }

    [Fact]
    public async Task UploadFilesAsync_UploadsFilesAndPreviewsSuccessfully()
    {
        // Arrange
        var fileContent = "File";
        var fileName = "file.pdf";
        var preview = "Preview";
        var previewFile = "file_preview.png";

        var files = new List<IFormFile> 
        { 
            MockFormFile(fileName, fileContent) 
        };
        var previews = new List<IFormFile>
        { 
            MockFormFile(previewFile, preview) 
        };

        _mockFileUploader.Setup(tu => tu.UploadAsync(It.IsAny<TransferUtilityUploadRequest>()))
                         .Returns(Task.CompletedTask)
                         .Verifiable();

        // Act
        var result = await _storageService.UploadFilesAsync(files, previews);

        // Assert
        Assert.Single(result);
        Assert.Equal($"https://{_bucketName}.s3.amazonaws.com/{Uri.EscapeDataString(fileName)}", result[0]);
        _mockFileUploader
            .Verify(x => x.UploadAsync(It.Is<TransferUtilityUploadRequest>(r => r.Key == fileName)), Times.Once);
        _mockFileUploader
            .Verify(x => x.UploadAsync(It.Is<TransferUtilityUploadRequest>(r => r.Key == previewFile)), Times.Once);
    }

    [Fact]
    public async Task UploadFilesAsync_WhenNoFilesProvided()
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

    private IFormFile MockFormFile(string fileName, string content)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.OpenReadStream()).Returns(stream);
        mockFile.Setup(x => x.FileName).Returns(fileName);
        mockFile.Setup(x => x.Length).Returns(stream.Length);

        return mockFile.Object;
    }
}
