using Html2Sql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(
        options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft
                .Json
                .ReferenceLoopHandling
                .Ignore
    );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var conStr = app.Environment.IsDevelopment() ? builder.Configuration.GetConnectionString("Default") : Environment.GetEnvironmentVariable("CONNSTR");
builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(conStr));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
