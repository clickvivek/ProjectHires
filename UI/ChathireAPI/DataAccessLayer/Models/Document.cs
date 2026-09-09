using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Document
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual ICollection<CandidateDocument> CandidateDocuments { get; } = new List<CandidateDocument>();
}
