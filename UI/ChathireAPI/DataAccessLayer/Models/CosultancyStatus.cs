using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CosultancyStatus
{
    public short Id { get; set; }

    public string Description { get; set; } = null!;

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<Consultancy> Consultancies { get; } = new List<Consultancy>();
}
