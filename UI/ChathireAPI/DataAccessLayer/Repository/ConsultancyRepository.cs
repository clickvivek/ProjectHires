using BusinessEntityAndDTO.DTO;
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
    public interface IConsultancyRepository : IRepository<Consultancy, long>
    {
        Task<List<Consultancy>> SearchConsultancies(string conName);
    }
    public class ConsultancyRepository : BaseRepository<Consultancy, long>, IConsultancyRepository
    {
        public ConsultancyRepository(EFContexts context) : base(context) { }


        //_context.ConsultancyUsers.Where(s => s.ConsultancyId == ConsultancyId && s.UserId == Userid).Count()>0
        public async Task<List<Consultancy>> SearchConsultancies(string conName)
        {

            return await _context.Consultancies.Where(s => s.Name.StartsWith(conName)).ToListAsync();
        }

    }
}
