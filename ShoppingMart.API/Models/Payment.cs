namespace ShoppingMart.API.Models
{
  public class Payment
  {
    public int Id { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = new PaymentMethod();
    public User User { get; set; } = new User();
    public int TotalAmount { get; set; }
    public int ShipingCharges { get; set; }
    public int AmountReduced { get; set; }
    public int AmountPaid { get; set; }
    public string CreatedAt { get; set; } = string.Empty;

  }
}
