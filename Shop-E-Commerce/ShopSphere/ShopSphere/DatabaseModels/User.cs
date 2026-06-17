using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopSphere.DatabaseModels;

public partial class User
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    public string PasswordHash { get; set; } = null!;

    [Required(ErrorMessage = "Mobile number is required")]
    public string? MobileNo { get; set; }

    [Required(ErrorMessage = "Address is required")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; set; } = null!;
    public DateTime? CreatedDate { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();

    public virtual ICollection<ProductApproval> ProductApprovalAdmins { get; set; } = new List<ProductApproval>();

    public virtual ICollection<ProductApproval> ProductApprovalRetailers { get; set; } = new List<ProductApproval>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
