using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChatApp.Shared.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ChatApp.Shared.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"]
                ?? throw new ArgumentNullException("AzureStorage:ConnectionString is missing in appsettings.json");
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName)
        {
            // 1. Kết nối tới Azurite
            var blobServiceClient = new BlobServiceClient(_connectionString);

            // 2. Lấy (hoặc tạo mới) cái "Xô" (Container) chứa ảnh
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Lệnh này cực kỳ quan trọng: Nếu xô chưa có, nó tạo xô và gán quyền Public để Frontend có thể lấy ảnh ra xem được
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // 3. Đổi tên file để tránh trùng lặp (ví dụ: avatar.jpg -> 1234-5678-avatar.jpg)
            var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
            var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

            // 4. Bơm file lên Azurite kèm theo ContentType (để trình duyệt biết đây là ảnh hay video)
            var blobHttpHeader = new BlobHttpHeaders { ContentType = contentType };
            await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeader });

            // 5. Trả về đường link URL của bức ảnh
            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl, string containerName)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                // Tách lấy cái tên file gốc từ cái URL
                var uri = new Uri(fileUrl);
                var fileName = Path.GetFileName(uri.LocalPath);

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(fileName);

                // Ra lệnh xóa
                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                throw new Exception($"Failed to delete file from Blob Storage. URL: {fileUrl}");
            }
        }
    }
}