namespace DrHeinekamp_Project.Helper
{
    public class AWSOptions
    {
        public required string AccessKey { get; set; }
        public required string SecretKey { get; set; }
        public required string Region { get; set; }
        public required string BucketName { get; set; }
    }
}
