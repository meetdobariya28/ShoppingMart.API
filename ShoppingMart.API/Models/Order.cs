namespace ShoppingMart.API.Models
{
  public class Order
  {
    public int Id { get; set; }
    public Payment Payment { get; set; } = new Payment();
    public User User { get; set; } = new User();
    public Cart Cart { get; set; } = new Cart();
    public string CreatedAt { get; set; } = string.Empty;
  }
}
