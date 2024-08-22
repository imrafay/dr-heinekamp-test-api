using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using DrHeinekamp_Project.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DrHeinekamp_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _service;
        private readonly IDocumentUploadService _storageUploadService;
        private readonly IDocumentDownloadService _storageDownloadService;

        public DocumentsController(
            IDocumentService service,
            IDocumentUploadService storageUploadService,
            IDocumentDownloadService storageDownloadService)
        {
            _service = service;
            _storageUploadService = storageUploadService;
            _storageDownloadService = storageDownloadService;
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var fileStream = await _storageDownloadService.DownloadFileAsync(fileName);
            return File(fileStream, "application/octet-stream", fileName);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            var files = await _service.GetListAsync();
            return Ok(files);
        }

        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> Delete(string fileName)
        {
            await _service.DeleteFileAsync(fileName);
            return NoContent();
        }

        [HttpPost("download-multiple")]
        public async Task<IActionResult> DownloadMultipleFiles([FromBody] List<string> fileNames)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                return BadRequest("No files specified for download.");
            }

            var stream = await _storageDownloadService.DownloadFilesAsync(fileNames);

            return File(stream, "application/zip", "documents.zip");
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleFiles(
            [FromForm] List<IFormFile> files,
            [FromForm] List<IFormFile> previews) {

            var urls = await _storageUploadService.UploadFilesAsync(files, previews);
            return Ok(new { urls });
        }
    }

}
