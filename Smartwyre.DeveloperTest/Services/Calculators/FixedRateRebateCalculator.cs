using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services.Calculators;

public class FixedRateRebateCalculator : IRebateCalculator
{
    public bool CanHandle(IncentiveType incentiveType)
        => incentiveType == IncentiveType.FixedRateRebate;

    public decimal? Calculate(Rebate rebate, Product product, CalculateRebateRequest request)
    {
        if (!product.SupportedIncentives.HasFlag(SupportedIncentiveType.FixedRateRebate))
            return null;

        if (rebate.Percentage == 0 || product.Price == 0 || request.Volume == 0)
            return null;

        return product.Price * rebate.Percentage * request.Volume;
    }
}
