# Calculis
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
![Nuget](https://img.shields.io/nuget/v/calculis)
[![.NET](https://github.com/ErgoSm/Calculis/actions/workflows/dotnet-test.yml/badge.svg?branch=master)](https://github.com/ErgoSm/Calculis/actions/workflows/dotnet-test.yml)
![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/ergosm/931c5637fc3e70a50785addf23bcf34a/raw/code-coverage.json)
## What is it?
Calculis is a micro-framework that provides the user with the ability to organize calculations according to mathematical expressions, including the time aspect of values. Provides an easy-to-use functionality for extending with your own functions.
## Content
- [Getting started](https://github.com/ErgoSm/Calculis#getting-started)
- [Expressions](https://github.com/ErgoSm/Calculis#expressions)
- [Extension](https://github.com/ErgoSm/Calculis#extensibility)

## Getting started
First, install Calculis from the package manager console:
```
PM> Install-Package Calculis -Version 0.1.8
```
Calculus works with objects implementing the IValueItem interface. Implement the interface in your program:
```csharp
class DataItem : IValueItem
{
    public string Name { get; set; }
    public double Value { get; set; }
    public DataItem(string name, double value)
    {
        Name = name;
        Value = value;
    }
}
```
Now you can create a collection of elements involved in calculations and pass them to an instance of the calculator:
```csharp
var items = new List<IValueItem>();

items.Add(new DataItem("i1", 3));
items.Add(new DataItem("i2", 2));

var calculis = new CalculisEngine(items);
```
After that, you can add elements calculated based on previously added ones:
```csharp
var calc1 = calculis.Add("calc1", "i1 + i2 * 5");
var calc2 = calculis.Add("calc2", "POW(calc1;2)");

Console.WriteLine($"calc1: {calc1.Value}"); //'calc1: 28'
Console.WriteLine($"calc2: {calc2.Value}"); //'calc2: 169'
```
## Expressions
The calculated element may contain the following expressions:
- Arithmetic expressions
```csharp
calculis.Add("Arithmetic_example1", "2 - (i1 + i2) / 5");
calculis.Add("Arithmetic_example2", "i1*i2/-2");
calculis.Add("Arithmetic_example2", "i1*2.5", CultureInfo("en-US"));
```
- Logical expressions
```csharp
calculis.Add("Logical_example1", "i1 > i2");
calculis.Add("Logical_example2", "i1 < 5 <> i2"); //the result is a logical intersection of the conditions
```
- Functions
```csharp
calculis.Add("Formulary_example1", "AVG(i1;i2;-5)");
calculis.Add("Formulary_example2", "IF(i1 > i2;-1;1)"); //returns -1
```
All types of expressions can be combined within a common expression. The order of calculations follows mathematical rules and is intuitive. More complete information can be found in the sections devoted to the corresponding types of expressions. You can get more information either about the [expressions](https://github.com/ErgoSm/Calculis/wiki/expressions) as well as [standard](https://github.com/ErgoSm/Calculis/wiki/standard-functions) and [temporal](https://github.com/ErgoSm/Calculis/wiki/temporal-functions) functions.
## Extensibility
The user can extend the existing library of functions by plugging their own libraries. To create a function, you need to create a new library project.dll and inherit the FunctionBase class and override the function Function. For example, let's create a function that calculates the remainder of the division:
```csharp
[ArgumentsNumber(2)]
public class RemFunction : FunctionBase
{
    public RemFunction(ICollection<IValueItem> args) : base(args)
    {
    }

    public override Func<double> Function { get => () => (int)_args[0].Value % (int)_args[1].Value; }
}
```
After that, you need to register the assembly containing the function in the instance of the calculator and you can use it in expressions:
```csharp
calculis.Register("AuxFunctions.dll");
var rest = calculis.Add("rest", "REM(calc1;3)");

Console.WriteLine($"reminder of {calc1.Value} / 3: {rest.Value}"); //'reminder of 28 / 3: 1'
```
