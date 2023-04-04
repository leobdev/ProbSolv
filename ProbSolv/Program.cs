using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ProbSolv.Data;
using ProbSolv.Models;
using ProbSolv.Services;
using ProbSolv.Services.Factories;
using ProbSolv.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var mailSettings = builder.Configuration.GetSection("MailSettings") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");



builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(DataUtility.GetConnectionString(builder.Configuration), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

/*builder.Services.AddDefaultIdentity<PSUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
*/

/*builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));*/

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<PSUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<PSUserClaimsPrincipalFactory>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();


builder.Services.AddScoped<IPSRolesService, PSRolesService>();
builder.Services.AddScoped<IPSCompanyInfoService, PSCompanyInfoService>();
builder.Services.AddScoped<IPSProjectService, PSProjectService>();
builder.Services.AddScoped<IPSTicketService, PSTicketService>();
builder.Services.AddScoped<IPSTicketHistoryService, PSTicketHistoryService>();
builder.Services.AddScoped<IPSNotificationService, PSNotificationService>();
builder.Services.AddScoped<IPSInviteService, PSInviteService>();
builder.Services.AddScoped<IPSFileService, PSFileService>();
builder.Services.AddScoped<IPSLookupService, PSLookupService>();
builder.Services.AddScoped<IEmailSender, PSEmailService>();
builder.Services.Configure<MailSettings>(mailSettings);
builder.Services.AddScoped<IPSBadgeService, PSBadgeService>();

builder.Services.AddDataProtection();


builder.Services.AddControllersWithViews();

var app = builder.Build();

//Allow datetime without tz to work
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


//Adds the preloaded data 
await DataUtility.ManageDataAsync(app);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");
app.MapRazorPages();

app.Run();
