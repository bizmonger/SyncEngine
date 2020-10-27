namespace SyncEngine

module Language =

    type ErrorDescription = string
    type AsyncResult<'response,'error> = Async<Result<'response,'error>>

    type Endpoint = string

    type Request<'submission> = {
        Endpoint   : Endpoint
        Submission : 'submission
    }

    type IRespond = abstract member RespondTo : Result<'response,'error> -> unit