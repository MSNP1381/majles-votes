using Html2Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using trvotes.Models;

var builder = WebApplication.CreateBuilder(args);

var conStr =
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production"
        ? builder.Configuration.GetConnectionString("Default")
        : Environment.GetEnvironmentVariable("CONNSTR");
Console.WriteLine(conStr);
builder.Services.AddDbContext<MyDbContext>(options => { options.UseSqlServer(conStr); });

builder.Services.AddCors();

//builder.Services
//    .AddIdentityCore<RegisterModel>(options =>
//    {
//        options.SignIn.RequireConfirmedAccount = false;
//        options.Password.RequireDigit = false;
//        options.Password.RequiredLength = 6;
//        options.Password.RequireNonAlphanumeric = false;
//        options.Password.RequireUppercase = false;
//        options.Password.RequireLowercase = false;
//        options.User.RequireUniqueEmail = true;
//    })
//    .AddRoles<IdentityRole>()
//    .AddRoleManager<RoleManager<IdentityRole>>()
//    .AddEntityFrameworkStores<DataContext>();

builder.Configuration.AddEnvironmentVariables();
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(
        options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft
                .Json
                .ReferenceLoopHandling
                .Ignore
    );
//builder.Services
//    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
//            )
//        };
//    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "majels api", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.MapControllers();

app.UseCors(builder1 =>
{
    builder1.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});
app.Run();
