## Some algorithmic problem and F# .

_Suterday, November 26th 2016_

### Initial problem

The original description can be found [here](http://bit.ly/2fNMLiO). I found this problem interesting and unusual for such kind of problems.

### Procedural solution

Let's see how such problem can be solved with in procedural way, with `C#` :

```C#
public static void Main (string[] args)
{
    var n = int.Parse(Console.ReadLine());
    var line = Console.ReadLine();

    if (n == 1) { Console.WriteLine(0); return; }

    int result = 0;
    for (int i = 0; i < 25; i++) {
        for (int j = 0; j < 25; j++) {
            if (i == j) continue;

            var char1 = (char)('a' + i);
            var char2 = (char)('a' + j);

            var aggregated = line.Where (x => x == char1 || x == char2).Aggregate(string.Empty, (agg, x) => {
                if ("#" == agg) return "#";
                if (agg.Length == 0) return x.ToString();
                if (agg[agg.Length - 1] == x) return "#";
                return agg + x;
            });

            if (aggregated != "#" && result < aggregated.Length) {
                result = aggregated.Length;
            }
        }
    }
    Console.WriteLine(result);
}
```

It's not very memory efficient solution, but for some string manipulation task it's not too bad. Time complexity of my solution is `O(n)`.

### Some thoughts about usefulness of usage of functional languages for solving such problems.

As we already know, one of foundation of functional programming is immutability. No matter if we deal with pure or impure functional language, we should keep in mind the fact that there is a price to pay for being immutable. Probably because of that fact `lisp` remained a kind of language for "marginals" during very long period. I'll not argue with the fact that for efficient usage of immutability-friendly language we need very advanced garbage collection mechanism and in far 1960th the humanity had not in possession such technology. 

Formally `F#` is much more closer to mathematical notation than any procedural language, so it is more appropriate for solving problems from such subdomains as number theory, algebra or dynamic programming. Is a functional language a right tool for such subdomains as algorithms and data structures that seem to be adapted for `C` or `Pascal` and are actually hostile for immutability?

### F# based solution with nested loops

There is no magic about adapt above presented code sample to `F#`. We just need to split some parts into smaller functional pieces.

```C#
let a = 'a'
let limit = 25
let ($) = "$"

let fold agg x =
    match agg with
    | s when s = ($) -> ($)
    | s when s.Length = 0 -> x.ToString()
    | s when s.[s.Length - 1] = x -> ($)
    | s -> s + x.ToString()

let char = (a |> (int32 >> (+)) >> char)

let (||~) char1 char2 x = x = char1 || x = char2

let doIt (line:string) prev =
    [
        for i in 0 .. limit do
        for j in 0 .. limit do
            if(i = j) then yield 0
            else
                let filter = (char i) ||~ (char j)
                let aggregate = (filter |> Seq.filter) >> (fold |> Seq.fold) ""

                match line.ToCharArray() |> aggregate with
                | "$" -> yield 0
                | s -> yield s.Length
    ] |> Seq.max

let n = Console.ReadLine()
let line = Console.ReadLine()

if (n = 1) then printfn "%d" 0

let res = doIt line n
printfn "%d" res
```

### F# based solution with nested loops

What functional code could be written without tail recursion can exclaim you, and you'll be right. Seriously speaking, nested loops seem awkward here, let's stop such profanation and eliminate these loops with recursion. We just need to remake the `doIt` method.

```C#
    let rec outer(i) =
        let rec nested(j) =
            if (i = j || j > limit) then [0]
            else
                let filter = (char i) ||~ (char j)
                let aggregate = (filter |> Seq.filter) >> (fold |> Seq.fold) ""

                (match line.ToCharArray() |> aggregate with
                 | "$" -> 0
                 | s -> s.Length) :: nested(j + 1)

        if i > limit then []
        else nested(i + 1) @ outer(i + 1)
    outer(0) |> Seq.max
```

Looks a bit better, isn't?

### Conclusion

I remain a bit suspicious about expediency of `F#` or another `Functional Language ` of your choice for solving some algorithmic problems, invented by the people and for the people who is used to work and think in procedural way. Maybe I'm just wrong and I was not able to write more digestible F# code like `(step1 >> step2 +++ step3 >> step4) ~== happiness `. Thanks for reading.