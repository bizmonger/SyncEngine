namespace SyncEngine

open System
open System.Timers
open Language
open Operations

type IEngine = 

    abstract member TryFind  : Id   -> DataSyncInstance option
    abstract member Start    : unit -> unit
    abstract member Stop     : unit -> unit
    abstract member Stop     : Id   -> unit
    abstract member Log      : unit -> Log
    abstract member ClearLog : unit -> unit

type Engine<'submission,'response>
    (syncItems:DataSyncItem<'submission,'response> seq, maxMemoryLogItems:int) =

    let mutable diagnostics = seq []
    let mutable errors = seq []

    let kvPairs = syncItems |> Seq.map(fun sync -> (sync.Id, (sync, new Timer())))
    let map     = Map.ofSeq kvPairs

    let destroy (v:Id * Timer) =

        let timer = (snd v)
        timer.Stop()
        timer.Dispose()

    let log event item : unit =

        let logItem = { Event=event; Timestamp= DateTime.Now }
        let update  =  seq [item.Id, logItem]

        if   diagnostics |> Seq.length < maxMemoryLogItems then
             diagnostics <- diagnostics |> Seq.append update
        else diagnostics <- diagnostics |> Seq.tail |> Seq.append update

    let start : Start<'submission,'response> =

        fun syncItem ->

            async {

                try
                    let execute () =

                        async {
                    
                            syncItem |> log "Poll Started"

                            let! serverResult = syncItem.Execute syncItem.Request
                            let  result       = { Request= syncItem.Request; Response= serverResult }

                            syncItem |> log "Poll Ended"

                            syncItem.Subscribers |> Seq.iter (fun subscriber -> subscriber.RespondTo result)
                        }

                    let miliseconds = (float) syncItem.Interval.Seconds * 1000.0
                    let _ , timer   = map.[syncItem.Id]
                    timer.Interval  <- miliseconds
                    timer.AutoReset <- true
                    timer.Elapsed.Add (fun _ -> execute() |> Async.Start)
                    timer.Start();
                    
                    syncItem |> log "Started"

                    return Ok ()
                    
                with ex -> return Error <| ex.GetBaseException().Message
        }

    member x.Errors with get()  = errors
                    and  set(v) = errors <- v

    interface IEngine with

        member x.Start() : unit =

            let execute (sync:DataSyncItem<_,_>) = 
        
                async {
            
                    match! start sync with
                    | Error msg -> x.Errors <- errors |> Seq.append msg
                    | Ok _      -> ()
                }
    
            syncItems |> Seq.iter (fun sync -> sync |> execute |> Async.RunSynchronously)

        member x.Stop() =

            let handle kv =

                let id   , value = fst kv    , snd kv
                let item , timer = fst value , snd value

                destroy (id, timer)
                item |> log "Stopped"
            
            kvPairs |> Seq.iter handle

        member x.Stop(id: Id) = 
        
            kvPairs |> Seq.tryFind(fun v -> (fst v) = id)
                    |> function
                       | None   -> ()
                       | Some v -> let id, timer = fst v, snd(snd v)
                                   destroy (id, timer)

        member x.TryFind(id: Id) = 
            
                kvPairs |> Seq.tryFind(fun v -> (fst v) = id)
                        |> function
                           | None   -> None
                           | Some v -> 
                            
                                let syncItem = fst(snd v)
                                Some <| DataSyncInstance(syncItem)

        member x.Log()      = diagnostics
        member x.ClearLog() = diagnostics <- seq []

type MultiEngine(engines:IEngine seq) =

    member x.Start() = engines |> Seq.iter(fun engine -> engine.Start())
    member x.Stop()  = async { engines |> Seq.iter(fun engine -> engine.Stop()) }

    member x.Log() : (Id * LogItem) seq = 
    
        engines |> Seq.map(fun engine -> engine.Log()) 
                |> Seq.concat
                |> Seq.sortByDescending(fun (_,logItem) -> logItem.Timestamp)

    member x.ClearLog() = engines |> Seq.iter(fun engine -> engine.ClearLog()) 

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