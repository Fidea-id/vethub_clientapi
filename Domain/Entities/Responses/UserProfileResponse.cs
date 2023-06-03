namespace Domain.Entities.Responses
{
    public class UserProfileResponse : BaseEntity
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Roles { get; set; }
    }
}
