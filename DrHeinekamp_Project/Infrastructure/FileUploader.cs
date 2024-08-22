using Amazon.S3.Transfer;

namespace DrHeinekamp_Project.Infrastructure
{
    public class FileUploader : IFileUploader
    {
        private readonly TransferUtility _transferUtility;

        public FileUploader(TransferUtility transferUtility)
        {
            _transferUtility = transferUtility;
        }

        public Task UploadAsync(TransferUtilityUploadRequest request)
        {
            return _transferUtility.UploadAsync(request);
        }
    }
}
