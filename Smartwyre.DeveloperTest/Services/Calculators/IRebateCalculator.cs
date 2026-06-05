using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services.Calculators;

public interface IRebateCalculator
{
    bool CanHandle(IncentiveType incentiveType);

    decimal? Calculate(Rebate rebate, Product product, CalculateRebateRequest request);
}
