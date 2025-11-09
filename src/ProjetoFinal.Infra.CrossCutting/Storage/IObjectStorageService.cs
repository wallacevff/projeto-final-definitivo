using System.Threading;
using System.Threading.Tasks;

namespace ProjetoFinal.Infra.CrossCutting.Storage;

public interface IObjectStorageService
{
    Task<ObjectStorageUploadResult> UploadAsync(
        ObjectStorageUploadRequest request,
        CancellationToken cancellationToken = default);
}
