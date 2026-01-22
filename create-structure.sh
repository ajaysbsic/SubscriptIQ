#!/bin/bash

# Create solution
dotnet new sln -n SubscriptIQ

# Create projects
dotnet new webapi -n SubscriptIQ.Api -o src/SubscriptIQ.Api --use-minimal-apis -f net8.0
dotnet new classlib -n SubscriptIQ.Core -o src/SubscriptIQ.Core -f net8.0
dotnet new classlib -n SubscriptIQ.Infrastructure -o src/SubscriptIQ.Infrastructure -f net8.0
dotnet new xunit -n SubscriptIQ.Tests -o tests/SubscriptIQ.Tests -f net8.0
dotnet new xunit -n SubscriptIQ.IntegrationTests -o tests/SubscriptIQ.IntegrationTests -f net8.0

# Add projects to solution
dotnet sln add src/SubscriptIQ.Api/SubscriptIQ.Api.csproj
dotnet sln add src/SubscriptIQ.Core/SubscriptIQ.Core.csproj
dotnet sln add src/SubscriptIQ.Infrastructure/SubscriptIQ.Infrastructure.csproj
dotnet sln add tests/SubscriptIQ.Tests/SubscriptIQ.Tests.csproj
dotnet sln add tests/SubscriptIQ.IntegrationTests/SubscriptIQ.IntegrationTests.csproj

# Add project references
dotnet add src/SubscriptIQ.Api/SubscriptIQ.Api.csproj reference src/SubscriptIQ.Core/SubscriptIQ.Core.csproj
dotnet add src/SubscriptIQ.Api/SubscriptIQ.Api.csproj reference src/SubscriptIQ.Infrastructure/SubscriptIQ.Infrastructure.csproj
dotnet add src/SubscriptIQ.Infrastructure/SubscriptIQ.Infrastructure.csproj reference src/SubscriptIQ.Core/SubscriptIQ.Core.csproj
dotnet add tests/SubscriptIQ.Tests/SubscriptIQ.Tests.csproj reference src/SubscriptIQ.Core/SubscriptIQ.Core.csproj
dotnet add tests/SubscriptIQ.IntegrationTests/SubscriptIQ.IntegrationTests.csproj reference src/SubscriptIQ.Api/SubscriptIQ.Api.csproj
dotnet add tests/SubscriptIQ.IntegrationTests/SubscriptIQ.IntegrationTests.csproj reference src/SubscriptIQ.Infrastructure/SubscriptIQ.Infrastructure.csproj
