using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DrHeinekamp_Project.DTOs;
using DrHeinekamp_Project.Helper;
using DrHeinekamp_Project.Services;
using DrHeinekamp_Project.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Reflection.Metadata;

public class StorageService: IStorageService
{
    private readonly IAmazonS3 _awsBucketClient;
    private readonly IUrlGeneratorService _urlGeneratorService;
    private readonly string _bucketName;

    public StorageService(
        IAmazonS3 awsBucketClient,
        IOptions<AWSOptions> options,
        IUrlGeneratorService urlGeneratorService)
    {
        _awsBucketClient = awsBucketClient;
        _bucketName = options.Value.BucketName;
        _urlGeneratorService = urlGeneratorService;
    }

    public async Task<DocumentListOutput> GetList()
    {
        var output = new DocumentListOutput();

        var listResponse = await _awsBucketClient.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _bucketName
        });

        var awsDocuments = listResponse.S3Objects;
        var documents = new List<DocumentInfo>();

        foreach (var document in awsDocuments)
        {
            if (!document.Key.EndsWith(Constants.FileTypes.PreviewSuffix))
            {
                var previewFileName = $"{Path.GetFileNameWithoutExtension(document.Key)}{Constants.FileTypes.PreviewSuffix}";
                var previewObject = awsDocuments
                    .FirstOrDefault(x => x.Key.Equals(previewFileName, StringComparison.OrdinalIgnoreCase));

                var previewUrl = previewObject != null ? _urlGeneratorService.GeneratePermanentUrl(previewFileName) : null;
                var expirationTime = document.LastModified.AddHours(1);
                documents.Add(new DocumentInfo
                {
                    Name = document.Key,
                    FileType = IconHelper.GetIconUrl(document.Key),
                    UploadDate = document.LastModified,
                    Url = _urlGeneratorService.GeneratePermanentUrl(document.Key),
                    TempUrl = _urlGeneratorService.GenerateTemporaryUrl(document.Key, expirationTime),
                    PreviewUrl = previewUrl
                });
            }
        }

        output.Documents = documents.OrderByDescending(x => x.UploadDate).ToList();
        output.DocumentsCount = documents.Count;

        return output;
    }

    public async Task<List<string>> UploadFilesAsync(
    [FromForm] List<IFormFile> files,
    [FromForm] List<IFormFile> previews)
    {
        if (files.Count != previews.Count)
        {
            throw new ArgumentException("The number of files and previews must match.");
        }

        var fileTransferUtility = new TransferUtility(_awsBucketClient);
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

            uploadTasks.Add(fileTransferUtility.UploadAsync(fileUploadRequest));
            uploadTasks.Add(fileTransferUtility.UploadAsync(previewUploadRequest));

            string encodedFileName = Uri.EscapeDataString(file.FileName);
            urls.Add($"https://{_bucketName}.s3.amazonaws.com/{encodedFileName}");
        }

        await Task.WhenAll(uploadTasks);

        return urls;
    }

    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName
        };

        using (var response = await _awsBucketClient.GetObjectAsync(request))
        {
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }

    public async Task<Stream> DownloadFilesAsync(List<string> fileNames)
    {
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var fileName in fileNames)
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                using (var response = await _awsBucketClient.GetObjectAsync(request))
                {
                    if (response.ContentLength > 0)
                    {
                        var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                        using (var entryStream = entry.Open())
                        {
                            await response.ResponseStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName
        };

        await _awsBucketClient.DeleteObjectAsync(deleteObjectRequest);
    }
}
