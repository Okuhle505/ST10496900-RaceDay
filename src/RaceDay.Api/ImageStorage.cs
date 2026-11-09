using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api;

public interface IImageStorage { Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken); }

public sealed class AzureBlobImageStorage(IConfiguration configuration) : IImageStorage
{
    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        var container = new BlobContainerClient(configuration["AzureBlob:ConnectionString"], configuration["AzureBlob:ContainerName"]);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        var name = $"{folder}/{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var blob = container.GetBlobClient(name);
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType } }, cancellationToken);
        return blob.Uri.ToString();
    }
}

public sealed class LocalImageStorage(IWebHostEnvironment environment) : IImageStorage
{
    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var root = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", folder);
        Directory.CreateDirectory(root);
        await using var stream = File.Create(Path.Combine(root, name));
        await file.CopyToAsync(stream, cancellationToken);
        return $"/uploads/{folder}/{name}";
    }
}
