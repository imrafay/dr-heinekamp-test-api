using Amazon.S3;
using Amazon.S3.Transfer;
using DrHeinekamp_Project.Helper;
using DrHeinekamp_Project.Infrastructure;
using DrHeinekamp_Project.Services;
using DrHeinekamp_Project.Services.Interfaces;
using Microsoft.Extensions.Options;

public static class AWSServiceCollection
{
    public static IServiceCollection AddAwsServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure AWS options
        services.Configure<AWSOptions>(configuration.GetSection("AWS"));

        // Register IAmazonS3
        services.AddScoped<IAmazonS3>(sp =>
        {
            var awsOptions = sp.GetRequiredService<IOptions<AWSOptions>>().Value;
            return new AmazonS3Client(
                awsOptions.AccessKey,
                awsOptions.SecretKey,
                Amazon.RegionEndpoint.GetBySystemName(awsOptions.Region));
        });

        services.AddScoped<TransferUtility>(sp =>
            new TransferUtility(sp.GetRequiredService<IAmazonS3>()));

        services.AddScoped<IUrlGeneratorService>(sp =>
        {
            var awsOptions = sp.GetRequiredService<IOptions<AWSOptions>>().Value;
            return new UrlGeneratorService(sp.GetRequiredService<IAmazonS3>(), awsOptions.BucketName);
        });

        services.AddScoped<IFileUploader, FileUploader>();

        services.AddScoped<IStorageUploadService>(sp =>
        {
            var fileUploader = sp.GetRequiredService<IFileUploader>();
            var awsOptions = sp.GetRequiredService<IOptions<AWSOptions>>().Value;
            return new StorageUploadService(fileUploader, awsOptions.BucketName);
        });

        services.AddScoped<IStorageDownloadService>(sp =>
        {
            var awsOptions = sp.GetRequiredService<IOptions<AWSOptions>>().Value;
            return new StorageDownloadService(sp.GetRequiredService<IAmazonS3>(), awsOptions.BucketName);
        });
        services.AddScoped<IStorageService, StorageService>();

        return services;
    }
}
