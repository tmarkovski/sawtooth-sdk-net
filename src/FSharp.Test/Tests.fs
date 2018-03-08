module Tests

open Xunit
open Sawtooth.Sdk.FSharp
open Sawtooth.Sdk.FSharp.Handler
open Sawtooth.Sdk.FSharp.Processor

let emptyApply _ _ = ()

[<Fact>]
let ``Create a handler`` () =

    Handler.create emptyApply
    |> withName "intkey"
    |> withVersion "1.0"
    |> withNamespace "namespace"
    |> Assert.NotNull

[<Fact>]
let ``Create a processor`` () =
    let handler = Handler.create emptyApply

    Processor.create "inproc://test"
    |> addHandler handler
    |> Assert.NotNull  