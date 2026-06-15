using System;
using System.Collections.Generic;

namespace ShopSphere.DatabaseModels;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? MobileNo { get; set; }

    public string? Address { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<CompareProduct> CompareProducts { get; set; } = new List<CompareProduct>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();

    public virtual ICollection<ProductApproval> ProductApprovalAdmins { get; set; } = new List<ProductApproval>();

    public virtual ICollection<ProductApproval> ProductApprovalRetailers { get; set; } = new List<ProductApproval>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
