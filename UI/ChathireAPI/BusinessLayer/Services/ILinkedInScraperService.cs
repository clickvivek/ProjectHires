using BusinessEntityAndDTO.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public interface ILinkedInScraperService
    {
        Task<LinkedInCompanyScrapedDto> ScrapeCompanyAsync(string url);
        Task<List<LinkedInCompanyScrapedDto>> ScrapeAndSaveCompaniesAsync(List<string> urls, bool autoSaveToDb, long? adminUserId);
    }
}
