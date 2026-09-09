using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Shared
{
    public class Result
    {
        public List<Error> errors { get; set; }
    }
    public class Result<E> : Result
    {
        public E value { get; set; }
    }
}
