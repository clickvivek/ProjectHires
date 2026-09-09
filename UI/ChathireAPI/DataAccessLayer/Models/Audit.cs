using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Audit
{
    public long Id { get; set; }

    public short AuditGroupId { get; set; }

    public long? AuditBy { get; set; }

    public bool IsSuccess { get; set; }

    public string? Message { get; set; }

    public string? KeyColumnName { get; set; }

    public string? KeyColumnValue { get; set; }

    public string MethodFullName { get; set; } = null!;

    public string MethodName { get; set; } = null!;

    public string? AdddlData { get; set; }

    public DateTime AuditDate { get; set; }

    public int? Source { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual AuditGroup AuditGroup { get; set; } = null!;
}
