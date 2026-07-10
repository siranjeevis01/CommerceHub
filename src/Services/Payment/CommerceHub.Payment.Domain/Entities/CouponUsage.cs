namespace CommerceHub.Payment.Domain.Entities;

public class CouponUsage : BaseEntity
{
    public int CouponId { get; set; }
    public int UserId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal OrderAmount { get; set; }
    public DateTime UsedAt { get; set; }
    public int OrderId { get; set; }
    public Coupon? Coupon { get; set; }
}
