namespace Domain.Entities.FIlters
{
    public class ServicesFilter : BaseEntityFilter
    {
        public string? Name { get; set; }
        public int? Duration { get; set; }
        public string? DurationType { get; set; }
        public bool? IsActive { get; set; }
        public int? Price { get; set; }
    }
}
