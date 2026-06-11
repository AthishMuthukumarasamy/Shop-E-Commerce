using System;
using System.Collections.Generic;

namespace ShopSphere.DatabaseModels;

public partial class ProductApproval
{
    public int ApprovalId { get; set; }

    public int ProductId { get; set; }

    public int RetailerId { get; set; }

    public int AdminId { get; set; }

    public string ApprovalStatus { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public virtual User Admin { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual User Retailer { get; set; } = null!;
}
