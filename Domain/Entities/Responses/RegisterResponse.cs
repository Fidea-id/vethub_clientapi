using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Responses
{
    public class RegisterResponse
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Roles { get; set; }
        public string ConfirmationToken { get; set; }
    }
}
