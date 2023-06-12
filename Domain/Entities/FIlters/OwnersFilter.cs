namespace Domain.Entities.Filters
{
    public class OwnersFilter : BaseEntityFilter
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Title { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
