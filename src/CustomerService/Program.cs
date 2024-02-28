using CustomerService.Contracts;
using CustomerService.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ICustomersService, CustomersService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
     options =>
     {
         options.SwaggerDoc("v1", new OpenApiInfo()
         {
             Title = "Customer services",
             Version = "v1",
             Description = "The customer data service api",
         });
         var path = Path.Combine(AppContext.BaseDirectory, "CustomerService.xml");
         options.IncludeXmlComments(path, true);
         options.OrderActionsBy(_ => _.RelativePath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();

app.Run();
