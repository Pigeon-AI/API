using Microsoft.OpenApi.Models;
using PigeonAPI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PigeonAPI", Version = "v1" });
});

builder.Logging.AddSimpleConsole(options =>
                    {
                        // needs this or docker logs get super messed up
                        options.SingleLine = false;
                    });

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PigeonAPI v1"));
// }

app.UseHttpsRedirection();

app.UseAuthorization();

// map the different api controllers
app.MapControllers();

// ensure that the database is created/connected to properly
using(var db = new DatabaseAccess(app.Logger))
{
    // I don't really know what I'm doing but this works
    db.Database.EnsureCreated();
    // db.Database.Migrate();
};

// must be last thing in this file
app.Run();
