using FluentAssertions;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.ValueObjects;
using Xunit;

namespace SubscriptIQ.Tests.Entities;

public class SubscriptionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSubscription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        // Act
        var subscription = Subscription.Create(tenantId, planId, trialDays: 14);

        // Assert
        subscription.Should().NotBeNull();
        subscription.TenantId.Should().Be(tenantId);
        subscription.PlanId.Should().Be(planId);
        subscription.Status.Should().Be(SubscriptionStatus.Trial);
        subscription.TrialEndDate.Should().NotBeNull();
        subscription.TrialEndDate.Should().BeAfter(DateTime.UtcNow);
        subscription.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithoutTrial_ShouldCreateActiveSubscription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        // Act
        var subscription = Subscription.Create(tenantId, planId);

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.TrialEndDate.Should().BeNull();
    }

    [Fact]
    public void Cancel_WhenActive_ShouldCancelSuccessfully()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid());
        subscription.ClearDomainEvents();

        // Act
        var result = subscription.Cancel("Customer requested", immediately: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.CancelledAt.Should().NotBeNull();
        subscription.EndDate.Should().Be(subscription.CurrentPeriodEnd);
        subscription.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Cancel_WithImmediateFlag_ShouldCancelImmediately()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = subscription.Cancel("Customer requested", immediately: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldReturnFailure()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid());
        subscription.Cancel("Test");

        // Act
        var result = subscription.Cancel("Test again");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already cancelled");
    }

    [Fact]
    public void Upgrade_WhenActive_ShouldUpgradeSuccessfully()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid());
        var newPlanId = Guid.NewGuid();
        subscription.ClearDomainEvents();

        // Act
        var result = subscription.Upgrade(newPlanId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.PlanId.Should().Be(newPlanId);
        subscription.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Upgrade_ToSamePlan_ShouldReturnFailure()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var subscription = Subscription.Create(Guid.NewGuid(), planId);

        // Act
        var result = subscription.Upgrade(planId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("same plan");
    }

    [Fact]
    public void Downgrade_WhenActive_ShouldScheduleDowngrade()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid());
        subscription.Activate();
        var newPlanId = Guid.NewGuid();
        var effectiveDate = subscription.CurrentPeriodEnd;

        // Act
        var result = subscription.Downgrade(newPlanId, effectiveDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.PendingDowngradePlanId.Should().Be(newPlanId);
        subscription.PlanId.Should().NotBe(newPlanId); // Not downgraded yet
    }

    [Fact]
    public void Renew_WhenActive_ShouldRenewSuccessfully()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid());
        subscription.Activate();
        var nextBillingDate = DateTime.UtcNow.AddMonths(1);
        var oldPeriodEnd = subscription.CurrentPeriodEnd;

        // Act
        var result = subscription.Renew(nextBillingDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.CurrentPeriodStart.Should().Be(oldPeriodEnd);
        subscription.CurrentPeriodEnd.Should().Be(nextBillingDate);
    }

    [Fact]
    public void Renew_WithPendingDowngrade_ShouldApplyDowngrade()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid());
        subscription.Activate();
        var newPlanId = Guid.NewGuid();
        subscription.Downgrade(newPlanId, subscription.CurrentPeriodEnd);
        var nextBillingDate = DateTime.UtcNow.AddMonths(1);

        // Act
        subscription.Renew(nextBillingDate);

        // Assert
        subscription.PlanId.Should().Be(newPlanId);
        subscription.PendingDowngradePlanId.Should().BeNull();
    }

    [Fact]
    public void IsActive_WhenInTrial_ShouldReturnTrue()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), trialDays: 14);

        // Act & Assert
        subscription.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsInTrial_WhenTrialActive_ShouldReturnTrue()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), trialDays: 14);

        // Act & Assert
        subscription.IsInTrial().Should().BeTrue();
    }
}
