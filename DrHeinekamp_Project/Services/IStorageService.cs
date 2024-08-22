using DrHeinekamp_Project.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DrHeinekamp_Project.Services
{
    public interface IStorageService
    {
        Task<DocumentListOutput> GetList();
        Task<List<string>> UploadFilesAsync(List<IFormFile> files, List<IFormFile> previews);
        Task<Stream> DownloadFileAsync(string fileName);
        Task<Stream> DownloadFilesAsync(List<string> fileNames);
        Task DeleteFileAsync(string fileName);
    }
}
