using Amazon.S3.Transfer;
using Amazon.S3;
using DrHeinekamp_Project.Infrastructure;
using DrHeinekamp_Project.Services.Interfaces;

public class StorageUploadService : IStorageUploadService
{
    private readonly IFileUploader _fileUploader;
    private readonly string _bucketName;

    public StorageUploadService(IFileUploader fileUploader, string bucketName)
    {
        _fileUploader = fileUploader;
        _bucketName = bucketName;
    }

    public async Task<List<string>> UploadFilesAsync(List<IFormFile> files, List<IFormFile> previews)
    {
        if (files.Count != previews.Count)
        {
            throw new ArgumentException("The number of files and previews must match.");
        }

        var uploadTasks = new List<Task>();
        var urls = new List<string>();

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            var preview = previews[i];

            var fileUploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = file.OpenReadStream(),
                Key = file.FileName,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.Private
            };

            var previewUploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = preview.OpenReadStream(),
                Key = preview.FileName,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.Private
            };

            uploadTasks.Add(_fileUploader.UploadAsync(fileUploadRequest));
            uploadTasks.Add(_fileUploader.UploadAsync(previewUploadRequest));

            string encodedFileName = Uri.EscapeDataString(file.FileName);
            urls.Add($"https://{_bucketName}.s3.amazonaws.com/{encodedFileName}");
        }

        await Task.WhenAll(uploadTasks);

        return urls;
    }
}
