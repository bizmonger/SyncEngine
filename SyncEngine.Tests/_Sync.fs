module SyncEngine.Tests

open System.Threading
open NUnit.Framework
open FsUnit
open SyncEngine.TestAPI.Mock
open SyncEngine.Language

[<Test>]
let ``Bootstrap sync engine`` () =

    async {
    
        // Setup
        let syncItem = { someDataSync1 with Subscribers = seq {someResponder1} }
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
        let syncItem = { someDataSync1 with Subscribers = seq {someResponder1; someResponder2} }
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
        let syncItem1 = { someDataSync1 with Subscribers = seq {someResponder1} }
        let syncItem2 = { someDataSync2 with Subscribers = seq {someResponder2} }

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
let ``Stopping engine sets state to started`` () =

    async {
    
        // Setup
        let syncItem1 = { someDataSync1 with Subscribers = seq {someResponder1} }
        let syncItem2 = { someDataSync2 with Subscribers = seq {someResponder2} }

        let engines = seq [Engine(seq {syncItem1}) :> IEngine
                           Engine(seq {syncItem2}) :> IEngine] |> MultiEngine

        engines.Start(); 
        
        do! Async.Sleep 1100

        // Test
        do! engines.Stop()

        // Verify
        engines.Log() 
        |> Seq.map(fun (_,v) -> v)
        |> Seq.filter(fun v -> v.Event = "Started")
        |> Seq.length |> should equal 2

    } |> Async.RunSynchronously

[<Test>]
let ``Stopping engine sets state to stopped`` () =

    async {
    
        // Setup
        let syncItem1 = { someDataSync1 with Subscribers = seq {someResponder1} }
        let syncItem2 = { someDataSync2 with Subscribers = seq {someResponder2} }

        let engines = seq [Engine(seq {syncItem1}) :> IEngine
                           Engine(seq {syncItem2}) :> IEngine] |> MultiEngine

        engines.Start(); 
        
        do! Async.Sleep 1100

        // Test
        do! engines.Stop()

        // Verify
        engines.Log() 
        |> Seq.map(fun (_,v) -> v)
        |> Seq.filter(fun v -> v.Event = "Stopped")
        |> Seq.length |> should equal 2

    } |> Async.RunSynchronously

[<Test>]
let ``Starting stoping 2 data sync items results in '4' log items`` () =

    async {
    
        // Setup
        let syncItem1 = { someDataSync1 with Subscribers = seq {someResponder1} }
        let syncItem2 = { someDataSync2 with Subscribers = seq {someResponder2} }

        let engines = seq [Engine(seq {syncItem1}) :> IEngine
                           Engine(seq {syncItem2}) :> IEngine] |> MultiEngine

        engines.Start(); 
        
        do! Async.Sleep 1100

        // Test
        do! engines.Stop()

        // Verify
        engines.Log() |> Seq.length |> should equal 4

    } |> Async.RunSynchronously


[<Test>]
let ``Clear log`` () =

    async {
    
        // Setup
        let syncItem1 = { someDataSync1 with Subscribers = seq {someResponder1} }
        let syncItem2 = { someDataSync2 with Subscribers = seq {someResponder2} }

        let engines = seq [Engine(seq {syncItem1}) :> IEngine
                           Engine(seq {syncItem2}) :> IEngine] |> MultiEngine

        engines.Start(); 
        do! Async.Sleep 1100
        do! engines.Stop()

        // Test
        engines.ClearLog()

        // Verify
        engines.Log() |> Seq.isEmpty |> should equal true

    } |> Async.RunSynchronously