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


        public async Task<List<Consultancy>> SearchConsultancies(string conName)
        {
            if (string.IsNullOrWhiteSpace(conName))
            {
                return new List<Consultancy>();
            }

            var clean = conName.Trim();
            var cleanNoProtocol = clean
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("www.", "", StringComparison.OrdinalIgnoreCase)
                .Trim('/', ' ', '\\');

            return await _context.Consultancies
                .Where(s => (s.Active == null || s.Active == true) &&
                            ((s.Name != null && (s.Name.Contains(clean) || s.Name.Contains(cleanNoProtocol))) ||
                             (s.Website != null && (s.Website.Contains(clean) || s.Website.Contains(cleanNoProtocol))) ||
                             (s.Domainname != null && (s.Domainname.Contains(clean) || s.Domainname.Contains(cleanNoProtocol)))))
                .Take(50)
                .ToListAsync();
        }

    }
}
