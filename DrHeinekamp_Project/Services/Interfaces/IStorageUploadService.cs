using DrHeinekamp_Project.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DrHeinekamp_Project.Services.Interfaces
{
    public interface IStorageUploadService
    {
        Task<List<string>> UploadFilesAsync(List<IFormFile> files, List<IFormFile> previews);
    }
}
