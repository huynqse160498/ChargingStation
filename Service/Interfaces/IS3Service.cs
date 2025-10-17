using Microsoft.AspNetCore.Http;

namespace Services.Interfaces
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<string> RenameFileAsync(string oldFileUrl, string newFileName);
    }
}
