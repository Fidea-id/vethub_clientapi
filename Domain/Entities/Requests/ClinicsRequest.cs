using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Requests
{
    public class ClinicsRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Description { get; set; }
        public string? Photo { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? WebUrl { get; set; }
        public string? MapUrl { get; set; }
    }
}
