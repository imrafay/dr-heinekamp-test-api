using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using DrHeinekamp_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace DrHeinekamp_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IStorageService _service;
        private readonly IStorageUploadService _storageUploadService;

        public DocumentsController(IStorageService service, IStorageUploadService storageUploadService)
        {
            _service = service;
            _storageUploadService = storageUploadService;
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var fileStream = await _service.DownloadFileAsync(fileName);
            return File(fileStream, "application/octet-stream", fileName);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            var files = await _service.GetList();
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

            var stream = await _service.DownloadFilesAsync(fileNames);

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
