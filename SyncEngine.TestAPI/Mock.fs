namespace SyncEngine.TestAPI

open System
open SyncEngine.Operations
open SyncEngine.Language

module Mock =

    let oneSecond   = TimeSpan(0,0,1)
    let someRequest = { Endpoint= "some_endpoint"; Submission= "some_submission" }

    let somePullOperation1 : Pull<string> = fun request _ -> async.Return <| Error "not implemented"
    let somePullOperation2 : Pull<int>    = fun request _ -> async.Return <| Error "not implemented"

    let someSyncItem1 = {
        Id          = "some_sync_id_1"
        Execute     = somePullOperation1
        Interval    = oneSecond
        Subscribers = seq []
    }

    let someSyncItem2 = {
        Id          = "some_sync_id_2"
        Execute     = somePullOperation2
        Interval    = oneSecond
        Subscribers = seq []
    }

    let start<'response> : Start<'response> =

        fun _ -> async.Return <| Error "not implemented"