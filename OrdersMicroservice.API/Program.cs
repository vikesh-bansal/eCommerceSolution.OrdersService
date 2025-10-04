using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddDomainLogicLayer(builder.Configuration);

builder.Services.AddControllers();

//swagger
builder.Services.AddSwaggerGen();

builder.Services.AddEndpointsApiExplorer();

//Cors
builder.Services.AddCors(options => { options.AddDefaultPolicy(builder => { builder.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader(); }); });
var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseRouting();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

//Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
