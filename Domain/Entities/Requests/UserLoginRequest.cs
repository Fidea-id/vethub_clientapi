using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Requests
{
    public class UserLoginRequest
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
