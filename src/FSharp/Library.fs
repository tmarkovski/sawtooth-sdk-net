namespace Sawtooth.Sdk.FSharp

open System
open Sawtooth.Sdk.Processor
open Sawtooth.Sdk.FSharp.Wrappers

module Handler = 
    let create apply =
        { FamilyName = String.Empty
          Version = String.Empty
          Namespaces = [ ]
          Apply = apply }

    let withName name handler =
        { handler with FamilyName = name }    

    let withVersion version handler =
        { handler with Version = version }

    let withNamespace name handler =
        { handler with Namespaces = List.append handler.Namespaces [ name ] }   

module Processor =
    let create address =
        TransactionProcessor address

    let addHandler handler (processor: TransactionProcessor) =
        processor.AddHandler (HandlerWrapper handler)
        processor

    let start (processor: TransactionProcessor) =
        processor.Start() 
        processor

    let stop (processor: TransactionProcessor) =
        processor.Stop()
