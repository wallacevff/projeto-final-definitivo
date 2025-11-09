using System.IO;

namespace ProjetoFinal.Infra.CrossCutting.Storage;

public class ObjectStorageDownloadResult
{
    public ObjectStorageDownloadResult(string bucketName, string objectName, Stream content)
    {
        BucketName = bucketName;
        ObjectName = objectName;
        Content = content;
    }

    public string BucketName { get; }

    public string ObjectName { get; }

    public Stream Content { get; }
}
