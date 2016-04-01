## Unit testing with F#

### Preface

Probably you've already heard a lot of good things about F# and maybe you've already tried to use it on your small or personal projects. But how should we proceed when it's a matter of something more important than a simple console application or a script. This article narrates about my personal experience of unit testing with F#.

### Source code

For practical matters I've created a small project, with source code available [here](https://github.com/vba/fsharp.tests). It contains a small module and few unit tests for that module. That's actually this module:

```
[<AutoOpen>]
module DistanceUnits

open System

[<Measure>] type m
[<Measure>] type cm
[<Measure>] type inch
[<Measure>] type ft
[<Measure>] type h


let mPerCm : float<m/cm> = 0.01<m/cm>
let cmPerInch : float<cm/inch> = 2.54<cm/inch>
let inchPerFeet: float<inch/ft> = 12.0<inch/ft>

let metersToCentimeters (x: float<m>) = x / mPerCm
let centimetersToInches (x: float<cm>) = x / cmPerInch
let inchesToFeets (x:float<inch>) = x / inchPerFeet

let centimetersToMeters: float<cm> -> float<m> = ( * ) mPerCm
let inchesToCentimeters: float<inch> -> float<cm> = ( * ) cmPerInch
let metersToInches: float<m> -> float<inch> = metersToCentimeters >> centimetersToInches
let metersToFeets: float<m> -> float<ft> =  metersToInches >> inchesToFeets
let feetsToInches: float<ft> -> float<inch> = ( * ) inchPerFeet
let metersToHours(m: float<m>): int<h> = raise(new InvalidOperationException("Unsupported operation"))
```

### Testing library

Theoretically you don't need any special library for test your application in F#. Although, if your are (as me) accustomed to use more standard and approved approach, you'll be happy to use one of bellow described libraries:

- [NUnit](link:http://www.nunit.org/)
- [xUnit](link:https://github.com/xunit/xunit)
- [MSTest](link:https://en.wikipedia.org/wiki/Visual_Studio_Unit_Testing_Framework)

I'll not try to argue with you to convince that my preferred test library is the best in the world, you can use them all in similar manner. Personally, I prefer to work with *xUnit* and I'll use it in this article, all presented code parts could be migrated to *yourTestList* with ease.

Let's start by adding following packages to your project [xunit](https://www.nuget.org/packages/xunit/2.1.0) and [xunit.runner.visualstudio](https://www.nuget.org/packages/xunit.runner.visualstudio/2.1.0)


### Assert libraries

As you know, each test framework is shipped with a minimalistic set of assertion functions. Basically such functionality is enough in 90% of cases, but in term of usage comfort it's far to be ideal. Let's take a look at few additional libraries that are handy.

- [Fluent Assertions](http://www.fluentassertions.com) - Interesting solution for amateurs of chained calls. It's good enough for C#. Unfortunately it's bit awkward to use in F#, mainly because it does not return `Unit`, so you have to cheat by writing somethings similar to `actual.Should().StartWith("S") |> ignore`.
- [FsUnit](https://github.com/dmohl/FsUnit) - This library is made for F#, but tuned for *NUnit*. You can find more samples [here](https://github.com/dmohl/FsUnit#examples). It's also *xUnit* compliant, but this compatibility seems limited and maintenance does not look reactive.
- [Unquote](https://github.com/SwensenSoftware/unquote) - Quite interesting and promising solution, that is based on [Quoted Expressions](https://msdn.microsoft.com/en-us/library/dd233212.aspx). Upon me, there is only one small limitation, it's dependence on F# version 4.0 or above. In this article I'm going to use this library.


### Mock libraries

If you ever meet the necessity of mocking in your unit tests you could use [Moq](https://github.com/Moq/moq4), but if you're looking for more F# friendly solution you'll be able to use [Foq](https://foq.codeplex.com). Let's see side by side how to use those libraries.

Method call with Moq:

```
var mock = new Mock<IFoo>();
mock.Setup(foo => foo.DoIt(1)).Returns(true);
var instance = mock.Object;
```

Method call with Foq:

```
let foo = Mock<IFoo>()
            .Setup(fun foo -> <@ foo.DoIt(1) @>).Returns(true)
            .Create()
```

Arguments comparison with Moq:

```
mock.Setup(foo => foo.DoIt(It.IsAny<int>())).Returns(true);
```

Arguments comparison with Foq:

```
mock.Setup(fun foo -> <@ foo.DoIt(any()) @>).Returns(true)
```

Work with properties with Moq:
```
mock.Setup(foo => foo.Name ).Returns("bar");
```

Work with properties with Foq:
```
mock.Setup(fun foo -> <@ foo.Name @>).Returns("bar")
```

### Other goodies

In some general cases you could use the well-known "arrange phase minimizer" or fake data generator - [AutoFixture](https://github.com/AutoFixture/AutoFixture). As well you can even take advantage of the integration of *AutoFixture* and *xUnit*.

If you are looking more functional approach for your tests, you can use [FsCheck](https://fscheck.github.io/FsCheck/).

### Tests writing

When everything is ready, you can pass to the phase of tests writing. With *xUnit* you can design your tests either trough standard classes or through definition of modules in F#. You're free to choose the approach that suits you best. Below I demonstrate examples of those approaches.

Class:

```
type ConverterTest1() =
    [<Fact>]
    member me.``It should convert meters to centimeters as expected``() =
        
        let actual = 1100.0<cm> |> centimetersToMeters

        test <@ actual = 11.0<m> @>

    [<Fact>]
    member me.``It should convert centimeters to meters as expected``() =
        let actual = 20.0<m> |> metersToCentimeters

        test <@ actual = 2000.00<cm> @>
```

Module:

```
module ConverterTest2 =
    open System
    [<Fact>]
    let ``It should convert meters to feets as expected`` () =
        let actual =  32.0<m> |> metersToFeets

        test <@ actual = 104.98687664041995<ft> @>

    [<Fact>]
    let ``It should fail when rubbish conversion is attempted`` () =
        raises<InvalidOperationException> <@ metersToHours 2.0<m> @
```

### In lieu of conclusion

All above demonstrated tests can be successfully executed in VS or on your integration server. Hope this article was able to clarify few aspects of unit testing with F#. Thanks for reading.

