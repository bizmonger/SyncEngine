namespace SyncEngine

module Language =

    type ErrorDescription = string
    type AsyncResult<'respose,'error> = Async<Result<'respose,'error>>

    type Endpoint = string

    type Request = {
        Endpoint   : Endpoint
        Submission : obj
    }

    type IRespond =

        abstract member Respond : AsyncResult<'respose,'error> -> unit