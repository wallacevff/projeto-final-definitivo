using System.IO;

namespace ProjetoFinal.Infra.CrossCutting.Storage;

public class ObjectStorageUploadRequest
{
    public required Stream Content { get; init; }

    public required string ObjectName { get; init; }

    public required string ContentType { get; init; }

    public required long Length { get; init; }
}
