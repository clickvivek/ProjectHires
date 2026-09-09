using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{

    public interface ICategoryRepository : IRepository<Category, int>
    {

    }
    public class CategoryRepository : BaseRepository<Category, int>, ICategoryRepository
    {
        public CategoryRepository(EFContexts context) : base(context) { }

    }

}
