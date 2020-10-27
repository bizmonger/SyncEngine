namespace SyncEngine

open System.Timers
open Operations

type IEngine = abstract member Start : unit -> unit

type Engine<'submission,'response>(syncItems:SyncItem<'submission,'response> seq) =

    let mutable errors = seq []

    let kvPairs = syncItems |> Seq.map(fun sync -> (sync.Id, new Timer()))
    let map     = Map.ofSeq kvPairs

    let start : Start<'submission,'response> =

        fun v ->

            async {

                try
                    let execute () =

                        async {
                    
                            let! result = v.Execute v.Request
                            v.Subscribers |> Seq.iter (fun s -> s.RespondTo result)
                        }

                    let miliseconds = (float) v.Interval.Seconds * 1000.0
                    let timer = map.[v.Id]
                    timer.Interval  <- miliseconds
                    timer.AutoReset <- true
                    timer.Elapsed.Add (fun _ -> execute() |> Async.RunSynchronously)
                    timer.Start()

                    return Ok ()
                    
                with ex -> return Error <| ex.GetBaseException().Message
        }

    member x.Errors with get()  = errors
                    and  set(v) = errors <- v

    interface IEngine with

        member x.Start() =

            let execute (sync:SyncItem<_,_>) = 
        
                async {
            
                    match! start sync with
                    | Error msg -> x.Errors <- errors |> Seq.append msg
                    | Ok _      -> ()
                }
    
            syncItems |> Seq.iter (fun sync -> sync |> execute |> Async.RunSynchronously)

type MultiEngine(engines:IEngine seq) =

    member x.Start() =

        engines |> Seq.iter(fun engine -> engine.Start())