using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DrHeinekamp_Project.DTOs;
using DrHeinekamp_Project.Helper;
using DrHeinekamp_Project.Services.Interfaces;
using DrHeinekamp_Project.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Reflection.Metadata;

public class DocumentService: IDocumentService
{
    private readonly IAmazonS3 _awsBucketClient;
    private readonly IUrlGeneratorService _urlGeneratorService;
    private readonly string _bucketName;

    public DocumentService(
        IAmazonS3 awsBucketClient,
        IOptions<AWSOptions> options,
        IUrlGeneratorService urlGeneratorService)
    {
        _awsBucketClient = awsBucketClient;
        _bucketName = options.Value.BucketName;
        _urlGeneratorService = urlGeneratorService;
    }

    public async Task<DocumentListOutput> GetListAsync()
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
                var previewFile = $"{Path.GetFileNameWithoutExtension(document.Key)}{Constants.FileTypes.PreviewSuffix}";
                var preview = awsDocuments
                    .FirstOrDefault(x => x.Key.Equals(previewFile, StringComparison.OrdinalIgnoreCase));

                var previewUrl = preview != null ? _urlGeneratorService.GeneratePermanentUrl(previewFile) : null;
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

    public async Task DeleteFileAsync(string fileName)
    {
        await DeleteFileFromBucketAsync(fileName);
        var previewFileName = Path.GetFileNameWithoutExtension(fileName) + Constants.FileTypes.PreviewSuffix;
        await DeleteFileFromBucketAsync(previewFileName);
    }

    private async Task DeleteFileFromBucketAsync(string fileName)
    {
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName
        };

        await _awsBucketClient.DeleteObjectAsync(deleteObjectRequest);
    }
}
