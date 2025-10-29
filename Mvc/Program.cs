using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mvc;
using Mvc.Data;
using Mvc.Models;
using Mvc.Repository;
using Mvc.Services;
using System.Text;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
#if UseJWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "輸入 JWT: Bearer {token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer",
                }
            },
            new string[]{ }
        }
    });
#endif
});
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// 本地sqlite 正式postgres
if (builder.Environment.IsDevelopment())
{
#if DEBUG
    var connectionString = builder.Configuration.GetConnectionString("Local");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));
#else
    var connectionString = builder.Configuration.GetConnectionString("RenderPostgreSQL");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));
#endif
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("RenderPostgreSQL");
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));
}

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Password Hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

#if DEBUG
builder.Services.AddScoped<IEmailService, EmailService>();
#else
builder.Services.AddScoped<IEmailService, ResendEmailService>();
#endif

builder.Services.AddSession();

#if UseJWT
// JWT 認證
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt => 
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = "mutiapp",
        ValidAudience = "mutiapp",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Oh4KfzBbM0FORVHZ5KUhd70OChpXVtae"))
    };
});
#else
// 加入 cookie 認證
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";   // 未登入時導向登入頁
        options.LogoutPath = "/Auth/Logout"; // 登出後導向頁面
    });
#endif

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/Status/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 預設路由格式
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
