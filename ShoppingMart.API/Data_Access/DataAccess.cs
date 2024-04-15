using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using ShoppingMart.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShoppingMart.API.Data_Access
{
  public class DataAccess : IDataAccess
  {

    private readonly IConfiguration configuration; //Step 2.1
    private readonly string dbconnection;
    private readonly string dateformat;

    public DataAccess(IConfiguration configuration) //Step 2.2
    {
      this.configuration = configuration;
      dbconnection = this.configuration["ConnectionStrings:DB"];
      dateformat = this.configuration["Constants:DateFormat"];
    }
    public Offer GetOffer(int id)
    {
      var offer = new Offer();
      using (SqlConnection sqlConnection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = sqlConnection,
        };

        string query = "SELECT * FROM Offers WHERE OfferId=" + id + ";";
        command.CommandText = query;

        sqlConnection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          offer.Id = (int)reader["OfferId"];
          offer.Title = (string)reader["Title"];
          offer.Discount = (int)reader["Discount"];
        }
      }
      return offer;
    }

    public List<ProductCategory> GetProductCategories()
    {
      var productCategories = new List<ProductCategory>();
      using (SqlConnection connection = new SqlConnection(dbconnection)) //add connection
      {
        SqlCommand command = new()
        {
          Connection = connection,
        };
        string query = "SELECT * FROM ProductCategories;";
        command.CommandText = query; //Add qi=uery in command

        connection.Open(); //open connection
        SqlDataReader reader = command.ExecuteReader(); //Store the query in reader

        while (reader.Read())
        {
          var category = new ProductCategory()
          {
            Id = (int)reader["CategoryId"],
            Category = (string)reader["Category"],
            SubCategory = (string)reader["SubCategory"]
          };
          productCategories.Add(category);
        }
      }
        return productCategories;
      
    
    }

    public ProductCategory GetProductCategory(int id)
    {
      var prodCategory = new ProductCategory();
      using (SqlConnection sqlConnection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = sqlConnection,
        };

        string query = "SELECT * FROM ProductCategories WHERE CategoryID =" + id + ";";
        command.CommandText = query;

        sqlConnection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          prodCategory.Id = (int)reader["CategoryId"];
          prodCategory.Category = (string)reader["Category"];
          prodCategory.SubCategory = (string)reader["SubCategory"];
        }
      }
      return prodCategory;
    }

    public List<Product> GetProducts(string category, string subcategory, int count)
    {
      var products = new List<Product>();
      using (SqlConnection sqlConnection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = sqlConnection,
        };

        try
        {
          string query = "SELECT TOP (@count) * FROM Products WHERE CategoryID=(SELECT CategoryId FROM ProductCategories WHERE Category=@c AND SubCategory=@s) ORDER BY NewId();";
          command.CommandText = query;//this query gives random suggestion bcs of "newid() and help form sql injection as we use @c and @s"
          command.Parameters.Add("@c", System.Data.SqlDbType.NVarChar).Value = category;
          command.Parameters.Add("@s", System.Data.SqlDbType.NVarChar).Value = subcategory;
          command.Parameters.Add("@count", System.Data.SqlDbType.Int).Value = count;

          sqlConnection.Open();
          SqlDataReader reader = command.ExecuteReader();
          while (reader.Read())
          {
            var product = new Product()
            {
              Id = (int)reader["ProductId"],
              Title = (string)reader["Title"],
              Description = (string)reader["Description"],
              Price = (double)reader["Price"],
              Quantity = (int)reader["Quantity"],
              ImageName = (string)reader["ImageName"]
            };
            var categoryId = (int)reader["CategoryId"];
            product.ProductCategory = GetProductCategory(categoryId);

            var offerId = (int)reader["OfferId"];
            product.Offer = GetOffer(offerId);

            products.Add(product);
          }
        }
        catch (Exception ex)
        {
          // Handle exception (e.g., log error, throw exception, etc.)
          Console.WriteLine("Error retrieving products: " + ex.Message);
        }
      }
      return products;
    }

    public Product GetProduct(int id)  //Step 2.3
    {
      var product = new Product();
      using (SqlConnection sqlConnection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = sqlConnection,
        };

        string query = "SELECT * FROM Products WHERE ProductId=" + id + ";";
        command.CommandText = query;

        sqlConnection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          product.Id = (int)reader["ProductId"];
          product.Title = (string)reader["Title"];
          product.Description = (string)reader["Description"];
          product.Price = (double)reader["Price"];
          product.Quantity = (int)reader["Quantity"];
          product.ImageName = (string)reader["ImageName"];

          var categoryId = (int)reader["CategoryId"];
          product.ProductCategory = GetProductCategory(categoryId);

          var offerId = (int)reader["OfferId"];
          product.Offer = GetOffer(offerId);

        }
      }
      return product;
    }

    public bool InsertUser(User user)
    {
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };
        connection.Open();

        string query = "SELECT COUNT(*) FROM Users WHERE Email='" + user.Email + "';";
        command.CommandText = query;
        int count = (int)command.ExecuteScalar();
        if (count > 0)
        {
          connection.Close();
          return false;
        }

        query = "INSERT INTO Users (FirstName, LastName, Address, Mobile, Email, Password, CreatedAt, ModifiedAt) values (@fn, @ln, @add, @mb, @em, @pwd, @cat, @mat);";

        command.CommandText = query;
        command.Parameters.Add("@fn", System.Data.SqlDbType.NVarChar).Value = user.FirstName;
        command.Parameters.Add("@ln", System.Data.SqlDbType.NVarChar).Value = user.LastName;
        command.Parameters.Add("@add", System.Data.SqlDbType.NVarChar).Value = user.Address;
        command.Parameters.Add("@mb", System.Data.SqlDbType.NVarChar).Value = user.Mobile;
        command.Parameters.Add("@em", System.Data.SqlDbType.NVarChar).Value = user.Email;
        command.Parameters.Add("@pwd", System.Data.SqlDbType.NVarChar).Value = user.Password;
        command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = user.CreatedAt;
        command.Parameters.Add("@mat", System.Data.SqlDbType.NVarChar).Value = user.ModifiedAt;

        command.ExecuteNonQuery();
      }
      return true;
    }

    public string IsLoggedIn(string email, string password)
    {
      User user = new();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        connection.Open();
        string query = "SELECT COUNT(*) FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
        command.CommandText = query;
        int count = (int)command.ExecuteScalar();
        if (count == 0) //if no user found then return empty string.
        {
          connection.Close();
          return "";
        }

        query = "SELECT * FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
        command.CommandText = query;

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          user.Id = (int)reader["UserId"];
          user.FirstName = (string)reader["FirstName"];
          user.LastName = (string)reader["LastName"];
          user.Email = (string)reader["Email"];
          user.Address = (string)reader["Address"];
          user.Mobile = (string)reader["Mobile"];
          user.Password = (string)reader["Password"];
          user.CreatedAt = (string)reader["CreatedAt"];
          user.ModifiedAt = (string)reader["ModifiedAt"];
        }

        string key = "S6DgV73VrV4qcN9R4gWvuZRSYVYq69aZ0njDqXq6FZG2Eg6tEjFLfn8mxZhfrskSf7XHNst6ht44KbG8FOCzg";
        string duration = "60";
        var symmetrickey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(symmetrickey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
                    new Claim("id", user.Id.ToString()),
                    new Claim("firstName", user.FirstName),
                    new Claim("lastName", user.LastName),
                    new Claim("address", user.Address),
                    new Claim("mobile", user.Mobile),
                    new Claim("email", user.Email),
                    new Claim("createdAt", user.CreatedAt),
                    new Claim("modifiedAt", user.ModifiedAt)
                };

        var jwtToken = new JwtSecurityToken(
            issuer: "localhost",
            audience: "localhost",
            claims: claims,
            expires: DateTime.Now.AddMinutes(Int32.Parse(duration)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
      }
      
    }

    public void UserReview(Review review)
    {
      using SqlConnection connection = new(dbconnection);
      SqlCommand command = new()
      {
        Connection = connection
      };

      string query = "INSERT INTO Reviews (UserId, ProductId, Review, CreatedAt) VALUES (@uid, @pid, @rv, @cat);";
      command.CommandText = query;
      command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = review.User.Id;
      command.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = review.Product.Id;
      command.Parameters.Add("@rv", System.Data.SqlDbType.NVarChar).Value = review.Value;
      command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = review.CreatedAt;

      connection.Open();
      command.ExecuteNonQuery();

    }

    public List<Review> GetProductReviews(int productId)
    {
      var reviews = new List<Review>();
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT * FROM Reviews WHERE ProductId=" + productId + ";";
        command.CommandText = query;

        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          var review = new Review()
          {
            Id = (int)reader["ReviewId"],
            Value = (string)reader["Review"],
            CreatedAt = (string)reader["CreatedAt"]
          };

          var userid = (int)reader["UserId"];
          review.User = GetUser(userid);

          var productid = (int)reader["ProductId"];
          review.Product = GetProduct(productid);

          reviews.Add(review);
        }
      }
      return reviews;
    }

    public User GetUser(int id)
    {
      var user = new User();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT * FROM Users WHERE UserId=" + id + ";";
        command.CommandText = query;

        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          user.Id = (int)reader["UserId"];
          user.FirstName = (string)reader["FirstName"];
          user.LastName = (string)reader["LastName"];
          user.Email = (string)reader["Email"];
          user.Address = (string)reader["Address"];
          user.Mobile = (string)reader["Mobile"];
          user.Password = (string)reader["Password"];
          user.CreatedAt = (string)reader["CreatedAt"];
          user.ModifiedAt = (string)reader["ModifiedAt"];
        }
      }
      return user;
    }

    public bool InsertCartItem(int userId, int productId)
    {
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        connection.Open();
        string query = "SELECT COUNT(*) FROM Carts WHERE UserId=" + userId + " AND Ordered='false';";
        command.CommandText = query; //To Chek if their is active cart is avialable like cartitem != 0;
        int count = (int)command.ExecuteScalar();
        if (count == 0)//empty cart
        {
          query = "INSERT INTO Carts (UserId, Ordered, OrderedOn) VALUES (" + userId + ", 'false', '');";
          command.CommandText = query; //add in cart with giveing false value for order as it hasn't placed 
          command.ExecuteNonQuery();
        }

        query = "SELECT CartId FROM Carts WHERE UserId=" + userId + " AND Ordered='false';";
        command.CommandText = query; //Select cart if cartitem != 0;
        int cartId = (int)command.ExecuteScalar();


        query = "INSERT INTO CartItems (CartId, ProductId) VALUES (" + cartId + ", " + productId + ");";
        command.CommandText = query;
        command.ExecuteNonQuery();
        return true;
      }
    }

    public Cart GetActiveCartOfUser(int userId)
    {
      var cart = new Cart();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };
        connection.Open();

        string query = "SELECT COUNT(*) From Carts WHERE UserId=" + userId + " AND Ordered='false';";
        command.CommandText = query;//Check if cart is empty or not

        int count = (int)command.ExecuteScalar();
        if (count == 0)//If not cart exist then return empty cart
        {
          return cart;
        }

        query = "SELECT CartId From Carts WHERE UserId=" + userId + " AND Ordered='false';";
        command.CommandText = query;//get CartId if exist

        int cartid = (int)command.ExecuteScalar();//Fatch it from DB

        query = "select * from CartItems where CartId=" + cartid + ";";
        command.CommandText = query;//Select all Items from Cart for specific Id and

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())//Read all the items as asked below
        {
          CartItem item = new()
          {
            Id = (int)reader["CartItemId"],
            Product = GetProduct((int)reader["ProductId"])
          };
          cart.CartItems.Add(item);
        }
        //other Items
        cart.Id = cartid;
        cart.User = GetUser(userId);
        cart.Ordered = false;
        cart.OrderedOn = "";
      }
      return cart;
    }

    public Cart GetCart(int cartId)
    {//Return the all the cart items of specific cart id.
      var cart = new Cart();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };
        connection.Open();

        string query = "SELECT * FROM CartItems WHERE CartId=" + cartId + ";";
        command.CommandText = query;//Get the Items from the CartItem table with specific Id

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          CartItem item = new()
          {
            Id = (int)reader["CartItemId"],
            Product = GetProduct((int)reader["ProductId"])
          };
          cart.CartItems.Add(item);
        }
        reader.Close();

        query = "SELECT * FROM Carts WHERE CartId=" + cartId + ";";
        command.CommandText = query;//Get from the Cart table
        reader = command.ExecuteReader();
        while (reader.Read())
        {
          cart.Id = cartId;
          cart.User = GetUser((int)reader["UserId"]);
          cart.Ordered = bool.Parse((string)reader["Ordered"]);//bool to string == .Parse
          cart.OrderedOn = (string)reader["OrderedOn"];
        }
        reader.Close();
      }
      return cart;
    }

    public List<Cart> GetAllPreviousCartsOfUser(int userId)
    {
      var carts = new List<Cart>();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };
        string query = "SELECT CartId FROM Carts WHERE UserId=" + userId + " AND Ordered='true';";
        command.CommandText = query;//When user has already Ordered == True
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          var cartid = (int)reader["CartId"];
          carts.Add(GetCart(cartid));
        }
      }
      return carts;
    }

    public List<PaymentMethod> GetPaymentMethods()
    {
     var result = new List<PaymentMethod>();
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT * FROM PaymentMethods;";
        command.CommandText = query;

        connection.Open();
        SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
          PaymentMethod method = new()
          {
            Id = (int)reader["PaymentMethodId"],
            Type = (string)reader["Type"],
            Provider = (string)reader["Provider"],
            Available = bool.Parse((string)reader["Available"]),
            Reason = (string)reader["Reason"]
          };

          result.Add(method);
        }
        
      }
      return result;

    }

    public int InsertPayment(Payment payment)
    {
      int value = 0;
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = @"INSERT INTO Payments (PaymentMethodId, UserId, TotalAmount, ShippingCharges, AmountReduced, AmountPaid, CreatedAt) 
                                VALUES (@pmid, @uid, @ta, @sc, @ar, @ap, @cat);";

        command.CommandText = query;
        command.Parameters.Add("@pmid", System.Data.SqlDbType.Int).Value = payment.PaymentMethod.Id;
        command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = payment.User.Id;
        command.Parameters.Add("@ta", System.Data.SqlDbType.NVarChar).Value = payment.TotalAmount;
        command.Parameters.Add("@sc", System.Data.SqlDbType.NVarChar).Value = payment.ShipingCharges;
        command.Parameters.Add("@ar", System.Data.SqlDbType.NVarChar).Value = payment.AmountReduced;
        command.Parameters.Add("@ap", System.Data.SqlDbType.NVarChar).Value = payment.AmountPaid;
        command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = payment.CreatedAt;

        connection.Open();
        value = command.ExecuteNonQuery();

        if (value > 0)
        {
          query = "SELECT TOP 1 Id FROM Payments ORDER BY Id DESC;";
          command.CommandText = query;
          value = (int)command.ExecuteScalar();
        }
        else
        {
          value = 0;
        }
      }
      return value;
    }

    public int InsertOrder(Order order)
    {
      int value = 0;

      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "INSERT INTO Orders (UserId, CartId, PaymentId, CreatedAt) values (@uid, @cid, @pid, @cat);";

        command.CommandText = query;
        command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = order.User.Id;
        command.Parameters.Add("@cid", System.Data.SqlDbType.Int).Value = order.Cart.Id;
        command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = order.CreatedAt;
        command.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = order.Payment.Id;

        connection.Open();
        value = command.ExecuteNonQuery();

        if (value > 0)
        {
          query = "UPDATE Carts SET Ordered='true', OrderedOn='" + DateTime.Now.ToString(dateformat) + "' WHERE CartId=" + order.Cart.Id + ";";
          command.CommandText = query;
          command.ExecuteNonQuery();

          query = "SELECT TOP 1 Id FROM Orders ORDER BY Id DESC;";
          command.CommandText = query;
          value = (int)command.ExecuteScalar();
        }
        else
        {
          value = 0;
        }
      }

      return value;
    }
  }
}
