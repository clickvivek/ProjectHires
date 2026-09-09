using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IJobOpeningProfileConsultancyCommentRepository : IRepository<JobOpeningProfileConsultancyComment, long>
    {
    }
    public class JobOpeningProfileConsultancyCommentRepository : BaseRepository<JobOpeningProfileConsultancyComment, long>, IJobOpeningProfileConsultancyCommentRepository
    {
        public JobOpeningProfileConsultancyCommentRepository(EFContexts context) : base(context) { }
    }
}
