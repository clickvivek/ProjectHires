using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class ChatHistory
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long ChatUserId { get; set; }

    public DateTime ChatTime { get; set; }

    public bool IsActive { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual User? User { get; set; }

    public virtual User? ChatUser { get; set; }
}
