using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;
using System.Collections.Generic;
using System.Linq;

namespace Smartwyre.DeveloperTest.Services;

public class RebateService : IRebateService
{
    private readonly IRebateDataStore _rebateDataStore;
    private readonly IProductDataStore _productDataStore;
    private readonly IEnumerable<IRebateCalculator> _calculators;

    public RebateService(
        IRebateDataStore rebateDataStore,
        IProductDataStore productDataStore,
        IEnumerable<IRebateCalculator> calculators)
    {
        _rebateDataStore = rebateDataStore;
        _productDataStore = productDataStore;
        _calculators = calculators;
    }

    public CalculateRebateResult Calculate(CalculateRebateRequest request)
    {
        var rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);
        var product = _productDataStore.GetProduct(request.ProductIdentifier);

        if (rebate == null || product == null)
            return new CalculateRebateResult { Success = false };

        var calculator = _calculators.FirstOrDefault(c => c.CanHandle(rebate.Incentive));

        if (calculator == null)
            return new CalculateRebateResult { Success = false };

        var rebateAmount = calculator.Calculate(rebate, product, request);

        if (rebateAmount == null)
            return new CalculateRebateResult { Success = false };

        _rebateDataStore.StoreCalculationResult(rebate, rebateAmount.Value);

        return new CalculateRebateResult
        {
            Success = true,
            RebateAmount = rebateAmount.Value
        };
    }
}
