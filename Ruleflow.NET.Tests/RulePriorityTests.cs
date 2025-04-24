// Modified RulePriorityTests implementation
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Core.Validators;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Tests
{
    [TestClass]
    public class RulePriorityTests
    {
        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public bool IsDiscounted { get; set; }
            public decimal DiscountPercentage { get; set; }
            public DateTime? ReleaseDate { get; set; }
        }

        [TestMethod]
        public void Rules_ExecutedInPriorityOrder_HigherFirst()
        {
            // Arrange
            // Create a helper class to track execution order
            var executionTracker = new List<string>();

            // Rule with normal priority (0)
            var normalPriorityRule = RuleflowExtensions.CreateRule<Product>()
                .WithId("NormalPriority")
                .WithAction(p => {
                    executionTracker.Add("NormalPriority");
                    if (string.IsNullOrEmpty(p.Name))
                        throw new ArgumentException("Name is required");
                })
                .Build();

            // Rule with high priority (10)
            var highPriorityRule = RuleflowExtensions.CreateRule<Product>()
                .WithId("HighPriority")
                .WithPriority(10)
                .WithAction(p => {
                    executionTracker.Add("HighPriority");
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            // Rule with low priority (-5)
            var lowPriorityRule = RuleflowExtensions.CreateRule<Product>()
                .WithId("LowPriority")
                .WithPriority(-5)
                .WithAction(p => {
                    executionTracker.Add("LowPriority");
                    if (p.Stock < 0)
                        throw new ArgumentException("Stock cannot be negative");
                })
                .Build();

            // Explicitly set the order so tests are predictable - this is important!
            // Order them intentionally NOT in priority order to verify the priority sorting works
            var rules = new List<IValidationRule<Product>>
            {
                normalPriorityRule,
                lowPriorityRule,
                highPriorityRule
            };

            var validator = new DependencyAwareValidator<Product>(rules);

            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 19.99m,
                Stock = 10
            };

            // Act - Clear execution tracker before validation
            executionTracker.Clear();
            validator.CollectValidationResults(product);

            // Assert
            // Expected order: HighPriority (10), NormalPriority (0), LowPriority (-5)
            Assert.AreEqual(3, executionTracker.Count);
            Assert.AreEqual("HighPriority", executionTracker[0]);
            Assert.AreEqual("NormalPriority", executionTracker[1]);
            Assert.AreEqual("LowPriority", executionTracker[2]);
        }

        [TestMethod]
        public void Rules_WithSamePriority_ExecutedInDefinitionOrder()
        {
            // Arrange
            var executionTracker = new List<string>();

            // Three rules with the same priority
            var rule1 = RuleflowExtensions.CreateRule<Product>()
                .WithId("Rule1")
                .WithPriority(5)
                .WithAction(p => {
                    executionTracker.Add("Rule1");
                })
                .Build();

            var rule2 = RuleflowExtensions.CreateRule<Product>()
                .WithId("Rule2")
                .WithPriority(5)
                .WithAction(p => {
                    executionTracker.Add("Rule2");
                })
                .Build();

            var rule3 = RuleflowExtensions.CreateRule<Product>()
                .WithId("Rule3")
                .WithPriority(5)
                .WithAction(p => {
                    executionTracker.Add("Rule3");
                })
                .Build();

            // The order in the collection should be maintained for rules with the same priority
            var validator = new DependencyAwareValidator<Product>(
                new[] { rule1, rule2, rule3 });

            var product = new Product { Id = 1, Name = "Test" };

            // Act - Clear execution tracker before validation
            executionTracker.Clear();
            validator.CollectValidationResults(product);

            // Assert - Should match the order they were added to the validator
            Assert.AreEqual(3, executionTracker.Count);
            Assert.AreEqual("Rule1", executionTracker[0]);
            Assert.AreEqual("Rule2", executionTracker[1]);
            Assert.AreEqual("Rule3", executionTracker[2]);
        }

        [TestMethod]
        public void EarlyFailingRule_DoesNotPreventExecution_OfHigherPriorityRules()
        {
            // Arrange
            var executionTracker = new List<string>();

            // Low priority rule that will fail
            var lowPriorityRule = RuleflowExtensions.CreateRule<Product>()
                .WithId("LowPriority")
                .WithPriority(1)
                .WithAction(p => {
                    executionTracker.Add("LowPriority");
                    throw new ArgumentException("Low priority rule failed");
                })
                .Build();

            // Medium priority rule
            var mediumPriorityRule = RuleflowExtensions.CreateRule<Product>()
                .WithId("MediumPriority")
                .WithPriority(5)
                .WithAction(p => {
                    executionTracker.Add("MediumPriority");
                })
                .Build();

            // High priority rule
            var highPriorityRule = RuleflowExtensions.CreateRule<Product>()
                .WithId("HighPriority")
                .WithPriority(10)
                .WithAction(p => {
                    executionTracker.Add("HighPriority");
                })
                .Build();

            // The order in the collection is intentionally not in priority order
            var rules = new List<IValidationRule<Product>>
            {
                lowPriorityRule,
                highPriorityRule,
                mediumPriorityRule
            };

            var validator = new DependencyAwareValidator<Product>(rules);

            var product = new Product { Id = 1, Name = "Test" };

            // Act - Clear execution tracker before validation
            executionTracker.Clear();
            var result = validator.CollectValidationResults(product);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Low priority rule failed", result.Errors[0].Message);

            // All rules should have executed in priority order despite the failure
            Assert.AreEqual(3, executionTracker.Count);
            Assert.AreEqual("HighPriority", executionTracker[0]);
            Assert.AreEqual("MediumPriority", executionTracker[1]);
            Assert.AreEqual("LowPriority", executionTracker[2]);
        }

        [TestMethod]
        public void PriorityIsPreserved_ForDependentRules()
        {
            // Arrange
            var executionTracker = new List<string>();

            // Base rule with medium priority
            var baseRule = RuleflowExtensions.CreateRule<Product>()
                .WithId("BaseRule")
                .WithPriority(5)
                .WithAction(p => {
                    executionTracker.Add("BaseRule");
                })
                .Build();

            // High priority rule
            var highPriorityRule = RuleflowExtensions.CreateRule<Product>()
                .WithId("HighPriority")
                .WithPriority(10)
                .WithAction(p => {
                    executionTracker.Add("HighPriority");
                })
                .Build();

            // Dependent rule with low priority, depends on base rule
            var dependentRule = RuleflowExtensions.CreateDependentRule<Product>("DependentRule")
                .DependsOn("BaseRule")
                .WithPriority(1) // Low priority
                .WithAction(p => {
                    executionTracker.Add("DependentRule");
                })
                .Build();

            // Create validator with mixed order
            var rules = new List<IValidationRule<Product>>
            {
                dependentRule,
                highPriorityRule,
                baseRule
            };

            var validator = new DependencyAwareValidator<Product>(rules);

            var product = new Product { Id = 1, Name = "Test" };

            // Act - Clear execution tracker before validation
            executionTracker.Clear();
            validator.CollectValidationResults(product);

            // Assert
            // Expected order based on priority: HighPriority, BaseRule, DependentRule
            Assert.AreEqual(3, executionTracker.Count);
            Assert.AreEqual("HighPriority", executionTracker[0]);
            Assert.AreEqual("BaseRule", executionTracker[1]);
            Assert.AreEqual("DependentRule", executionTracker[2]);
        }

        [TestMethod]
        public void ConditionalRule_ExecutionOrderRespectsPriority()
        {
            // Arrange
            var executionTracker = new List<string>();

            // Low priority conditional rule
            var lowPriorityRule = RuleflowExtensions
                .CreateConditionalRule<Product>(p => p.Price > 0)
                .Then(builder => builder
                    .WithPriority(1)
                    .WithAction(p => {
                        executionTracker.Add("LowPriority");
                    })
                )
                .Build();

            // High priority regular rule
            var highPriorityRule = RuleflowExtensions.CreateRule<Product>()
                .WithPriority(10)
                .WithAction(p => {
                    executionTracker.Add("HighPriority");
                })
                .Build();

            // Medium priority switch rule
            var mediumPriorityRule = RuleflowExtensions
                .CreateSwitchRule<Product, bool>(p => p.IsDiscounted)
                .Case(true, builder => builder
                    .WithPriority(5)
                    .WithAction(p => {
                        executionTracker.Add("MediumPriority");
                    })
                )
                .Default(builder => builder
                    .WithAction(p => { /* Nothing */ })
                )
                .Build();

            // The order in the collection is intentionally not in priority order
            var rules = new List<IValidationRule<Product>>
            {
                lowPriorityRule,
                mediumPriorityRule,
                highPriorityRule
            };

            var validator = new DependencyAwareValidator<Product>(rules);

            var product = new Product
            {
                Id = 1,
                Name = "Test",
                Price = 10.0m,
                IsDiscounted = true
            };

            // Act - Clear execution tracker before validation
            executionTracker.Clear();
            validator.CollectValidationResults(product);

            // Assert
            Assert.AreEqual(3, executionTracker.Count);
            Assert.AreEqual("HighPriority", executionTracker[0]);
            Assert.AreEqual("MediumPriority", executionTracker[1]);
            Assert.AreEqual("LowPriority", executionTracker[2]);
        }
    }
}