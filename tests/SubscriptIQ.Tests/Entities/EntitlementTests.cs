using FluentAssertions;
using SubscriptIQ.Core.Entities;
using Xunit;

namespace SubscriptIQ.Tests.Entities;

public class EntitlementTests
{
    [Fact]
    public void CreateFeatureFlag_WithValidData_ShouldCreateEntitlement()
    {
        // Arrange
        var planId = Guid.NewGuid();

        // Act
        var entitlement = Entitlement.CreateFeatureFlag(planId, "advanced-analytics", isEnabled: true);

        // Assert
        entitlement.Should().NotBeNull();
        entitlement.PlanId.Should().Be(planId);
        entitlement.FeatureKey.Should().Be("advanced-analytics");
        entitlement.Type.Should().Be(EntitlementType.FeatureFlag);
        entitlement.IsEnabled.Should().BeTrue();
        entitlement.Limit.Should().BeNull();
    }

    [Fact]
    public void CreateUsageLimit_WithValidData_ShouldCreateEntitlement()
    {
        // Arrange
        var planId = Guid.NewGuid();

        // Act
        var entitlement = Entitlement.CreateUsageLimit(planId, "api-calls", limit: 10000);

        // Assert
        entitlement.Type.Should().Be(EntitlementType.UsageLimit);
        entitlement.Limit.Should().Be(10000);
        entitlement.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void CreateUsageLimit_WithNegativeLimit_ShouldThrowException()
    {
        // Arrange
        var planId = Guid.NewGuid();

        // Act
        Action act = () => Entitlement.CreateUsageLimit(planId, "api-calls", limit: -100);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void UpdateLimit_OnUsageLimit_ShouldUpdateSuccessfully()
    {
        // Arrange
        var entitlement = Entitlement.CreateUsageLimit(Guid.NewGuid(), "api-calls", limit: 1000);

        // Act
        entitlement.UpdateLimit(5000);

        // Assert
        entitlement.Limit.Should().Be(5000);
    }

    [Fact]
    public void UpdateLimit_OnFeatureFlag_ShouldThrowException()
    {
        // Arrange
        var entitlement = Entitlement.CreateFeatureFlag(Guid.NewGuid(), "feature", isEnabled: true);

        // Act
        Action act = () => entitlement.UpdateLimit(1000);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*usage limit entitlements*");
    }

    [Fact]
    public void Enable_ShouldSetIsEnabledToTrue()
    {
        // Arrange
        var entitlement = Entitlement.CreateFeatureFlag(Guid.NewGuid(), "feature", isEnabled: false);

        // Act
        entitlement.Enable();

        // Assert
        entitlement.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Disable_ShouldSetIsEnabledToFalse()
    {
        // Arrange
        var entitlement = Entitlement.CreateFeatureFlag(Guid.NewGuid(), "feature", isEnabled: true);

        // Act
        entitlement.Disable();

        // Assert
        entitlement.IsEnabled.Should().BeFalse();
    }
}
