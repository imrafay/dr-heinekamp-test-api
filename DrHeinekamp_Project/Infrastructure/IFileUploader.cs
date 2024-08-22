using Amazon.S3.Transfer;

namespace DrHeinekamp_Project.Infrastructure
{
    public interface IFileUploader
    {
        Task UploadAsync(TransferUtilityUploadRequest request);
    }

}
