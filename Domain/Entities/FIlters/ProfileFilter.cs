namespace Domain.Entities.FIlters
{
    public class ProfileFilter : BaseEntityFilter
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Roles { get; set; }
    }
}
