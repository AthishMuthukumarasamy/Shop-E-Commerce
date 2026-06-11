using System;
using System.Collections.Generic;

namespace ShopSphere.DatabaseModels;

public partial class Cart
{
    public int CartId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User User { get; set; } = null!;
}
