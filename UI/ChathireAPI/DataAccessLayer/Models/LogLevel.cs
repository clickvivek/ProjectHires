using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class LogLevel
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual ICollection<Log> Logs { get; } = new List<Log>();
}
