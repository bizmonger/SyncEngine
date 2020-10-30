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

    type DataSyncItem<'submission,'response> = {
        Id          : Id
        Request     : Request<'submission>
        Execute     : Request<'submission> -> AsyncResult<'response, ErrorDescription> // aka: PULL
        Interval    : TimeSpan
        Subscribers : IRespond seq
    }

    and SyncState<'submission,'response> = 
        | NotStarted of DataSyncItem<'submission,'response>
        | Started    of DataSyncItem<'submission,'response>
        | Stopped    of DataSyncItem<'submission,'response>

    and IRespond = abstract member RespondTo : ContextResponse<'submission,'response> -> unit

    type Log = string seq