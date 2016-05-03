
type Wheat = {wheat:string}
type Flour = {flour:string}
type Dough = {dough:string}
type Bread = {bread:string}

module SimpleComposition =

    let grind (w:Wheat) =
        printfn "make the flour"
        {flour = ""}

    let kneadDough (f:Flour) =
        printfn "make the dough"
        {dough = ""}

    let distributeDough (d:Dough) =
        printfn "distribute the dough"
        seq { yield d}

    let bake temperature duration (sd:seq<Dough>) =
        printfn "bake the bread, duration: %d, temperature: %d" duration temperature
        seq { yield {bread = ""}}

    let bakeRecipe1: seq<Dough> -> seq<Bread> = bake 350 45

    let run() =
        printfn "Simple composition # run"
        (grind >> kneadDough >> distributeDough >> bakeRecipe1) ({wheat = ""}) |> ignore


module MonadicComposition =
    type Either<'a, 'b> =
        | Left of 'a
        | Right of 'b

    let chainFunOrFail twoTrackInput switchFunction =
        match twoTrackInput with
        | Left s -> switchFunction s
        | Right f -> Right f

    let (>>=) = chainFunOrFail

    let grind (w:Wheat): Either<Flour, string> =
        printfn "make the flour"
        Left {flour = ""}

    let kneadDough (f:Flour) =
        printfn "make the dough"
        Left {dough = ""}

    let distributeDough (d:Dough) =
        printfn "distribute the dough"
        Left(seq { yield d})

    let bake temperature duration (sd:seq<Dough>) =
        printfn "bake the bread, duration: %d, temperature: %d" duration temperature
        Left (seq { yield {bread = ""}})

    let bakeRecipe1: seq<Dough> -> Either<seq<Bread>, string> = bake 350 45

    let run() =
        printfn "Monadic composition # run"
        ({wheat = ""} |> grind) >>= kneadDough >>= distributeDough >>= bakeRecipe1 |> ignore


[<EntryPoint>]
let main _ =
    SimpleComposition.run()
    MonadicComposition.run()
    0
