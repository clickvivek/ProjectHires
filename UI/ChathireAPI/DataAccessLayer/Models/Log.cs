using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Log
{
    public long Id { get; set; }

    public short LogLevel { get; set; }

    public string? Message { get; set; }

    public string? AdddlData { get; set; }

    public int? Source { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual LogLevel LogLevelNavigation { get; set; } = null!;
}
