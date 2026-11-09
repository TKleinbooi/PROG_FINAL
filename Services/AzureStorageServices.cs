
using Azure.Storage.Blobs;

namespace RaceDayAPI.Services;

public class AzureStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadFileAsync(
        IFormFile file,
        string containerName)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Please select a valid file.");
        }

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync(
            Azure.Storage.Blobs.Models.PublicAccessType.Blob);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var blobClient = containerClient.GetBlobClient(fileName);

        await using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(
        string fileUrl,
        string containerName)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return;

        var blobName = Path.GetFileName(
            new Uri(fileUrl).AbsolutePath);

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }
}

