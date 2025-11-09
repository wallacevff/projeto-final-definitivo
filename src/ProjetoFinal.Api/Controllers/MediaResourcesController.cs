using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Api.Models;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Infra.CrossCutting.Storage;

namespace ProjetoFinal.Api.Controllers;

[Route("api/media-resources")]
[Route("api/v1/media-resources")]
public class MediaResourcesController : BaseController<
    MediaResourceDto,
    MediaResourceCreateDto,
    MediaResourceCreateDto,
    MediaResourceFilter,
    Guid,
    IMediaResourceAppService>
{
    private const long DefaultUploadLimit = 200 * 1024 * 1024; // 200 MB
    private readonly IObjectStorageService _storageService;

    public MediaResourcesController(
        IMediaResourceAppService service,
        IObjectStorageService storageService) : base(service)
    {
        _storageService = storageService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(DefaultUploadLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = DefaultUploadLimit)]
    public async Task<ActionResult<MediaResourceDto>> UploadAsync(
        [FromForm] MediaUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("Arquivo nao enviado.");
        }

        var contentType = NormalizeContentType(request.File);
        var sha256 = await ComputeSha256Async(request.File, cancellationToken);
        var existing = await Service.FindByShaAsync(sha256, cancellationToken);
        if (existing is not null)
        {
            return Ok(existing);
        }

        var objectName = BuildObjectName(request.File.FileName);
        var mediaKind = request.Kind ?? InferMediaKind(request.File.ContentType, request.File.FileName);

        await using var stream = request.File.OpenReadStream();
        var uploadResult = await _storageService.UploadAsync(
            new ObjectStorageUploadRequest
            {
                Content = stream,
                ObjectName = objectName,
                ContentType = contentType,
                Length = request.File.Length
            },
            cancellationToken);

        var dto = new MediaResourceCreateDto
        {
            FileName = Path.GetFileName(objectName),
            OriginalFileName = request.File.FileName,
            ContentType = contentType,
            Kind = mediaKind,
            SizeInBytes = request.File.Length,
            StoragePath = $"{uploadResult.BucketName}/{uploadResult.ObjectName}",
            Sha256 = sha256
        };

        var media = await Service.AddAsync(dto, cancellationToken);
        return Ok(media);
    }

    private static string NormalizeContentType(IFormFile file)
    {
        return string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
    }

    private static string BuildObjectName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant() ?? string.Empty;
        return $"media/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
    }

    private static async Task<string> ComputeSha256Async(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static MediaKind InferMediaKind(string? contentType, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return MediaKind.Image;
            }
            if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return MediaKind.Video;
            }
            if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                return MediaKind.Audio;
            }
        }

        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        return extension switch
        {
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" => MediaKind.Image,
            ".mp4" or ".mov" or ".mkv" or ".webm" => MediaKind.Video,
            ".mp3" or ".wav" or ".ogg" => MediaKind.Audio,
            _ => MediaKind.Document
        };
    }
}
