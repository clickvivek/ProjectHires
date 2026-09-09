using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.Models
{
    public class FileModel
    {
        public IFormFile ImageFile { get; set; }
    }
}
