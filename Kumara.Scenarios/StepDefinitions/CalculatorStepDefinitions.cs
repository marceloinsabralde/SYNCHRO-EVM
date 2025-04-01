// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System;

using Reqnroll;

namespace Kumara.Scenarios.StepDefinitions;

[Binding]
public sealed class CalculatorStepDefinitions(ScenarioContext scenarioContext)
{
    private readonly ScenarioContext _scenarioContext = scenarioContext;

    [Given("the first number is {int}")]
    public void GivenTheFirstNumberIs(int number)
    {
        // Implement arrange (precondition) logic
        _scenarioContext["FirstNumber"] = number;
    }

    [Given("the second number is {int}")]
    public void GivenTheSecondNumberIs(int number)
    {
        // Implement arrange (precondition) logic
        _scenarioContext["SecondNumber"] = number;
    }

    [When("the two numbers are added")]
    public void WhenTheTwoNumbersAreAdded()
    {
        // Implement act (action) logic
        int firstNumber = (int)_scenarioContext["FirstNumber"];
        int secondNumber = (int)_scenarioContext["SecondNumber"];
        _scenarioContext["Result"] = firstNumber + secondNumber;
    }

    [Then("the result should be {int}")]
    public void ThenTheResultShouldBe(int result)
    {
        // Implement assert (verification) logic
        int actualResult = (int)_scenarioContext["Result"];
        if (actualResult != result)
        {
            throw new Exception($"Expected result to be {result}, but was {actualResult}");
        }
    }
}
