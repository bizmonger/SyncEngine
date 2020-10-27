module SyncEngine.Tests

open System.Threading
open NUnit.Framework
open FsUnit
open SyncEngine.TestAPI.Mock

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
let ``Engine with multiple syncs`` () =

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


[<Test>]
let ``Engine only syncs registered syncitem subscribers`` () =

    async {
    
        // Setup
        let syncItem1 = { someSyncItem1 with Subscribers = seq [someResponder1] }
        let syncItem2 = { someSyncItem2 with Subscribers = seq [someResponder2] }
        let engine    = Engine(seq [syncItem1; (*syncItem2*)])

        // Test
        engine.Start()
        Thread.Sleep 1100

        // Verify
        [someResponder1.Responded
         someResponder2.Responded

        ] |> List.forall id
          |> should equal true

    } |> Async.RunSynchronously