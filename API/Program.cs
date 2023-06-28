using System.Text.Json;
using System.Text.Json.Serialization;
using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// if(builder.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage();
// }

// Configure the HTTP request pipeline.

app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();
            
app.MapControllers();

using var scope=app.Services.CreateScope();
var services=scope.ServiceProvider;
try
{
    var context=services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(context);
}
catch (Exception ex)
{
    
    var logger=services.GetService<ILogger<Program>>();
    logger.LogError(ex,"An Error occered during migration");
}

app.Run(); 
