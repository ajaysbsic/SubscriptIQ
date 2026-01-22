#!/bin/bash

# Infrastructure packages
dotnet add src/SubscriptIQ.Infrastructure/SubscriptIQ.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/SubscriptIQ.Infrastructure/SubscriptIQ.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/SubscriptIQ.Infrastructure/SubscriptIQ.Infrastructure.csproj package Stripe.net

# API packages
dotnet add src/SubscriptIQ.Api/SubscriptIQ.Api.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/SubscriptIQ.Api/SubscriptIQ.Api.csproj package Swashbuckle.AspNetCore

# Test packages
dotnet add tests/SubscriptIQ.Tests/SubscriptIQ.Tests.csproj package Moq
dotnet add tests/SubscriptIQ.Tests/SubscriptIQ.Tests.csproj package FluentAssertions
dotnet add tests/SubscriptIQ.IntegrationTests/SubscriptIQ.IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/SubscriptIQ.IntegrationTests/SubscriptIQ.IntegrationTests.csproj package Testcontainers.PostgreSql
dotnet add tests/SubscriptIQ.IntegrationTests/SubscriptIQ.IntegrationTests.csproj package FluentAssertions
