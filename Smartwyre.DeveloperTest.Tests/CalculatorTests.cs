using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class CalculatorTests
{
    private static Rebate BuildRebate(IncentiveType incentive, decimal amount = 0m, decimal percentage = 0m)
        => new() { Incentive = incentive, Amount = amount, Percentage = percentage };

    private static Product BuildProduct(SupportedIncentiveType supported, decimal price = 0m)
        => new() { SupportedIncentives = supported, Price = price };

    private static CalculateRebateRequest BuildRequest(decimal volume = 0m)
        => new() { Volume = volume };

    // FixedCashAmount
    [Fact]
    public void FixedCashAmount_returns_the_amount_when_supported_and_positive()
    {
        var calculator = new FixedCashAmountCalculator();

        var result = calculator.Calculate(
            BuildRebate(IncentiveType.FixedCashAmount, amount: 100m),
            BuildProduct(SupportedIncentiveType.FixedCashAmount),
            BuildRequest());

        Assert.Equal(100m, result);
    }

    [Theory]
    [InlineData(SupportedIncentiveType.FixedRateRebate, 100)]
    [InlineData(SupportedIncentiveType.FixedCashAmount, 0)]
    public void FixedCashAmount_returns_null_for_invalid_scenarios(
        SupportedIncentiveType supported, decimal amount)
    {
        var calculator = new FixedCashAmountCalculator();

        var result = calculator.Calculate(
            BuildRebate(IncentiveType.FixedCashAmount, amount: amount),
            BuildProduct(supported),
            BuildRequest());

        Assert.Null(result);
    }

    // FixedRateRebate
    [Fact]
    public void FixedRateRebate_calculates_price_times_percentage_times_volume()
    {
        var calculator = new FixedRateRebateCalculator();

        var result = calculator.Calculate(
            BuildRebate(IncentiveType.FixedRateRebate, percentage: 0.1m),
            BuildProduct(SupportedIncentiveType.FixedRateRebate, price: 200m),
            BuildRequest(volume: 5m));

        Assert.Equal(100m, result);
    }

    [Theory]
    [InlineData(SupportedIncentiveType.FixedCashAmount, 0.1, 200, 5)]
    [InlineData(SupportedIncentiveType.FixedRateRebate, 0, 200, 5)]
    [InlineData(SupportedIncentiveType.FixedRateRebate, 0.1, 0, 5)]
    [InlineData(SupportedIncentiveType.FixedRateRebate, 0.1, 200, 0)]
    public void FixedRateRebate_returns_null_for_invalid_scenarios(
        SupportedIncentiveType supported, decimal percentage, decimal price, decimal volume)
    {
        var calculator = new FixedRateRebateCalculator();

        var result = calculator.Calculate(
            BuildRebate(IncentiveType.FixedRateRebate, percentage: percentage),
            BuildProduct(supported, price: price),
            BuildRequest(volume: volume));

        Assert.Null(result);
    }

    // AmountPerUom
    [Fact]
    public void AmountPerUom_calculates_amount_times_volume()
    {
        var calculator = new AmountPerUomCalculator();

        var result = calculator.Calculate(
            BuildRebate(IncentiveType.AmountPerUom, amount: 10m),
            BuildProduct(SupportedIncentiveType.AmountPerUom),
            BuildRequest(volume: 4m));

        Assert.Equal(40m, result);
    }

    [Theory]
    [InlineData(SupportedIncentiveType.FixedCashAmount, 10, 4)]
    [InlineData(SupportedIncentiveType.AmountPerUom, 0, 4)]
    [InlineData(SupportedIncentiveType.AmountPerUom, 10, 0)]
    public void AmountPerUom_returns_null_for_invalid_scenarios(
        SupportedIncentiveType supported, decimal amount, decimal volume)
    {
        var calculator = new AmountPerUomCalculator();

        var result = calculator.Calculate(
            BuildRebate(IncentiveType.AmountPerUom, amount: amount),
            BuildProduct(supported),
            BuildRequest(volume: volume));

        Assert.Null(result);
    }

    // CanHandle
    [Fact]
    public void Each_calculator_only_handles_its_own_incentive_type()
    {
        Assert.True(new FixedCashAmountCalculator().CanHandle(IncentiveType.FixedCashAmount));
        Assert.True(new FixedRateRebateCalculator().CanHandle(IncentiveType.FixedRateRebate));
        Assert.True(new AmountPerUomCalculator().CanHandle(IncentiveType.AmountPerUom));

        var fixedCash = new FixedCashAmountCalculator();
        Assert.False(fixedCash.CanHandle(IncentiveType.FixedRateRebate));
        Assert.False(fixedCash.CanHandle(IncentiveType.AmountPerUom));
    }
}