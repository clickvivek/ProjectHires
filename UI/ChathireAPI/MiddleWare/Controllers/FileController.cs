using AutoMapper;
using Azure.Storage.Blobs;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using Middleware.Shared;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : BaseCtrler<FileController>
    {
        public FileController(IServiceProvider serviceProvider, ILogger<FileController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {
        }

        [Route("upload")]
        [HttpPost]
        //[ApiAuthorize("AddUser")]
        public async Task<string> Upload([FromForm] FileModel model, string containerName)
        {
            string fileName="";
            if (model.ImageFile != null)
            {
                var fileManager = managerFactory.Get<IFileManager>();

                fileName = await fileManager.Upload(model, containerName);
            }
            return fileName;
        }

        [Route("get")]
        [HttpGet]
        public async Task<IActionResult> Get(string fileName, string fileType, string containerName)
        {
            var fileManager = managerFactory.Get<IFileManager>();

            var imgStream = await fileManager.Get(fileName, containerName);

            return File(imgStream, $"image/{fileType}");
        }


        [Route("download")]
        [HttpGet]
        public async Task<IActionResult> GetDownload(string fileName, string fileType, string containerName)
        {
            var fileManager = managerFactory.Get<IFileManager>();

            var imgStream = await fileManager.Get(fileName, containerName);
           
            return File(imgStream, $"image/{fileType}", $"blobfile.{fileType}");
        }

        [Route("delete")]
        [HttpDelete]
        //[ApiAuthorize("AddUser")]
        public async Task<bool> Delete(string imageName, string containerName)
        {
            if (imageName != null && imageName != "")
            {
                var fileManager = managerFactory.Get<IFileManager>();

                await fileManager.Delete(imageName, containerName);
                return true;
            }
            return false;
        }

    }
}
