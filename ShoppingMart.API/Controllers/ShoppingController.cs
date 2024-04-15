using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingMart.API.Data_Access;
using ShoppingMart.API.Models;

namespace ShoppingMart.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ShoppingController : ControllerBase // Step-4.1
  {
    readonly IDataAccess dataAccess;
    private readonly string DateFormat;

    public ShoppingController(IDataAccess dataAccess, IConfiguration configuration)  // Step-4.2
    {
      this.dataAccess = dataAccess;
      DateFormat = configuration["Constants: DateFormat"];
    }

    [HttpGet("GetCategoryList")]
    public IActionResult GetCategoryList()
    {
      var result = dataAccess.GetProductCategories();
      return Ok(result);
    }

    [HttpGet("GetProducts")]
    public IActionResult GetProducts(string category, string subcategory, int count)
    {
      var result = dataAccess.GetProducts(category, subcategory, count);
      return Ok(result);

    }

    [HttpGet("GetProduct/{id}")]  // Step-4.3
    public IActionResult GetProduct(int id)
    {
      var result = dataAccess.GetProduct(id);
      return Ok(result);
    }

    [HttpPost("RegisterUser")]
    public IActionResult RegisterUser([FromBody] User user)
    {
      user.CreatedAt = DateTime.Now.ToString();
      user.ModifiedAt = DateTime.Now.ToString();

      var result = dataAccess.InsertUser(user);

      string? message;
      if (result) message = "Registered Successfully!!";
      else message = "*Email already Exist ...";
      return Ok(message);
    }

    [HttpPost("LoginUser")]
    public IActionResult LoginUser([FromBody] User user)
    {
      var token = dataAccess.IsLoggedIn(user.Email, user.Password);
      if (token == "") token = "invalid!";
      return Ok(token);
    }

    [HttpPost("Review")]
    public IActionResult Review([FromBody] Review review)
    {
      review.CreatedAt = DateTime.Now.ToString();
      dataAccess.UserReview(review);
      return Ok("Review Inserted");
    }

    [HttpGet("GetProductReviews/{productId}")]
    public IActionResult GetProductReviews(int productId)
    {
      var result = dataAccess.GetProductReviews(productId);
      return Ok(result);
    }

    [HttpPost("InsertCartItem/{userId}/{productId}")]
    public IActionResult InsertCartItem(int userId, int productId)
    {
      var result = dataAccess.InsertCartItem(userId, productId);
      return Ok(result ? "Inserted" : "Not Inserted" );
    }

    [HttpGet("GetActiveCartOfUser/{id}")]
    public IActionResult GetActiveCartOfUser(int id)
    {
      var result = dataAccess.GetActiveCartOfUser(id);
      return Ok(result);
    }

    [HttpGet("GetAllPreviousCartsOfUser/{id}")]
    public IActionResult GetAllPreviousCartsOfUser(int id)
    {
      var result = dataAccess.GetAllPreviousCartsOfUser(id);
      return Ok(result);
    }

    [HttpGet("GetPaymentMethods")]
    public IActionResult GetPaymentMethods()
    {
      var result = dataAccess.GetPaymentMethods();
      return Ok(result);
    }

    [HttpPost("InsertPayment")]
    public IActionResult InsertPayment(Payment payment)
    {
      payment.CreatedAt = DateTime.Now.ToString();
      int id = dataAccess.InsertPayment(payment);
      return Ok(id.ToString());
    }

    [HttpPost("InsertOrder")]
    public IActionResult InsertOrder(Order order)
    {
      order.CreatedAt = DateTime.Now.ToString();
      int id = dataAccess.InsertOrder(order);
      return Ok(id.ToString());
    }
  }
}
