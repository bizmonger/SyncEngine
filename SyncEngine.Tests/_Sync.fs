module SyncEngine.Tests

open NUnit.Framework
open FsUnit
open SyncEngine.TestAPI
open SyncEngine.TestAPI.Mock
open System.Threading

let isError = function | Error _ -> true | Ok _ -> false

//[<Test>]
//let ``Bootstrap sync engine`` () =

//    async {
    
//        let! result = [ Mock.start someSyncItem1
//                        Mock.start someSyncItem2
//                      ] |> Async.Parallel

//        result |> Array.exists isError |> should equal false

//    } |> Async.RunSynchronously

[<Test>]
let ``Bootstrap sync engine 2`` () =

    async {
    
        // Setup
        let syncItem = { someSyncItem1 with Subscribers = seq [someResponder1] }
        let engine = Engine(seq [syncItem])

        // Test
        engine.Start()
        Thread.Sleep 1100

        // Verify
        someResponder1.Responded |> should equal true

    } |> Async.RunSynchronously