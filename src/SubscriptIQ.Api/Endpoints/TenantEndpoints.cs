using Microsoft.AspNetCore.Mvc;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;

namespace SubscriptIQ.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .WithOpenApi();

        group.MapPost("/", async (
            [FromBody] CreateTenantRequest request,
            [FromServices] ITenantRepository tenantRepository,
            [FromServices] IUnitOfWork unitOfWork) =>
        {
            var tenant = Tenant.Create(request.Name);
            
            await tenantRepository.AddAsync(tenant);
            await unitOfWork.SaveChangesAsync();

            return Results.Created($"/api/tenants/{tenant.Id}", new
            {
                id = tenant.Id,
                name = tenant.Name,
                apiKey = tenant.ApiKey,
                isActive = tenant.IsActive,
                createdAt = tenant.CreatedAt
            });
        })
        .WithName("CreateTenant");

        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromServices] ITenantRepository tenantRepository) =>
        {
            var tenant = await tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return Results.NotFound();

            return Results.Ok(new
            {
                id = tenant.Id,
                name = tenant.Name,
                apiKey = tenant.ApiKey,
                isActive = tenant.IsActive,
                stripeCustomerId = tenant.StripeCustomerId,
                createdAt = tenant.CreatedAt,
                updatedAt = tenant.UpdatedAt
            });
        })
        .WithName("GetTenant");

        group.MapGet("/", async (
            [FromServices] ITenantRepository tenantRepository) =>
        {
            var tenants = await tenantRepository.GetAllAsync();
            return Results.Ok(tenants.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                isActive = t.IsActive,
                createdAt = t.CreatedAt
            }));
        })
        .WithName("GetAllTenants");
    }
}

public record CreateTenantRequest(string Name);
