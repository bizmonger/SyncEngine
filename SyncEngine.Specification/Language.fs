namespace SyncEngine

module Language =

    type ErrorDescription = string
    type AsyncResult<'respose,'error> = Async<Result<'respose,'error>>

    type Endpoint = string

    type Request<'submission> = {
        Endpoint   : Endpoint
        Submission : 'submission
    }

    type IRespond = abstract member RespondTo : Result<'respose,'error> -> unit