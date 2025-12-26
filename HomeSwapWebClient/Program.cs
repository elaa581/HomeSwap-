var builder = WebApplication.CreateBuilder(args);
// MVC
builder.Services.AddControllersWithViews();

// SESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authentication (obligatoire si tu utilises UseAuthentication)
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
    });

// HttpClient vers l’API
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7122/");
});

var app = builder.Build();

// PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// Avant app.Run()
app.UseStaticFiles();


app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();   // autorisation

// ROUTE PAR DÉFAUT ? LOGIN
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();


