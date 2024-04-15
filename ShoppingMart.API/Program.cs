using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ShoppingMart.API.Data_Access;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDataAccess, DataAccess>();//Step-3

builder.Services.AddCors(option => //13.This used when the CORS policy error is shown when two local Host are different
{
  option.AddPolicy("AllowPolicy", builder =>
  {
    builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin(); //This will allow any local host or headers or origin 
  });
});
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowOrigin", builder =>
  {
    builder.WithOrigins("http://localhost:4200")  // Add your allowed origins here
           .AllowAnyHeader()
           .AllowAnyMethod();
  });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
{
  x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = "localhost",
    ValidAudience = "localhost",
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("S6DgV73VrV4qcN9R4gWvuZRSYVYq69aZ0njDqXq6FZG2Eg6tEjFLfn8mxZhfrskSf7XHNst6ht44KbG8FOCzg")),
    ClockSkew = TimeSpan.Zero
  };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowOrigin");
app.UseEndpoints(endpoints =>
{
  endpoints.MapControllers(); // Map controller routes
});

app.UseHttpsRedirection();
app.UseAuthentication(); //use JWT Token
app.UseCors("AllowPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
