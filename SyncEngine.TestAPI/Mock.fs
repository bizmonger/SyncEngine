﻿namespace SyncEngine.TestAPI

open System
open SyncEngine.Language
open SyncEngine.Operations

type MockResponse() =

    let mutable responded = false

    member x.Responded with get()  = responded
                       and  set(v) = responded <- v

    interface IRespond with

        member x.RespondTo(_:ContextResponse<'submission,'response>) = x.Responded <- true

module Mock =
    
    let maxMemoryLogItems = 99
    let oneSecond   = TimeSpan(0,0,1)
    let someRequest = { Endpoint= "some_endpoint"; Submission= "some_submission" }

    let somePullOperation1 : Pull<int,string> = fun _ -> async.Return <| Ok "successful response"
    let somePullOperation2 : Pull<string,int> = fun _ -> async.Return <| Ok 200

    let someDataSync1 = {
        Id          = "some_sync_id_1"
        Request     = { Endpoint="some_endpoint"; Submission=123 }
        Execute     = somePullOperation1
        Interval    = oneSecond
        Subscribers = seq []
    }

    let someDataSync2 = {
        Id          = "some_sync_id_2"
        Request     = { Endpoint="some_endpoint"; Submission="123" }
        Execute     = somePullOperation2
        Interval    = oneSecond
        Subscribers = seq []
    }

    let someResponder1 = MockResponse()
    let someResponder2 = MockResponse()