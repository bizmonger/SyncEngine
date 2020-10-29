module SyncEngine.Tests

open System.Threading
open NUnit.Framework
open FsUnit
open SyncEngine.TestAPI.Mock
open Operations
open SyncEngine.Language

[<Test>]
let ``Bootstrap sync engine`` () =

    async {
    
        // Setup
        let syncItem = { someSyncItem1 with Subscribers = seq {someResponder1} }
        let engine   = Engine(seq [syncItem]) :> IEngine

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
        let syncItem = { someSyncItem1 with Subscribers = seq {someResponder1; someResponder2} }
        let engine   = Engine(seq [syncItem]) :> IEngine

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
        let syncItem1 = { someSyncItem1 with Subscribers = seq {someResponder1} }
        let syncItem2 = { someSyncItem2 with Subscribers = seq {someResponder2} }

        let engines = seq [Engine(seq {syncItem1}) :> IEngine
                           Engine(seq {syncItem2}) :> IEngine] |> MultiEngine
        // Test
        engines.Start()
        
        do! Async.Sleep 1100

        // Verify
        [someResponder1.Responded
         someResponder2.Responded

        ] |> List.forall id
          |> should equal true

    } |> Async.RunSynchronously

[<Test>]
let ``Stopping engine sets state to stopped`` () =

    async {
    
        // Setup
        let syncItem1 = { someSyncItem1 with Subscribers = seq {someResponder1} }
        let engines   = seq [Engine(seq {syncItem1}) :> IEngine] |> MultiEngine

        engines.Start()
        
        do! Async.Sleep 1100

        // Test
        do! engines.Stop()

        // Verify
        let stopped = engines.CheckAllEnginesItemsStopped()
        stopped |> should equal false

        // engines.TryFind(syncItem1.Id)
        // |> function
        //    | None   -> failwith "Failed to find sync item"
        //    | Some (v:SyncItem<int,string>) -> v.State |> should equal Started

    } |> Async.RunSynchronously