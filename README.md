

<div align="center">
  <img src="Ruleflow.NET/Engine/Images/Ruleflow.NET.png" alt="Ruleflow.NET Logo">
</div>

## 🌟 Overview

Ruleflow.NET is a flexible, high-performance business rules and validation framework for .NET applications. Built with modern C# features and a fluent API design, Ruleflow.NET helps you create, manage, and execute complex business logic and validation rules with minimal code and maximum readability.

> ⚠️ **Note:** Ruleflow.NET is currently under active development. While core functionality is stable, some features may change before the final release.

## ✨ Key Features

- **Intuitive Fluent API** - Create complex validation rules with a natural, readable syntax
- **Conditional Logic** - Build sophisticated rule flows with if/then/else and switch expressions
- **Rule Dependencies** - Define rules that depend on the results of other rules
- **Prioritized Execution** - Control the order of rule evaluation with priority settings
- **Comprehensive Results** - Get detailed validation results with configurable severity levels
- **Dependency Awareness** - Built-in dependency graph validation to prevent circular references
- **Clean Separation** - Keep your business logic separate from your application code

## 🚀 Getting Started

### Installation

```bash
# Coming soon to NuGet!
dotnet add package Ruleflow.NET
```

### Basic Usage

```csharp
// Create a simple validation rule
var ageRule = RuleflowExtensions.CreateRule<Person>()
    .WithAction(p => {
        if (p.Age < 18)
            throw new ArgumentException("Person must be at least 18 years old");
    })
    .WithMessage("Age validation failed")
    .WithSeverity(ValidationSeverity.Error)
    .Build();

// Create a validator with our rule
var validator = new Validator<Person>(new[] { ageRule });

// Validate a person
var person = new Person { Name = "John", Age = 17 };
var result = validator.CollectValidationResults(person);

// Check the result
if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.Severity}: {error.Message}");
    }
}
```

### Advanced Validation with Conditional Logic

```csharp
// Create a conditional validation rule
var drivingRule = RuleflowExtensions
    .CreateConditionalRule<Person>(p => p.HasDrivingLicense)
    .Then(builder => builder
        .WithAction(p => {
            if (p.Age < 16)
                throw new ArgumentException("Driving license holders must be at least 16");
        })
        .WithMessage("Invalid driving license")
    )
    .Else(builder => builder
        .WithAction(p => {
            if (p.Age < 13)
                throw new ArgumentException("Non-drivers must be at least 13");
        })
        .WithMessage("Age validation failed")
    )
    .Build();
```

## 🔧 Advanced Features

### Dependent Rules

```csharp
// Create rules with dependencies
var primaryRule = RuleflowExtensions.CreateRule<Order>()
    .WithId("PrimaryCheck")
    .WithAction(o => {
        if (o.Amount <= 0)
            throw new ArgumentException("Order amount must be positive");
    })
    .Build();

var dependentRule = RuleflowExtensions.CreateDependentRule<Order>("SecondaryCheck")
    .DependsOn("PrimaryCheck")
    .WithDependencyType(DependencyType.RequiresAllSuccess)
    .WithAction(o => {
        if (o.Items.Count == 0)
            throw new ArgumentException("Order must have at least one item");
    })
    .Build();
```

### Switch Pattern Rules

```csharp
// Create a switch-based rule
var statusRule = RuleflowExtensions
    .CreateSwitchRule<Order, OrderStatus>(o => o.Status)
    .Case(OrderStatus.Draft, builder => builder
        .WithAction(o => {
            if (o.DraftDate == null)
                throw new ArgumentException("Draft orders must have a draft date");
        })
    )
    .Case(OrderStatus.Submitted, builder => builder
        .WithAction(o => {
            if (o.SubmissionDate == null)
                throw new ArgumentException("Submitted orders must have a submission date");
        })
    )
    .Default(builder => builder
        .WithAction(o => {
            if (o.LastModified == null)
                throw new ArgumentException("All orders must have a last modified date");
        })
    )
    .Build();
```

## 🏗️ Architecture

Ruleflow.NET is designed around a set of core interfaces and components:

- **`IValidationRule<T>`** - Base interface for all validation rules
- **`IValidator<T>`** - Interface for validators that can validate objects of type T
- **`IValidationResult`** - Contains the results of a validation operation
- **`ValidationRuleBuilder<T>`** - Fluent API for constructing validation rules
- **`DependencyAwareValidator<T>`** - Validator that supports rule dependencies
- **`ValidationContext`** - Context for validation operations, including results of rule evaluations

## 🌐 Use Cases

- **Form Validation** - Validate user input with complex business rules
- **API Request Validation** - Ensure incoming requests meet your requirements
- **Business Rule Processing** - Execute business logic in a structured, maintainable way
- **Workflow Validation** - Verify that each step in a workflow can proceed
- **Data Integrity Checks** - Ensure your data meets your business constraints

## 📋 Roadmap
- [ ] Coming soon!

## 📚 Documentation

Comprehensive documentation is in progress and will be available soon.

## 🤝 Contributing

Contributions are welcome! Feel free to submit issues or pull requests to help improve Ruleflow.NET.

## 📄 License

Ruleflow.NET is licensed under the [Apache License 2.0](LICENSE.txt).

---

Made with ❤️
