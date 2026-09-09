using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Status
{
    public short Id { get; set; }

    public string? Description { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }
}
