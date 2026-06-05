using Moq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class RebateServiceTests
{
    // Helpers to build the service with controlled dependencies
    private static (RebateService Service, Mock<IRebateDataStore> RebateStore) BuildService(
        Rebate rebate,
        Product product,
        decimal? calculatorResult = null,
        bool calculatorCanHandle = true)
    {
        var rebateStore = new Mock<IRebateDataStore>();
        rebateStore.Setup(s => s.GetRebate(It.IsAny<string>())).Returns(rebate);

        var productStore = new Mock<IProductDataStore>();
        productStore.Setup(s => s.GetProduct(It.IsAny<string>())).Returns(product);

        var calculator = new Mock<IRebateCalculator>();
        calculator.Setup(c => c.CanHandle(It.IsAny<IncentiveType>())).Returns(calculatorCanHandle);
        calculator.Setup(c => c.Calculate(It.IsAny<Rebate>(), It.IsAny<Product>(), It.IsAny<CalculateRebateRequest>()))
                  .Returns(calculatorResult);

        var service = new RebateService(rebateStore.Object, productStore.Object, new[] { calculator.Object });
        return (service, rebateStore);
    }

    // Guard clauses
    [Fact]
    public void Calculate_fails_when_rebate_is_not_found()
    {
        var (service, _) = BuildService(rebate: null, product: new Product());

        var result = service.Calculate(new CalculateRebateRequest());

        Assert.False(result.Success);
    }

    [Fact]
    public void Calculate_fails_when_product_is_not_found()
    {
        var (service, _) = BuildService(rebate: new Rebate(), product: null);

        var result = service.Calculate(new CalculateRebateRequest());

        Assert.False(result.Success);
    }

    [Fact]
    public void Calculate_fails_when_no_calculator_handles_the_incentive_type()
    {
        var rebate = new Rebate { Incentive = IncentiveType.FixedCashAmount };
        var (service, _) = BuildService(rebate, new Product(), calculatorCanHandle: false);

        var result = service.Calculate(new CalculateRebateRequest());

        Assert.False(result.Success);
    }

    [Fact]
    public void Calculate_fails_when_calculator_returns_null()
    {
        var rebate = new Rebate { Incentive = IncentiveType.FixedCashAmount };
        var (service, _) = BuildService(rebate, new Product(), calculatorResult: null);

        var result = service.Calculate(new CalculateRebateRequest());

        Assert.False(result.Success);
    }

    // calculation is valid

    [Fact]
    public void Calculate_succeeds_and_returns_the_amount_when_calculation_is_valid()
    {
        var rebate = new Rebate { Incentive = IncentiveType.FixedCashAmount };
        var (service, _) = BuildService(rebate, new Product(), calculatorResult: 250m);

        var result = service.Calculate(new CalculateRebateRequest());

        Assert.True(result.Success);
        Assert.Equal(250m, result.RebateAmount);
    }

    [Fact]
    public void Calculate_persists_the_result_once_when_calculation_succeeds()
    {
        var rebate = new Rebate { Incentive = IncentiveType.FixedCashAmount };
        var (service, rebateStore) = BuildService(rebate, new Product(), calculatorResult: 250m);

        service.Calculate(new CalculateRebateRequest());

        rebateStore.Verify(s => s.StoreCalculationResult(rebate, 250m), Times.Once);
    }

    [Fact]
    public void Calculate_does_not_persist_anything_when_calculation_fails()
    {
        var rebate = new Rebate { Incentive = IncentiveType.FixedCashAmount };
        var (service, rebateStore) = BuildService(rebate, new Product(), calculatorResult: null);

        service.Calculate(new CalculateRebateRequest());

        rebateStore.Verify(
            s => s.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()),
            Times.Never);
    }
}
