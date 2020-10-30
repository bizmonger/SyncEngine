namespace SyncEngine

open SyncEngine.Diagnostics.Operations

module Diagnostics =

    let log             : Log<'submission,'response> = fun _ -> async.Return <| Error "not implemented"
    let dataSyncItemLog : DataSyncItemLog            = fun _ -> async.Return <| Error "not implemented"
    let report          : Report                     = fun _ -> async.Return <| Error "not implemented"
    let flush           : Flush                      = fun _ -> async.Return <| Error "not implemented"