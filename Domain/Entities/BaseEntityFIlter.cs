namespace Domain.Entities
{
    public class BaseEntityFilter
    {
        public int? Id { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? SortProp { get; set; }
        public string? SortMode { get; set; }
    }
}
