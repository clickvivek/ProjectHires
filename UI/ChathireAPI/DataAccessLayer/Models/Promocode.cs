using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Promocode
{
    public long Id { get; set; }

    public string? Description { get; set; }

    public string? Promocode1 { get; set; }

    public int? NoOfFreeDownloads { get; set; }

    public int? NoOfFreeJobPosting { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }
}
