using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopSphere.DatabaseModels;

public partial class Product
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Product Name is required")]
    public string ProductName { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock is required")]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Please select a category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Please select a brand")]
    public int BrandId { get; set; }


    public int RetailerId { get; set; }

    public string Status { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductApproval> ProductApprovals { get; set; } = new List<ProductApproval>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual User? Retailer { get; set; }

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
