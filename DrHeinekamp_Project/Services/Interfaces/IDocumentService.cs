using DrHeinekamp_Project.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DrHeinekamp_Project.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentListOutput> GetListAsync();
        Task DeleteFileAsync(string fileName);
    }
}
