using System;
using System.Collections.Generic;

namespace ShopSphere.DatabaseModels;

public partial class Otp
{
    public int OtpId { get; set; }

    public int UserId { get; set; }

    public string OtpCode { get; set; } = null!;

    public DateTime ExpiryTime { get; set; }

    public bool? IsUsed { get; set; }

    public virtual User User { get; set; } = null!;
}
