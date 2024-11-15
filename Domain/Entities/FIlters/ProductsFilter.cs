﻿namespace Domain.Entities.Filters
{
    public class ProductsFilter : BaseEntityFilter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? Price { get; set; }
        public bool? IsBundle { get; set; } // Indicates if the product is a bundle
        public bool? HasDiscount { get; set; } // Indicates if the product has discount or no
    }
}
