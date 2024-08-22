using Amazon.S3;
using Amazon.S3.Model;
using System;

namespace DrHeinekamp_Project.Services
{
    public class UrlGeneratorService : IUrlGeneratorService
    {
        private readonly IAmazonS3 _awsBucketClient;
        private readonly string _bucketName;

        public UrlGeneratorService(IAmazonS3 awsBucketClient, string bucketName)
        {
            _awsBucketClient = awsBucketClient;
            _bucketName = bucketName;
        }

        public string GeneratePermanentUrl(string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            return _awsBucketClient.GetPreSignedURL(request);
        }

        public string GenerateTemporaryUrl(string key, DateTime expiryDateTime)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = expiryDateTime
            };

            return _awsBucketClient.GetPreSignedURL(request);
        }
    }
}
