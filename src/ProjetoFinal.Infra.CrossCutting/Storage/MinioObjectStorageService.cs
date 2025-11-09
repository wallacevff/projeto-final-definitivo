using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

namespace ProjetoFinal.Infra.CrossCutting.Storage;

public class MinioObjectStorageService : IObjectStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioConfiguration _configuration;
    private readonly ILogger<MinioObjectStorageService> _logger;
    private readonly SemaphoreSlim _bucketSemaphore = new(1, 1);
    private bool _bucketReady;

    public MinioObjectStorageService(
        IMinioClient client,
        IOptions<MinioConfiguration> options,
        ILogger<MinioObjectStorageService> logger)
    {
        _client = client;
        _configuration = options.Value;
        _logger = logger;
    }

    public async Task<ObjectStorageUploadResult> UploadAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ObjectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ContentType);

        await EnsureBucketAsync(cancellationToken);

        if (request.Content.CanSeek)
        {
            request.Content.Seek(0, SeekOrigin.Begin);
        }

        var putArgs = new PutObjectArgs()
            .WithBucket(_configuration.BucketName)
            .WithObject(request.ObjectName)
            .WithStreamData(request.Content)
            .WithObjectSize(request.Length)
            .WithContentType(request.ContentType);

        await _client.PutObjectAsync(putArgs, cancellationToken);

        var publicUrl = BuildObjectUrl(request.ObjectName);
        _logger.LogInformation("Uploaded object {Object} to bucket {Bucket}", request.ObjectName, _configuration.BucketName);

        return new ObjectStorageUploadResult(_configuration.BucketName, request.ObjectName, publicUrl);
    }

    public async Task<ObjectStorageDownloadResult> DownloadAsync(
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketAsync(cancellationToken);

        var memoryStream = new MemoryStream();
        var getArgs = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(memoryStream);
            });

        await _client.GetObjectAsync(getArgs, cancellationToken);
        memoryStream.Position = 0;

        return new ObjectStorageDownloadResult(bucketName, objectName, memoryStream);
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        if (_bucketReady)
        {
            return;
        }

        await _bucketSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_bucketReady)
            {
                return;
            }

            var bucketExists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_configuration.BucketName),
                cancellationToken);

            if (!bucketExists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_configuration.BucketName);
                if (!string.IsNullOrWhiteSpace(_configuration.Region))
                {
                    makeBucketArgs = makeBucketArgs.WithLocation(_configuration.Region);
                }

                await _client.MakeBucketAsync(makeBucketArgs, cancellationToken);
                _logger.LogInformation("Bucket {Bucket} created on MinIO.", _configuration.BucketName);
            }

            _bucketReady = true;
        }
        finally
        {
            _bucketSemaphore.Release();
        }
    }

    private string BuildObjectUrl(string objectName)
    {
        var endpoint = _configuration.Endpoint.TrimEnd('/');
        return $"{endpoint}/{_configuration.BucketName}/{objectName}";
    }
}
