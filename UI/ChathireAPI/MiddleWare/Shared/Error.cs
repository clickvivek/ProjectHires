using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Shared
{
    interface IError
    {
        string Id { get; set; }
        string Message { get; set; }
    }
    public class Error : IError
    {
        public string Id { get; set; }
        public string Message { get; set; }

        // other fields

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
