using AutoMapper;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Common;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;


namespace BusinessLayer.Manager
{
    public interface IFileManager
    {
        Task<string> Upload(FileModel fileModel, string containerName);
        Task<string> UploadWithName(FileModel fileModel, string customFileName, string containerName);
        Task<Stream> Get(string imageName, string containerName);
        Task Delete(string imageName, string containerName);
        BlobClient GetBlobClient(string imageName, string containerName);
        string GetContentType(string fileName);
    }
    public class FileManager : BaseManager<FileManager>, IFileManager  
    {
        private readonly BlobServiceClient _blobServiceClient;

        public FileManager(IServiceProvider provider, ILogger<FileManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
            _blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=hiresblob;AccountKey=XPt9HT6xcg2SEjcClC1aYbRoDR/PO/Lv8f0IB9TgjTbXEAO286ChXwGFNMlYMNvoE3TCcNaXYIHP+AStJ0IZ5A==;EndpointSuffix=core.windows.net");
        }

        public async Task<string> Upload(FileModel fileModel, string containerName)
        {
            var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
            string newFileName = NewFileName(fileModel.ImageFile.FileName);
            var blobClient = blobContainer.GetBlobClient(newFileName);

            string contentType = GetContentType(newFileName);
            var options = new Azure.Storage.Blobs.Models.BlobUploadOptions
            {
                HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    ContentType = contentType,
                    ContentDisposition = $"inline; filename=\"{newFileName}\""
                }
            };

            await blobClient.UploadAsync(fileModel.ImageFile.OpenReadStream(), options);

            return blobClient.Name;
        }

        public async Task<string> UploadWithName(FileModel fileModel, string customFileName, string containerName)
        {
            var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);
            string ext = Path.GetExtension(fileModel.ImageFile.FileName);
            if (string.IsNullOrEmpty(ext)) ext = ".png";
            
            string cleanName = customFileName.Replace("http://", "").Replace("https://", "").Replace("www.", "").Trim('/', ' ', '\\');
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                cleanName = cleanName.Replace(c, '_');
            }
            if (string.IsNullOrEmpty(cleanName)) cleanName = "company_logo";

            string finalFileName = cleanName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ? cleanName : cleanName + ext;
            var blobClient = blobContainer.GetBlobClient(finalFileName);

            string contentType = ext.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "image/png"
            };

            var options = new Azure.Storage.Blobs.Models.BlobUploadOptions
            {
                HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    ContentType = contentType,
                    CacheControl = "no-cache, no-store, must-revalidate"
                }
            };

            await blobClient.UploadAsync(fileModel.ImageFile.OpenReadStream(), options);

            return blobClient.Name;
        }

        public async Task<Stream> Get(string imageName, string containerName)
        {
            var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainer.GetBlobClient(imageName);
            if (!await blobClient.ExistsAsync())
            {
                return null!;
            }
            var downloadContent = await blobClient.DownloadAsync();
            return downloadContent.Value.Content;
        }

        public BlobClient GetBlobClient(string imageName, string containerName)
        {
            var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);

            return blobContainer.GetBlobClient(imageName);
            //var downloadContent = await blobClient.DownloadAsync();
            //return downloadContent.Value.Content;
        }

        public async Task Delete(string imageName, string containerName)
        {
            var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainer.GetBlobClient(imageName);
            await blobClient.DeleteAsync();
            
        }

        public string GetContentType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "application/octet-stream";
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".rtf" => "application/rtf",
                ".txt" => "text/plain",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }

        public static string NewFileName(string orgFilename)
        {
            if (string.IsNullOrWhiteSpace(orgFilename))
            {
                return $"file_{DateTime.Now:yyyyMMddHHmmss}";
            }

            string ext = Path.GetExtension(orgFilename);
            string baseName = Path.GetFileNameWithoutExtension(orgFilename);

            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "file";
            }

            // Remove invalid file name characters
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                baseName = baseName.Replace(c, '_');
            }

            baseName = baseName.Trim().TrimEnd('.');
            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "file";
            }

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            return $"{baseName}_{timestamp}{ext}";
        }
    }
}
