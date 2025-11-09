namespace ProjetoFinal.Infra.CrossCutting.Storage;

public class ObjectStorageUploadResult
{
    public ObjectStorageUploadResult(string bucketName, string objectName, string? publicUrl)
    {
        BucketName = bucketName;
        ObjectName = objectName;
        PublicUrl = publicUrl;
    }

    public string BucketName { get; }

    public string ObjectName { get; }

    public string? PublicUrl { get; }
}
