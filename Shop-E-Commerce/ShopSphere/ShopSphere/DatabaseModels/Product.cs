using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopSphere.DatabaseModels;

public partial class Product
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Product Name is required")]
    public string ProductName { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(1, 999999, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock is required")]
    [Range(0, 99999, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }

    
    public int CategoryId { get; set; }
    
    public int BrandId { get; set; }
    
    public int RetailerId { get; set; }

    [ValidateNever]
    public string Status { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ValidateNever]
    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ValidateNever]
    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductApproval> ProductApprovals { get; set; } = new List<ProductApproval>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    [ValidateNever]
    public virtual User Retailer { get; set; } = null!;

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
