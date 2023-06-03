namespace Domain.Entities.Models
{
    public class Profile : BaseEntity
    {
        public int GlobalId { get; set; }
        public string Name { get; set; }
        public string Entity { get; set; }
        public string Email { get; set; }
        public string Photo { get; set; }
        public string Roles { get; set; }
    }
}
