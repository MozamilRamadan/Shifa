
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shifa.Application.Interfaces.MedicalRecords;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Doctor")]
public class MedicalFilesController : ControllerBase
{
    private readonly IMedicalFileService _medicalFileService;

    public MedicalFilesController(
        IMedicalFileService medicalFileService)
    {
        _medicalFileService = medicalFileService;
    }

    [HttpPost("record/{recordId:int}")]
    [RequestSizeLimit(21 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        int recordId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new
            {
                Message = "Please select a file."
            });

        var currentUserId = GetCurrentUserId();

        await using var stream = file.OpenReadStream();

        var fileId = await _medicalFileService.UploadAsync(
            recordId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            currentUserId,
            cancellationToken);

        return Ok(new
        {
            Id = fileId,
            Message = "File uploaded successfully."
        });
    }

    [HttpGet("{fileId:int}/download")]
    public async Task<IActionResult> Download(
    int fileId,
    CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        var result = await _medicalFileService.DownloadAsync(
            fileId,
            currentUserId,
            cancellationToken);

        return File(
            result.FileStream,
            result.FileType,
            result.FileName);
    }

    [HttpDelete("{fileId:int}")]
    public async Task<IActionResult> Delete(
        int fileId,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        await _medicalFileService.DeleteAsync(
            fileId,
            currentUserId,
            cancellationToken);

        return Ok(new
        {
            Message = "File deleted successfully."
        });
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            throw new UnauthorizedAccessException();

        return currentUserId;
    }
}
