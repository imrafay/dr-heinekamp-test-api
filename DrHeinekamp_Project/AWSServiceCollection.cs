﻿using Amazon.S3;
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
        services.Configure<AWSOptions>(configuration.GetSection("AWS"));

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

        services.AddScoped<IDocumentUploadService>(sp =>
        {
            var fileUploader = sp.GetRequiredService<IFileUploader>();
            var awsOptions = sp.GetRequiredService<IOptions<AWSOptions>>().Value;
            return new DocumentUploadService(fileUploader, awsOptions.BucketName);
        });

        services.AddScoped<IDocumentDownloadService>(sp =>
        {
            var awsOptions = sp.GetRequiredService<IOptions<AWSOptions>>().Value;
            return new DocumentDownloadService(sp.GetRequiredService<IAmazonS3>(), awsOptions.BucketName);
        });
        services.AddScoped<IDocumentService, DocumentService>();

        return services;
    }
}
