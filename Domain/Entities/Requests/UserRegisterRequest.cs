using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Requests
{
    public class UserRegisterRequest
    {
        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public string Roles { get; set; }
    }
}
