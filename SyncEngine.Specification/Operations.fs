namespace SyncEngine

open System
open Language

module Operations =

    type Pull<'submission,'response> = Request<'submission> -> AsyncResult<'response, ErrorDescription>

    type SyncItem<'submission,'response> = {
        Id          : string
        Request     : Request<'submission>
        Execute     : Pull<'submission,'response>
        Interval    : TimeSpan
        Subscribers : IRespond seq
    }

    type Start<'submission,'response> = SyncItem<'submission,'response> -> AsyncResult<unit,ErrorDescription>