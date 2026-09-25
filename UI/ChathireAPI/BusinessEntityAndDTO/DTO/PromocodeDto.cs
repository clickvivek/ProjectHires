using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessEntityAndDTO.DTO
{
    public class PromocodeDto
    {
        public long Id { get; set; }
        public string? Promocode { get; set; }
        public string? Description { get; set; }
        public int? NoOfFreeDownloads { get; set; }
        public int? NoOfFreeJobPosting { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? DailyChatLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? Active { get; set; }
        public DateTime? Updated { get; set; }
        public long? UpdatedBy { get; set; }
        public int RedemptionCount { get; set; }
        public bool? IsSingleUse { get; set; }
        public int? MaxRedemptions { get; set; }
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    }

    public class CreatePromocodeDto
    {
        [Required(ErrorMessage = "Promo code is required")]
        [StringLength(50, ErrorMessage = "Promo code cannot exceed 50 characters")]
        public string Promocode { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Start Date is mandatory")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is mandatory")]
        public DateTime EndDate { get; set; }

        public int? NoOfFreeDownloads { get; set; }
        public int? NoOfFreeJobPosting { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? DailyChatLimit { get; set; }
        public bool? IsSingleUse { get; set; } = true;
        public int? MaxRedemptions { get; set; }
        public bool? Active { get; set; } = true;
    }

    public class RedeemPromocodeDto
    {
        [Required(ErrorMessage = "Promo code is required")]
        public string Promocode { get; set; } = null!;
        public long? UserId { get; set; }
    }

    public class RedeemPromocodeResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Promocode { get; set; }
        public int FreeDownloadsGranted { get; set; }
        public int FreeJobPostingGranted { get; set; }
        public int DailyChatLimitGranted { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
