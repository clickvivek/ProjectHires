using BusinessEntityAndDTO.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public interface ILinkedInScraperService
    {
        Task<LinkedInCompanyScrapedDto> ScrapeCompanyAsync(string url);
        Task<List<LinkedInCompanyScrapedDto>> ScrapeAndSaveCompaniesAsync(List<string> urls, bool autoSaveToDb, long? adminUserId);
        Task<LinkedInBatchUpdateResultDto> ReviewAndUpdateLinkedInWebsitesAsync(int? limit = null, int? offset = null, int concurrency = 3, long? specificConsultancyId = null);
    }
}
