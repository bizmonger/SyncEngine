namespace SyncEngine

open System.Timers
open Language
open Operations

type IEngine = 

    abstract member Start : unit -> unit
    abstract member Stop  : unit -> unit
    abstract member Stop  : Id   -> unit

type Engine<'submission,'response>(syncItems:SyncItem<'submission,'response> seq) =

    let mutable errors = seq []
    let mutable state  = ""

    let kvPairs = syncItems |> Seq.map(fun sync -> (sync.Id, new Timer()))
    let map     = Map.ofSeq kvPairs

    let start : Start<'submission,'response> =

        fun v ->

            async {

                try
                    let execute () =

                        async {
                    
                            let! serverResult = v.Execute v.Request
                            let  result = { Request= v.Request; Response= serverResult }

                            v.Subscribers |> Seq.iter (fun subscriber -> subscriber.RespondTo result)
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

    member x.State with get()  = state
                   and  set(v) = state <- v

    interface IEngine with

        member x.Start() : unit =

            let execute (sync:SyncItem<_,_>) = 
        
                async {
            
                    match! start sync with
                    | Error msg -> x.Errors <- errors |> Seq.append msg
                    | Ok _      -> ()
                }
    
            syncItems |> Seq.iter (fun sync -> sync |> execute |> Async.RunSynchronously)

        member x.Stop()       : unit = kvPairs |> Seq.iter(fun v -> (snd v).Stop())
        member x.Stop(id: Id) : unit = 
        
            kvPairs |> Seq.tryFind(fun v -> (fst v) = id)
                    |> function
                       | None   -> ()
                       | Some v -> (snd v).Stop()


type MultiEngine(engines:IEngine seq) =

    member x.Start() = engines |> Seq.iter(fun engine -> engine.Start())
    member x.Stop()  = async { engines |> Seq.iter(fun engine -> engine.Stop()) }

    member x.TryFind(id:Id) =

        None
        //engines |> Seq.tryFind (fun v -> v.SyncItems |> Seq.tryFind())