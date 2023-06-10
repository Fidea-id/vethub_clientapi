namespace Domain.Entities.Models
{
    public class PatientsFilter : BaseEntityFilter
    {
        public int? OwnersId { get; set; }
        public string? Name { get; set; }
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? Vaccinated { get; set; }
    }
}
