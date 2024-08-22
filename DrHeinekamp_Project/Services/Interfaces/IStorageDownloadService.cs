using DrHeinekamp_Project.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DrHeinekamp_Project.Services.Interfaces
{
    public interface IStorageDownloadService
    {
        Task<Stream> DownloadFileAsync(string fileName);

        Task<Stream> DownloadFilesAsync(List<string> fileNames);
    }
}
