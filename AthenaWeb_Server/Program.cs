using Athena_DataAccess.Data;
using AthenaWeb_Server.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AthenaWeb_Server.Service.IService;
using AthenaWeb_Server.Service;
using MQTTnet.Client;
using MQTTnet;
using Athena_Business.Repository.IRepository;
using Athena_Business.Repository;
using Microsoft.AspNetCore.ResponseCompression;
using AthenaWeb_Server.Hubs;
using Syncfusion.Blazor;
using Athena_DataAccess;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTE0ODUyNEAzMjMwMmUzNDJlMzBkanVsRTFxNmNIekllUDZoRG8zMW5mWFhFQXp5RHVQV3gzdlJISGxWbldnPQ==");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddResponseCompression(opts =>
{
	opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
		new[] { "application/octet-stream" });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AthenaAppDbContext>(options =>
		options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
//builder.Services.AddDbContext<AthenaAppDbContext>(options =>
//{
//	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});
builder.Services.AddIdentity<AppUser, IdentityRole>()
	.AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<AthenaAppDbContext>();
//
var factory = new MqttFactory();
builder.Services.AddSingleton<IMqttClient>(factory.CreateMqttClient());
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IMqttMessageService, MqttMessageService>();
builder.Services.AddScoped<ICameraRepository, CameraRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventHeaderRepository, EventHeaderRepository>();
builder.Services.AddScoped<IEventVideoRepository, EventVideoRepository>();
builder.Services.AddScoped<IFCMInfoRepository, FCMInfoRepository>();

builder.Services.AddHostedService<HostedMqttMessageService>();

builder.Services.AddHttpClient();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddSyncfusionBlazor();

builder.WebHost.UseUrls("http://*:8100;https://*:8101");

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapHub<SignalingHub>("/hubs/signaling");
app.MapFallbackToPage("/_Host");

await SeedDatabase();

app.Run();

async ValueTask SeedDatabase()
{
	using (var scope = app.Services.CreateScope())
	{
		var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
		await dbInitializer.Initialize();
	}
}