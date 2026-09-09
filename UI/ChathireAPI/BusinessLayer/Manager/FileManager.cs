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
