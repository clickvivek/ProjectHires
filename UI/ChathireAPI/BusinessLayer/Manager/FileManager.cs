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
            //fileModel.ImageFile.FileName = NewFileName(fileModel.ImageFile.FileName);
            //System.IO.File.Move(fileModel.ImageFile.FileName, );
            var blobClient = blobContainer.GetBlobClient(NewFileName(fileModel.ImageFile.FileName));

            await blobClient.UploadAsync(fileModel.ImageFile.OpenReadStream());

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

            await blobClient.UploadAsync(fileModel.ImageFile.OpenReadStream(), overwrite: true);

            return blobClient.Name;
        }

        public async Task<Stream> Get(string imageName, string containerName)
        {
            var blobContainer = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainer.GetBlobClient(imageName);
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

        static string NewFileName(string orgFilename)
        {
            
            string FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString() + Path.GetExtension(orgFilename);
            return FileName;
        }
    }
}
