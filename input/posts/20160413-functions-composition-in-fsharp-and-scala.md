## Functions composition in F# and Scala

### Let's speak simply

I have started think about writing this article a week ago, when I tried to explain my 7 years old kid what mathematical function are. Sounds crazy and maybe pointless, but I finished my introduction by speaking about functions composition. It was so natural to show the meaning of functions by giving the examples from real world and talking about functions composition. The goal of this article to demonstrate how easy and powerful is function composition. I'll start by pure composition introduction and cool explanation, after that we'll taste a bit of currying and get some fun with monads. Hope you'll enjoy.

### Function as a box

Let's imagine math functions as boxes, where each box can take any positive amount of parameters, then it does some job and finally returns a result. Briefly speaking, we can represent a sum function as one of followed figures:

![](https://habrastorage.org/files/b30/d99/72c/b30d9972c5a34b74bcbeaadf82bb1d0f.png)

_Figure 1, alphanumerical representation of a sum function_

![](https://habrastorage.org/files/355/28d/990/35528d99027a4be19dd4d7d831ab7907.png)

_Figure 2, symbolic representation of a sum function_

Let's consider a situation when we need to assemble and launch an all-in-one bread factory. This factory will work with on demand principles, every demand will activate a chain of specific operations and will end up by suppling the final product - bread. On initial step we need to define specific operations, we will represent each operation as a function/box. There is a list of higher order operations that we would expect:

- Grind, takes a wheat, grind it and returns a flour
- KneadDough, takes the flour, mix it with internal ingredients and produces a dough
- DistributeDough, takes all quantity of dough, distribute it among the forms and produce a sequence of dough portions
- Bake, takes dough portions and bake it, returns portions of bread

It's time to organize our bread factory by assembling the production chain, as following:
```
 w -> [Grind] -> [KneadDough] -> [DistributeDough] -> [Bake] -> b
```

That's all, our chain is operational, it's composed of small parts where each part can be composed of other sub parts, etc. You can model a great amount of things around you just by using the notions of functions composition. It's really simple. You can discover more theoretical aspects [here](https://en.wikipedia.org/wiki/Function_composition).

### Composition expression

Let's have a look how we can represent the production chain with javascript from above example:
```javascript
var b = bake(distribureDough(kneadDough(grind(w))));
```

Just imagine how will look a similar chain of 10 - 15 functions, but it's one of issues. It's not really a composition, because in math, function composition is the pointwise application of one function to the result of another to produce a third function. We can partially reach this in following manner:
```javascript
function myChain1(w) {
    return bake(distribureDough(kneadDough(grind(w))));
}
var b = myChain1(w);
```

It's bit awkward, isn't? Let's appeal to the power of functional programming and make it in more digestible way. We will operate with more comprehensible example. First we need to define what's a composition in functional way.

###### Scala version

```scala
implicit class Forward[TIn, TIntermediate](f: TIn => TIntermediate) {
    def >> [TOut](g: TIntermediate => TOut): TIn => TOut = source => g(f(source))
}
```

###### F# version

By default F# has composition operator by default, you don't need to define anything.
```F#
let (>>) f g x = g ( f(x) )
```

The compiler of F# is smart enough to surmise that you're dealing with functions, the type of above function `(>>)` will be as following:
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

If you're not familiar with the notion of currying, you can get more details [here](https://en.wikipedia.org/wiki/Currying). In this part we will combine two powerful mechanisms that came from functional world - currying and composition. Let's consider the situation when you need to work with functions that have more than one parameter and most of this parameters are known before the execution. For example the `bake` function from previous part can have parameter as temperature or duration of baking.

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

Can you imagine the situation when in the middle of the chain something go wrong? For example a case when the pipe that supplies yeast or water gets chock and no dough is produced or when the oven gets broken and we obtain a half-baked mass of dough. The pure function composition can be interesting for failure tolerant or unbreakable tasks. But what should we do in above described situation? The answer is trivial, use the monads. You can find a lot of fundamental information 