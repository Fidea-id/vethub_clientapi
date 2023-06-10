namespace Domain.Entities.Models
{
    public class ProductDiscounts : BaseEntity
    {
        public int DiscountId { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; }
        public double DiscountValue { get; set; }
        public DiscountType DiscountType { get; set; } // Enum or separate table to indicate the type of discount
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public enum DiscountType
    {
        Percentage,
        Amount,
    }
}
