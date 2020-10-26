namespace SyncEngine

open System
open Language

module Operations =

    type Pull<'response> = Request -> unit -> AsyncResult<'response, ErrorDescription>

    type SyncInterval<'response> = {
        Id          : string
        Execute     : Pull<'response>
        Interval    : TimeSpan
        Subscribers : IRespond seq
    }

    type Start<'response> = SyncInterval<'response> -> AsyncResult<unit,ErrorDescription>