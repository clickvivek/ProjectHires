using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{

    public interface ICityRepository : IRepository<City, int>
    {
        Task<List<City>> GetLocation(string? city, int? state, bool IsState);
    }
    public class CityRepository : BaseRepository<City, int>, ICityRepository
    {
        public CityRepository(EFContexts context) : base(context) { }

        public async Task<List<City>> GetLocation(string? searchString, int? state, bool IsState)
        {
            var query = _context.Cities
                    .Include(c => c.IdStateNavigation)
                    .ThenInclude(c => c.CountryCodeNavigation)
                    .Where(s => (searchString == null || (s.City1 != null && s.City1.StartsWith(searchString)) || (s.Zip != null && s.Zip.StartsWith(searchString)) || (IsState == true && s.IdStateNavigation.StateName != null && s.IdStateNavigation.StateName.StartsWith(searchString)))
                                && (state == null || ( state != null && s.IdState == state))).Distinct();

                return await query.ToListAsync();
        }

    }
   
}
