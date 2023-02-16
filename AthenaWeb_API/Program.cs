using Athena_Business.Repository;
using Athena_Business.Repository.IRepository;
using Athena_DataAccess.Data;
using AthenaWeb_API.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "AthenaWeb_API", Version = "v1.0.0" });
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Bearer Token을 입력해 주세요.",
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey
	});
	c.AddSecurityRequirement(new OpenApiSecurityRequirement {
				   {
					 new OpenApiSecurityScheme
					 {
					   Reference = new OpenApiReference
					   {
						 Type = ReferenceType.SecurityScheme,
						 Id = "Bearer"
					   }
					  },
					  new string[] { }
					}
				});
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AthenaAppDbContext>(options =>
		options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
//builder.Services.AddDbContext<AthenaAppDbContext>(options =>
//{
//	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
	.AddDefaultTokenProviders().AddEntityFrameworkStores<AthenaAppDbContext>();

builder.Services.AddScoped<IEventRepository, EventRepository>();

var apiSettingsSection = builder.Configuration.GetSection("APISettings");
builder.Services.Configure<APISettings>(apiSettingsSection);

var apiSettings = apiSettingsSection.Get<APISettings>();
var key = Encoding.ASCII.GetBytes(apiSettings.SecretKey);
builder.Services.AddAuthentication(opt =>
{
	opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
	x.RequireHttpsMetadata = false;
	x.SaveToken = true;
	x.TokenValidationParameters = new()
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateAudience = true,
		ValidateIssuer = true,
		ValidAudience = apiSettings.ValidAudience,
		ValidIssuer = apiSettings.ValidIssuer,
		ClockSkew = TimeSpan.Zero
	};
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddCors(o => o.AddPolicy("Athena", builder =>
{
	builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

builder.WebHost.UseUrls("http://*:8094;https://*:8095");

var app = builder.Build();

app.UseSwagger();
if (!app.Environment.IsDevelopment())
{
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "AthenaWeb_API");
		c.RoutePrefix = String.Empty;
	});
}
else
{
	app.UseSwaggerUI(c =>
	{
	});
}

//app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("Athena");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
