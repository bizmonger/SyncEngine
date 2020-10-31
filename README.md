# Automate periodic client/server synchronization
Single data sync item

    [<Test>]
    let ``Bootstrap sync engine`` () =

        async {
    
            // Setup
            let syncItem = { someDataSync1 with Subscribers = seq {someResponder1} }
            let engine   = Engine(seq [syncItem], maxMemoryLogItems) :> IEngine

            // Test
            engine.Start()
            Thread.Sleep 1100

            // Verify
            someResponder1.Responded |> should equal true

        } |> Async.RunSynchronously
        
Multiple data sync items

    [<Test>]
    let ``Engine with multiple syncs`` () =

        async {
    
            // Setup
            let syncItem = { someDataSync1 with Subscribers = seq {someResponder1; someResponder2} }
            let engine   = Engine(seq [syncItem], maxMemoryLogItems) :> IEngine

            // Test
            engine.Start()
            Thread.Sleep 1100

            // Verify
            [someResponder1.Responded
             someResponder2.Responded

            ] |> List.forall id
              |> should equal true

        } |> Async.RunSynchronously
        
View tests for additional examples.
