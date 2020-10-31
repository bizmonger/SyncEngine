namespace SyncEngine

open System.Timers
open Language
open Operations

type IEngine = 

    abstract member TryFind : Id   -> DataSyncInstance option
    abstract member Start   : unit -> unit
    abstract member Stop    : unit -> unit
    abstract member Stop    : Id   -> unit

type Engine<'submission,'response>
    (syncItems:DataSyncItem<'submission,'response> seq, diagnostics:Diagnostics) =

    let mutable errors = seq []

    let kvPairs = syncItems |> Seq.map(fun sync -> (sync.Id, (sync, new Timer())))
    let map     = Map.ofSeq kvPairs

    let destroy (v:Id * Timer) =

        let timer = (snd v)
        timer.Stop()
        timer.Dispose()

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
                    let _ , timer = map.[v.Id]
                    timer.Interval  <- miliseconds
                    timer.AutoReset <- true
                    timer.Elapsed.Add (fun _ -> execute() |> Async.Start)
                    timer.Start()

                    return Ok ()
                    
                with ex -> return Error <| ex.GetBaseException().Message
        }

    member x.Errors with get()  = errors
                    and  set(v) = errors <- v

    interface IEngine with

        member x.TryFind(id: Id) = 
        
            kvPairs |> Seq.tryFind(fun v -> (fst v) = id)
                    |> function
                       | None   -> None
                       | Some v -> 
                        
                            let syncItem = fst(snd v)
                            Some <| DataSyncInstance(syncItem)

        member x.Start() : unit =

            let execute (sync:DataSyncItem<_,_>) = 
        
                async {
            
                    match! start sync with
                    | Error msg -> x.Errors <- errors |> Seq.append msg
                    | Ok _      -> ()
                }
    
            syncItems |> Seq.iter (fun sync -> sync |> execute |> Async.RunSynchronously)

        member x.Stop() =

            let handle v =

                let id, timer = fst v, snd(snd v)
                destroy (id, timer)
            
            kvPairs |> Seq.iter handle

        member x.Stop(id: Id) = 
        
            kvPairs |> Seq.tryFind(fun v -> (fst v) = id)
                    |> function
                       | None   -> ()
                       | Some v -> let id, timer = fst v, snd(snd v)
                                   destroy (id, timer)


type MultiEngine(engines:IEngine seq) =

    member x.Start() = engines |> Seq.iter(fun engine -> engine.Start())
    member x.Stop()  = async { engines |> Seq.iter(fun engine -> engine.Stop()) }

    member x.TryFind(id:Id) =

        let find (v:IEngine) =

            id |> v.TryFind |> function
            | None      -> None
            | Some item -> Some item

        engines 
        |> Seq.map find
        |> Seq.toList
        |> function
           | []   -> None
           | h::_ -> Some h