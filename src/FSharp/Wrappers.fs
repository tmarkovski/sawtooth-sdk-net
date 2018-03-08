module Sawtooth.Sdk.FSharp.Wrappers

open Sawtooth.Sdk.Processor
open System.Threading.Tasks

type Context (context: TransactionContext) =
    member __.GetState addresses =
        context.GetStateAsync addresses

type TransactionHandler = {
    FamilyName: string
    Version: string
    Namespaces: string list
    Apply: Request -> Context -> unit }

and Request = { Payload: byte array; SourceRequest: TpProcessRequest }

type HandlerWrapper (handler: TransactionHandler) =

    interface ITransactionHandler with

        member x.FamilyName = handler.FamilyName

        member x.Version = handler.Version

        member x.Namespaces = List.toArray handler.Namespaces

        member x.ApplyAsync (request: TpProcessRequest, context) =
            handler.Apply { SourceRequest = request; Payload = request.Payload.ToByteArray() } (Context context)
            Task.CompletedTask