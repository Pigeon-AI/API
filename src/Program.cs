using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PigeonAPI", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PigeonAPI v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

app.Run($"http://localhost:{port}");
