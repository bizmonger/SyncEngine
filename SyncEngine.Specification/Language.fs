namespace SyncEngine

module Language =

    type ErrorDescription = string
    type AsyncResult<'response,'error> = Async<Result<'response,'error>>

    type Endpoint = string
    type Id       = string

    type SyncState = NotStarted | Started | Stopped

    type Request<'submission> = {
        Endpoint   : Endpoint
        Submission : 'submission
    }

    type ContextResponse<'submission,'response> = {
        Request  : Request<'submission>
        Response : 'response
    }

    type IRespond = abstract member RespondTo : ContextResponse<'submission,'response> -> unit