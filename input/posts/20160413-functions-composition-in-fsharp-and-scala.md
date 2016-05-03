## Functions composition in F# and Scala

_Tuesday, May 3rd 2016_

### Let's speak simply

I have started to think about writing this article few weeks ago, when I tried to explain my 7 years old kid what mathematical functions are. We started by look at very simple aspects. Sounds crazy and maybe pointless, but I finished my introduction by speaking about functions composition. It was so natural to show the meaning of functions by giving the examples from real world and talking about functions composition. The goal of this article is to demonstrate how easy and powerful function composition is. I'll start by pure composition introduction and cool explanation, after that we'll taste a bit of currying and get some fun with monads. Hope you'll enjoy.

### Function as a box

Let's imagine math functions as boxes, where each box can take any positive amount of parameters, then does some job and returns a result. Briefly speaking, we can represent a sum function as one of followed figures:

![](https://habrastorage.org/files/b30/d99/72c/b30d9972c5a34b74bcbeaadf82bb1d0f.png)

_Figure 1, alphanumerical representation of a sum function_

![](https://habrastorage.org/files/355/28d/990/35528d99027a4be19dd4d7d831ab7907.png)

_Figure 2, symbolic representation of a sum function_

Let's consider a situation when we need to assemble and launch an all-in-one bread factory. This factory will work with on demand principles, every demand will activate a chain of specific operations and will end up by supplying a final product - the bread. On initial step we need to define specific operations, we will represent each operation as a function/box. There is a list of higher order operations that we would expect:

- Grind, takes a wheat, grind it and returns a flour
- KneadDough, takes the flour, mix it with internal ingredients and produces a dough
- DistributeDough, takes all quantity of dough, distribute it among the forms and produce a sequence of dough portions
- Bake, takes dough portions and bake it, returns portions of bread

It's time to organize our bread factory by assembling the production chain, as following:
```
 w -> [Grind] -> [KneadDough] -> [DistributeDough] -> [Bake] -> b
```

![]()

_Figure 3, assembled chain representation_

That's all, our chain is operational, it's composed of small parts where each part can be composed of other sub parts, etc. You can model a great amount of things around you just by using the notions of functions composition. It's really simple. You can discover more theoretical aspects [here](https://en.wikipedia.org/wiki/Function_composition).

### Composition expression

Let's have a look how we can represent the production chain with javascript from above example:
```javascript
var b = bake(distribureDough(kneadDough(grind(w))));
```

Just imagine how will look a similar chain of 10 - 15 functions, but it's only one of potential issues. It's not really a composition, because in math, function composition is the pointwise application of one function to the result of another to produce a third function. We can partially reach this in following manner:
```javascript
function myChain1(w) {
    return bake(distribureDough(kneadDough(grind(w))));
}
var b = myChain1(w);
```

It's a bit awkward, isn't? Let's appeal to the power of functional programming and make it in more digestible way. We will operate with more comprehensible example. First we need to define what's a composition in functional way.

###### Scala version

```scala
implicit class Forward[TIn, TIntermediate](f: TIn => TIntermediate) {
    def >> [TOut](g: TIntermediate => TOut): TIn => TOut = source => g(f(source))
}
```

###### F# version

Generally, F# has composition operator by default, you don't need to define anything. But if you need to redefine it, you can achieve it as below:
```F#
let (>>) f g x = g ( f(x) )
```

The compiler of F# is smart enough to surmise that you're dealing with functions, the type of above function `(>>)` will be as followed:
```
f:('a -> 'b) -> g:('b -> 'c) -> x:'a -> 'c
```

##### Chain it all together

The solution for previous task will look in _Scala_ like:
```scala
object BreadFactory {

    case class Wheat()
    case class Flour()
    case class Dough()
    case class Bread()

    def grind: (Wheat => Flour) = w => {println("make the flour"); Flour()}
    def kneadDough: (Flour => Dough) = f => {println("make the dough"); Dough()}
    def distributeDough: (Dough => Seq[Dough]) = d => {println("distribute the dough"); Seq[Dough]()}
    def bake: (Seq[Dough] => Seq[Bread]) = sd => {println("bake the bread"); Seq[Bread]()}

    def main(args: Array[String]): Unit = {
        (grind >> kneadDough >> distributeDough >> bake) (Wheat())
    }

    implicit class Forward[TIn, TIntermediate](f: TIn => TIntermediate) {
        def >> [TOut](g: TIntermediate => TOut): TIn => TOut = source => g(f(source))
    }
}
```

F# version will be more concise:
```F#
type Wheat = {wheat:string}
type Flour = {flour:string}
type Dough = {dough:string}
type Bread = {bread:string}

let grind (w:Wheat) = printfn "make the flour"; {flour = ""}
let kneadDough (f:Flour) = printfn "make the dough"; {dough = ""}
let distributeDough (d:Dough) = printfn "distribute the dough"; seq { yield d}
let bake (sd:seq<Dough>) = printfn "bake the bread"; seq { yield {bread = ""}}

(grind >> kneadDough >> distributeDough >> bake) ({wheat = ""})
```

The output will be:
```
make the flour
make the dough
distribute the dough
bake the bread
```

### Currying

If you're not familiar with the notion of currying, you can get more details [here](https://en.wikipedia.org/wiki/Currying). In this part we will combine two powerful mechanisms that came from functional world - currying and composition. Let's consider the situation when you need to work with functions that have more than one parameter and most of this parameters are known before the execution. For example the `bake` function from previous part can have parameter as temperature or duration of baking that are perfectly known before.

Scala:
```scala
def bake: (Int => Int => Seq[Dough] => Seq[Bread]) =
    temperature => duration => sd => {
        println(s"bake the bread, duration: $duration, temperature: $temperature")
        Seq[Bread]()
    }
```

F#:
```F#
let bake temperature duration (sd:seq<Dough>) = 
    printfn "bake the bread, duration: %d, temperature: %d" temperature duration
    seq { yield {bread = ""}}
```

Currying is our friend, let's define one recipe for baking the bread.

Scala:
```scala
def bakeRecipe1 = bake(350)(45)

def main(args: Array[String]): Unit = {
    (grind >> kneadDough >> distributeDough >> bakeRecipe1) (Wheat())
}
```

F#:
```
let bakeRecipe1: seq<Dough> -> seq<Bread> = bake 350 45
(grind >> kneadDough >> distributeDough >> bakeRecipe1) ({wheat = ""})
```

The output in both case will be:
```
make the flour
make the dough
distribute the dough
bake the bread, duration: 45, temperature: 350
```

### Monadic chaining

Can you imagine the situation when in the middle of the chain something goes wrong? For example, a case when the pipe that supplies yeast or water gets chock and no dough is produced or when the oven gets broken and we obtain a half-baked mass of dough. The pure function composition can be interesting for failure tolerant or unbreakable tasks. But what should we do in above described situation? The answer is trivial, use the monads, hugh. You can find a lot of fundamental information about monads on [wikipedia](https://en.wikipedia.org/wiki/Monad_(functional_programming)) page. Let's see how monads can be helpful in our case, first we need to define (in F#) or use (in Scala) a special type, called `Either`. F# definition can look like a discriminated union below:

```F#
type Either<'a, 'b> = 
    | Left of 'a 
    | Right of 'b
```
Now we are ready to chain, for that purpose we need to create an equivalent of monadic bind operation that should take a monadic value(M) and a function(f) that can transform such value (`f: (x -> M y)`).

F#:
```F#
let chainFunOrFail twoTrackInput switchFunction = 
    match twoTrackInput with
    | Left s -> switchFunction s
    | Right f -> Right f

let (>>=) = chainFunOrFail
```

Scala:
```Scala
implicit class MonadicForward[TLeft, TRight](twoTrackInput: Either[TLeft,TRight]) {
    def >>= [TIntermediate](switchFunction: TLeft => Either[TIntermediate, TRight]) =
        twoTrackInput match {
            case Left (s) => switchFunction(s)
            case Right (f) => Right(f)
        }
}
```
The last thing that we should do is a slight adaption of above described chain to new `Either`-friendly format.

F#:
```F#
let grind (w:Wheat): Either<Flour, string> =
    printfn "make the flour"; Left {flour = ""}
let kneadDough (f:Flour) =
    printfn "make the dough"; Left {dough = ""}
let distributeDough (d:Dough) =
    printfn "distribute the dough"; Left(seq { yield d})
let bake temperature duration (sd:seq<Dough>) =
    printfn "bake the bread, duration: %d, temperature: %d" duration temperature
    Left (seq { yield {bread = ""}})
let bakeRecipe1: seq<Dough> -> Either<seq<Bread>, string> = bake 350 45

({wheat = ""} |> grind) >>= kneadDough >>= distributeDough >>= bakeRecipe1
```

Scala:
```Scala
def grind: (Wheat => Either[Flour, String]) = w => {
    println("make the flour"); Left(Flour())
}
def kneadDough: (Flour => Either[Dough, String]) = f => {
    println("make the dough"); Left(Dough())
}
def distributeDough: (Dough => Either[Seq[Dough], String]) = d => {
    println("distribute the dough"); Left(Seq[Dough]())
}
def bake: (Int => Int => Seq[Dough] => Either[Seq[Bread], String]) =
    temperature => duration => sd => {
        println(s"bake the bread, duration: $duration, temperature: $temperature")
        Left(Seq[Bread]())
    }
def bakeRecipe1 = bake(350)(45)

def main(args: Array[String]): Unit = {
    grind(Wheat()) >>= kneadDough >>= distributeDough >>= bakeRecipe1
}
```

The common output will be as followed below:
```
make the flour
make the dough
distribute the dough
bake the bread, duration: 45, temperature: 350
```
If one of your chain element returns `Right` with appropriated error indicator, the following chain elements will be ignored and execution workflow will just bypass it and all following chain parts and propagate thrown error forward. You can try to experiment failure scenarios by yourself.

### Final part

As you may noticed, there is some magical relation between theory of categories(origin of monads) and composition of functions. The goal of this article is to show how to manipulate such techniques in practice and how to organize your code in more functional way. You can dive in more fundamental aspects of exposed elements by yourself. Hope this article would be useful for those of you who look for either abandon imperative programming and understand functional way of thinking or just discover practical aspects of functional composition and monads.

### Code samples

- All in one F# module is available [here](http://)
- Scala version can be downloaded from [here](http://)
