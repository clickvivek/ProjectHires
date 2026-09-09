using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class UserFavorite
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public long? UserId { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserFavoritesDetail> UserFavoritesDetails { get; } = new List<UserFavoritesDetail>();
}
