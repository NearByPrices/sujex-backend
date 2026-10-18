namespace ErpCrm.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string folder);
    Task DeleteAsync(string fileUrl);
}
