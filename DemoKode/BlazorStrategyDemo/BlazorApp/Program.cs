using BlazorApp.Components;
using BlazorApp.Strategy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Alle strategier registreres bag det SAMME interface.
// Containeren kan så levere dem som en samling.
builder.Services.AddScoped<IDiscountStrategy, NoDiscount>();
builder.Services.AddScoped<IDiscountStrategy>(_ => new PercentageDiscount(0.10m));
builder.Services.AddScoped<IDiscountStrategy>(_ => new FixedAmountDiscount(50m, 300m));
builder.Services.AddScoped<IDiscountStrategy>(_ => new BlackFridayDiscount(0.25m));
builder.Services.AddScoped<IDiscountStrategy, KajSpecialDiscount>(_ => new KajSpecialDiscount(0.90m));
// Context'en. Den får AUTOMATISK en IEnumerable<IDiscountStrategy>
// med alle fire strategier ind i konstruktøren.
builder.Services.AddScoped<IPriceCalculator, PriceCalculator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
