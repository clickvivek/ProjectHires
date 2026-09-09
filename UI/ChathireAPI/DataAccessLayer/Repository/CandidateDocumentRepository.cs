using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface ICandidateDocumentRepository : IRepository<CandidateDocument, long>
    {
        Task<string> RemoveDocument(long candidateProfileId, short documentId, UserContext userContext);
    }
    public class CandidateDocumentRepository : BaseRepository<CandidateDocument, long>, ICandidateDocumentRepository
    {
        public CandidateDocumentRepository(EFContexts context) : base(context) { }

        public async Task<string> RemoveDocument(long candidateProfileId, short documentId, UserContext userContext)
        {
            string fileName = "";
            var cp = _context.CandidateDocuments.Where(a => a.CandidateProfileId == candidateProfileId && a.DocumentId == documentId).ToList();
            if (cp != null && cp.Count>0)
            {
                fileName = cp.FirstOrDefault().Doc;
                _context.RemoveRange(cp);
                await _context.SaveChangesAsync();
            }
            return fileName;
        }

    }

}
