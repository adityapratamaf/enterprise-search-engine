using SearchEngine.Application.Features.FileAttachments.Commands.UploadFile;
using SearchEngine.Application.Features.FileAttachments.Queries.DownloadFile;
using SearchEngine.Application.Features.FileAttachments.Commands.DeleteFile;
using SearchEngine.Application.Features.FileAttachments.Queries.GetAllFiles;
using SearchEngine.Application.Features.FileAttachments.Queries.GetFileById;
using SearchEngine.Application.Features.FileAttachments.DTOs;
using SearchEngine.WebAPI.Contracts.FileAttachments;
using SearchEngine.WebAPI.OpenApi.Descriptions;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[EnableRateLimiting("api")]
[Route("api/file-attachments")]
[Authorize]
public class FileAttachmentsController
    : ControllerBase
{
    private readonly IMediator
        _mediator;

    public FileAttachmentsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    // Upload (satu atau banyak file)
    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointDescription(FileAttachmentDescriptions.Upload)]
    public async Task<IActionResult> Upload(
        [FromForm] UploadFileRequest request)
    {
        var files =
            new List<CreateFileUploadRequest>();

        foreach (var file in request.Files)
        {
            files.Add(
                new CreateFileUploadRequest
                {
                    FileName =
                        file.FileName,

                    ContentType =
                        file.ContentType,

                    FileStream =
                        file.OpenReadStream()
                });
        }

        var result =
            await _mediator.Send(
                new UploadFileCommand(
                    request.Module,
                    request.RecordId,
                    files));

        return Ok(result);
    }

    // Download File
    [HttpGet("{id:guid}/download")]
    [EndpointDescription(FileAttachmentDescriptions.Download)]
    public async Task<IActionResult> Download(
        Guid id)
    {
        var result =
            await _mediator.Send(
                new DownloadFileQuery(id));

        return File(
            result.Content,
            result.ContentType,
            result.FileName);
    }

    // Delete
    [HttpDelete("{id:guid}")]
    [EndpointDescription(FileAttachmentDescriptions.Delete)]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        var result =
            await _mediator.Send(
                new DeleteFileCommand(id));

        return Ok(result);
    }

    // Get All
    [HttpGet]
    [EndpointDescription(FileAttachmentDescriptions.GetAll)]
    public async Task<IActionResult> GetAll(
        [FromQuery]
        GetAllFilesQuery query)
    {
        var result =
            await _mediator.Send(query);

        return Ok(result);
    }

    // Get By Id
    [HttpGet("{id:guid}")]
    [EndpointDescription(FileAttachmentDescriptions.GetById)]
    public async Task<IActionResult> GetById(
        Guid id)
    {
        var result =
            await _mediator.Send(
                new GetFileByIdQuery(id));

        return Ok(result);
    }

}
