using FluentAssertions;
using SubscriptIQ.Core.ValueObjects;
using Xunit;

namespace SubscriptIQ.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoney()
    {
        // Act
        var money = Money.Create(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        // Act
        Action act = () => Money.Create(-10, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Create_WithNullCurrency_ShouldThrowException()
    {
        // Act
        Action act = () => Money.Create(100, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency is required*");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(50, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(50, "EUR");

        // Act
        Action act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldSubtractAmounts()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(30, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_ShouldMultiplyAmount()
    {
        // Arrange
        var money = Money.Create(25, "USD");

        // Act
        var result = money.Multiply(4);

        // Assert
        result.Amount.Should().Be(100);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.Create(123.45m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("123.45 USD");
    }

    [Fact]
    public void Zero_ShouldCreateZeroMoney()
    {
        // Act
        var money = Money.Zero("EUR");

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("EUR");
    }
}
