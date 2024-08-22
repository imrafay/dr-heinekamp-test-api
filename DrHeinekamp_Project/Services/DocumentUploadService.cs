using Amazon.S3.Transfer;
using Amazon.S3;
using DrHeinekamp_Project.Infrastructure;
using DrHeinekamp_Project.Services.Interfaces;

public class DocumentUploadService : IDocumentUploadService
{
    private readonly IFileUploader _fileUploader;
    private readonly string _bucketName;

    public DocumentUploadService(IFileUploader fileUploader, string bucketName)
    {
        _fileUploader = fileUploader;
        _bucketName = bucketName;
    }

    public async Task<List<string>> UploadFilesAsync(List<IFormFile> files, List<IFormFile> previews)
    {
        if (files.Count != previews.Count)
        {
            throw new ArgumentException("Files and previews count must match.");
        }

        var tasks = new List<Task>();
        var urls = new List<string>();

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            var preview = previews[i];

            var fileUpload = new TransferUtilityUploadRequest
            {
                InputStream = file.OpenReadStream(),
                Key = file.FileName,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.Private
            };

            var previewUpload = new TransferUtilityUploadRequest
            {
                InputStream = preview.OpenReadStream(),
                Key = preview.FileName,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.Private
            };

            tasks.Add(_fileUploader.UploadAsync(fileUpload));
            tasks.Add(_fileUploader.UploadAsync(previewUpload));

            string encodedFileName = Uri.EscapeDataString(file.FileName);
            urls.Add($"https://{_bucketName}.s3.amazonaws.com/{encodedFileName}");
        }

        await Task.WhenAll(tasks);

        return urls;
    }
}
