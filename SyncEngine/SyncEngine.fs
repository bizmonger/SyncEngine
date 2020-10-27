namespace SyncEngine

open System.Timers
open Language
open Operations

type Engine<'submission,'response>(syncs:SyncItem<'submission,'response> seq) =

    let mutable timer : Timer = null
    let mutable errors = seq []

    let start : Start<'submission,'response> =

        fun v _ ->
        
            async {

                let execute () =

                    async {
                    
                        let! result = v.Execute v.Request
                        v.Subscribers |> Seq.iter (fun s -> s.RespondTo result)
                    }

                let miliseconds = (float) v.Interval.Seconds * 1000.0
                timer <- new Timer(miliseconds)
                timer.AutoReset <- true
                timer.Elapsed.Add (fun _ -> execute() |> Async.RunSynchronously)
                timer.Start()

                return Ok ()    
        }

    member x.Errors with get()  = errors
                    and  set(v) = errors <- v

    member x.Start() =

        let execute (sync:SyncItem<_,_>) = 
        
            async {
            
                match! start sync () with
                | Error msg -> x.Errors <- errors |> Seq.append msg
                | Ok _      -> ()
            }
    
        syncs |> Seq.iter (fun sync -> sync |> execute |> Async.RunSynchronously)