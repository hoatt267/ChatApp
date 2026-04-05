namespace ChatApp.Shared.Interfaces
{
    public interface IBlobStorageService
    {
        // Hàm tải file lên Blob Storage. Trả về URL của file đã tải lên.
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName);

        // Hàm xóa file khỏi Blob Storage dựa trên URL của file.
        Task DeleteFileAsync(string fileUrl, string containerName);
    }
}