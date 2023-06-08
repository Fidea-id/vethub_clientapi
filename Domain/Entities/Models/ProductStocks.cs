using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Models
{
    public class ProductStocks : BaseEntity
    {
        public int ProductId { get; set; }
        public double Stock { get; set; }
        public string Volume { get; set; }
    }
}
