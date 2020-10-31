namespace SyncEngine

open System

module Language =

    type ErrorDescription = string
    type AsyncResult<'response,'error> = Async<Result<'response,'error>>

    type Endpoint = string
    type Id       = string

    type Request<'submission> = {
        Endpoint   : Endpoint
        Submission : 'submission
    }

    type ContextResponse<'submission,'response> = {
        Request  : Request<'submission>
        Response : 'response
    }

    type IRespond = abstract member RespondTo : ContextResponse<'submission,'response> -> unit

    type DataSyncItem<'submission,'response> = {
        Id          : Id
        Request     : Request<'submission>
        Execute     : Request<'submission> -> AsyncResult<'response, ErrorDescription> // ALIAS: PULL
        Interval    : TimeSpan
        Subscribers : IRespond seq
    }

    type LogItem = { 
        Event     : string 
        Timestamp : DateTime
    }

    type Log = (Id * LogItem) seq

    type IDataSync = 
        abstract member Start: unit -> unit
        abstract member Stop : unit -> unit

    type DataSyncInstance(dataSyncItem:obj) =

        interface IDataSync with

            member x.Start() = ()
            member x.Stop () = ()