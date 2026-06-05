# Smartwyre Developer Test

## How to Run

### Console Runner
```bash
cd Smartwyre.DeveloperTest.Runner
dotnet run
```
The runner will prompt for a Rebate Identifier, a Product Identifier, and a Volume.

### Tests
```bash
cd Smartwyre.DeveloperTest.Tests
dotnet test
```

---

## Packages Used

| Package | Project | Purpose |
|---|---|---|
| `Microsoft.Extensions.DependencyInjection` | Runner | Build and configure the DI container |
| `Moq` | Tests | Mock dependencies in unit tests |

---

## What I Changed and Why

### Dependency Inversion in the data stores

The original `RebateService` was instantiating `RebateDataStore` and `ProductDataStore` directly with `new`, which made the service impossible to test in isolation. I introduced `IRebateDataStore` and `IProductDataStore` so that the service depends on abstractions rather than concrete implementations. The data stores themselves retain their original stub implementations, since actual database access was out of scope for this exercise — but the interfaces are ready to receive real implementations without any changes to the service layer.

### Fixing the null check bug

In the original code, `rebate.Incentive` was accessed inside the `switch` statement before the `rebate == null` guard, which would cause a `NullReferenceException` at runtime whenever the rebate was not found. In the refactored version, both `rebate` and `product` are checked upfront before any calculation is attempted.

### Duplicate `RebateDataStore` instantiation

The original code created a second instance of `RebateDataStore` at the end of the method just to call `StoreCalculationResult`. This was unnecessary and has been removed — the same injected instance handles both operations.

### Strategy pattern for incentive type calculators

The biggest structural change was replacing the `switch` statement with the Strategy pattern. The original design required modifying `RebateService` every time a new incentive type was introduced, which violates the Open/Closed Principle.

I created an `IRebateCalculator` interface with two methods: `CanHandle`, which tells the service whether a calculator is responsible for a given incentive type, and `Calculate`, which performs the calculation and returns `decimal?` — returning `null` to indicate that the combination of rebate, product and request is invalid (for example, a zero percentage or an unsupported incentive type).

Each of the three existing incentive types has its own calculator class inside `Services/Calculators/`:

| Calculator | Incentive Type | Formula |
|---|---|---|
| `FixedCashAmountCalculator` | `FixedCashAmount` | `Rebate.Amount` |
| `FixedRateRebateCalculator` | `FixedRateRebate` | `Price × Percentage × Volume` |
| `AmountPerUomCalculator` | `AmountPerUom` | `Amount × Volume` |

To add a new incentive type in the future, a developer only needs to create a new class implementing `IRebateCalculator` and register it in the DI container. No existing code needs to be touched.

### RebateService as an orchestrator

With the calculators handling the business rules for each incentive type, `RebateService` became a clean orchestrator: fetch the rebate, fetch the product, guard against nulls, find the right calculator, delegate the calculation, and persist the result if successful. The list of calculators is injected via the constructor as `IEnumerable<IRebateCalculator>`, which is how `Microsoft.Extensions.DependencyInjection` resolves multiple registrations of the same interface.

### Adding `RebateAmount` to the result

The original `CalculateRebateResult` only exposed a `Success` flag, discarding the calculated amount after persisting it. I added a `RebateAmount` property to the result so that callers — and tests — can access the value that was computed and stored.

---

## Unit Tests

Tests are organized into two classes. `CalculatorTests` covers each calculator in isolation, using `[Theory]` with `[InlineData]` to group invalid scenarios and avoid repetition. `RebateServiceTests` covers the orchestration logic of `RebateService` using mocked dependencies via Moq, verifying guard clauses, successful calculation, and whether `StoreCalculationResult` is called or not depending on the outcome.