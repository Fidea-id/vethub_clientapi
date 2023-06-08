using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Models
{
    public class ProductBundles:BaseEntity
    {
        public int BundleId { get; set; }
        public int ProductId { get; set; }
        public double Quantity { get; set; }
    }
}
