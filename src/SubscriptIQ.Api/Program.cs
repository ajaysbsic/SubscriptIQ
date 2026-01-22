using SubscriptIQ.Infrastructure;
using SubscriptIQ.Api.Endpoints;
using Stripe;
using CoreSubscriptionService = SubscriptIQ.Core.Services.SubscriptionService;
using CoreEntitlementService = SubscriptIQ.Core.Services.EntitlementService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "SubscriptIQ API", 
        Version = "v1",
        Description = "Plug-and-play subscription, billing, and entitlement platform"
    });
});

// Add Infrastructure services (DbContext, Repositories, Background Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Core services
builder.Services.AddScoped<CoreSubscriptionService>();
builder.Services.AddScoped<CoreEntitlementService>();

// Configure Stripe
var stripeApiKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeApiKey))
{
    StripeConfiguration.ApiKey = stripeApiKey;
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Map API endpoints
app.MapHealthEndpoints();
app.MapTenantEndpoints();
app.MapPlanEndpoints();
app.MapSubscriptionEndpoints();
app.MapEntitlementEndpoints();
app.MapWebhookEndpoints();

app.Run();
