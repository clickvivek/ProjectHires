using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class UserFavoritesDetail
{
    public long Id { get; set; }

    public long? UserFavoriteId { get; set; }

    public long? RefId { get; set; }

    public bool? IsJobOpening { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual UserFavorite? UserFavorite { get; set; }
}
