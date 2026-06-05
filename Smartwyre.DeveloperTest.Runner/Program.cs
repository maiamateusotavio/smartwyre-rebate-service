using Microsoft.Extensions.DependencyInjection;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;
using System;

namespace Smartwyre.DeveloperTest.Runner;

class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IRebateDataStore, RebateDataStore>()
            .AddSingleton<IProductDataStore, ProductDataStore>()
            .AddSingleton<IRebateCalculator, FixedCashAmountCalculator>()
            .AddSingleton<IRebateCalculator, FixedRateRebateCalculator>()
            .AddSingleton<IRebateCalculator, AmountPerUomCalculator>()
            .AddSingleton<IRebateService, RebateService>()
            .BuildServiceProvider();

        var rebateService = serviceProvider.GetRequiredService<IRebateService>();

        Console.Write("Enter Rebate Identifier: ");
        var rebateIdentifier = Console.ReadLine();

        Console.Write("Enter Product Identifier: ");
        var productIdentifier = Console.ReadLine();

        Console.Write("Enter Volume: ");
        if (!decimal.TryParse(Console.ReadLine(), out var volume))
        {
            Console.WriteLine("Invalid volume. Please enter a numeric value.");
            return;
        }

        var request = new CalculateRebateRequest
        {
            RebateIdentifier = rebateIdentifier,
            ProductIdentifier = productIdentifier,
            Volume = volume
        };

        var result = rebateService.Calculate(request);

        Console.WriteLine();
        if (result.Success)
        {
            Console.WriteLine($"Calculation successful!");
            Console.WriteLine($"Rebate Amount: {result.RebateAmount:C}");
        }
        else
        {
            Console.WriteLine("Calculation failed. The rebate could not be applied.");
        }
    }
}