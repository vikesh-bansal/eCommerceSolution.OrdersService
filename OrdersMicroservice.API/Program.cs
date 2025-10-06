using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

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

builder.Services.AddHttpClient<UsersMicroserviceClient>(client => { client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}"); });
builder.Services.AddHttpClient<ProductsMicroserviceClient>(client => { client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}"); });
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
