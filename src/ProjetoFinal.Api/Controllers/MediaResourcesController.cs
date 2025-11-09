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
using Microsoft.Extensions.Options;
using ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

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
    private readonly MinioConfiguration _minioConfiguration;

    public MediaResourcesController(
        IMediaResourceAppService service,
        IObjectStorageService storageService,
        IOptions<MinioConfiguration> minioConfiguration) : base(service)
    {
        _storageService = storageService;
        _minioConfiguration = minioConfiguration.Value;
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

    [HttpGet("{mediaId:guid}/download")]
    public async Task<IActionResult> DownloadAsync([FromRoute] Guid mediaId, CancellationToken cancellationToken = default)
    {
        var media = await Service.GetByIdAsync(mediaId, cancellationToken);
        if (media is null)
        {
            return NotFound();
        }

        var parsed = ParseStoragePath(media.StoragePath);
        if (parsed is null)
        {
            return NotFound();
        }
        var (bucket, objectName) = parsed.Value;

        var download = await _storageService.DownloadAsync(bucket, objectName, cancellationToken);
        var contentType = string.IsNullOrWhiteSpace(media.ContentType) ? "application/octet-stream" : media.ContentType;
        var fileName = string.IsNullOrWhiteSpace(media.OriginalFileName) ? media.FileName : media.OriginalFileName;

        return File(download.Content, contentType, fileName);
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

    private (string bucket, string objectName)? ParseStoragePath(string? storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return null;
        }

        var normalized = storagePath.TrimStart('/');
        var parts = normalized.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return null;
        }

        if (parts.Length == 1)
        {
            var bucketName = string.IsNullOrWhiteSpace(_minioConfiguration.BucketName)
                ? null
                : _minioConfiguration.BucketName;

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return null;
            }

            return (bucketName!, parts[0]);
        }

        return (parts[0], parts[1]);
    }
}
