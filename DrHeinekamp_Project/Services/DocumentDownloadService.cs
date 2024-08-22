using Amazon.S3.Transfer;
using Amazon.S3;
using DrHeinekamp_Project.Infrastructure;
using DrHeinekamp_Project.Services.Interfaces;
using Amazon.S3.Model;
using System.IO.Compression;

public class DocumentDownloadService: IDocumentDownloadService
{
    private readonly IAmazonS3 _awsBucketClient;
    private readonly string _bucketName;

    public DocumentDownloadService(IAmazonS3 awsBucketClient, string bucketName)
    {
        _awsBucketClient = awsBucketClient;
        _bucketName = bucketName;
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
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
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
                        var file = zip.CreateEntry(fileName, CompressionLevel.Fastest);
                        using (var entryStream = file.Open())
                        {
                            await response.ResponseStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }
        }

        stream.Position = 0;
        return stream;
    }
}
