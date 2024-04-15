namespace ShoppingMart.API.Models
{
  public class PaymentMethod
  {
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public bool Available { get; set; }

    public string Reason { get; set; } = string.Empty;
  }
}
