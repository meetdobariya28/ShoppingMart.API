using ShoppingMart.API.Models;

namespace ShoppingMart.API.Data_Access
{
  public interface IDataAccess
  {
    List<ProductCategory> GetProductCategories();

    ProductCategory GetProductCategory(int id);
    Offer GetOffer(int id);
    List<Product> GetProducts(string category, string subcategory, int count);

    Product GetProduct(int id); //Step - 1

    bool InsertUser(User user);

    string IsLoggedIn(string email, string password);

    void UserReview(Review review);
    List<Review> GetProductReviews(int productId);
    User GetUser(int id);

    bool InsertCartItem(int userId, int productId);

    Cart GetActiveCartOfUser(int userId);//Cart Not ordered yet

    Cart GetCart(int cartId);//Give Cart from the cardid is used for GetAllPreviousCartsOfUser();

    List<Cart> GetAllPreviousCartsOfUser(int userId);//List of the previous Item

    List<PaymentMethod> GetPaymentMethods();

    int InsertPayment(Payment payment);

    int InsertOrder(Order order); 
  }
}
