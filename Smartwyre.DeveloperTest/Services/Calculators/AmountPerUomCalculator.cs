using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services.Calculators;

public class AmountPerUomCalculator : IRebateCalculator
{
    public bool CanHandle(IncentiveType incentiveType)
        => incentiveType == IncentiveType.AmountPerUom;

    public decimal? Calculate(Rebate rebate, Product product, CalculateRebateRequest request)
    {
        if (!product.SupportedIncentives.HasFlag(SupportedIncentiveType.AmountPerUom))
            return null;

        if (rebate.Amount == 0 || request.Volume == 0)
            return null;

        return rebate.Amount * request.Volume;
    }
}
