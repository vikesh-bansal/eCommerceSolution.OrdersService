using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using Polly;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

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

builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

builder.Services.AddHttpClient<UsersMicroserviceClient>(client => { client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}"); }).
    AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy()).
    AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCircuitBreakerPolicy());
;
builder.Services.AddHttpClient<ProductsMicroserviceClient>(client => { client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}"); }).
    AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetFallbackPolicy()
    );
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
