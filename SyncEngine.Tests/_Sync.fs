module SyncEngine.Tests

open NUnit.Framework
open FsUnit
open SyncEngine.TestAPI
open SyncEngine.TestAPI.Mock
open System.Threading

[<Test>]
let ``Bootstrap sync engine`` () =

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

[<Test>]
let ``Bootstrap sync engine with multiple syncs`` () =

    async {
    
        // Setup
        let syncItem = { someSyncItem1 with Subscribers = seq [someResponder1; someResponder2] }
        let engine   = Engine(seq [syncItem])

        // Test
        engine.Start()
        Thread.Sleep 1100

        // Verify
        [someResponder1.Responded
         someResponder2.Responded

        ] |> List.forall id
          |> should equal true

    } |> Async.RunSynchronously