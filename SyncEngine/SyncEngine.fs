namespace SyncEngine

open System
open System.Threading
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
    let mutable errors      = seq []

    let kvPairs = syncItems |> Seq.map(fun sync -> (sync.Id, (sync, new CancellationTokenSource())))
    let map     = Map.ofSeq kvPairs

    let log event item : unit =

        let logItem = { Event=event; Timestamp= DateTime.Now }
        let update  = seq [item.Id, logItem]

        if   diagnostics |> Seq.length < maxMemoryLogItems then
             diagnostics <- diagnostics |> Seq.append update
        else diagnostics <- diagnostics |> Seq.tail |> Seq.append update

    let start : Start<'submission,'response> =

        fun syncItem ->

            let execute () =

                async { 

                    syncItem |> log "Poll Started"

                    let! serverResult = syncItem.Execute syncItem.Request
                    let  result = { Request= syncItem.Request; Response= serverResult }

                    syncItem |> log "Poll Ended"
                    syncItem.Subscribers |> Seq.iter (fun v -> v.RespondTo result)
                }

            let rec loop () = 

                async {

                    let miliseconds = syncItem.Interval.Seconds * 1000
                    do! execute()
                    do! Async.Sleep(miliseconds)
                    return! loop () 
                }
                    
            async {

                try
                    do! loop  ()
                    return Ok ()
                    
                with ex -> return Error <| ex.GetBaseException().Message
            }

    member x.Errors with get()  = errors
                    and  set(v) = errors <- v

    interface IEngine with

        member x.Start() : unit =

            let execute (sync:DataSyncItem<_,_>) = 

                sync |> log "Started"

                async {

                    match! start sync with
                    | Error msg -> x.Errors <- errors |> Seq.append msg
                    | Ok _      -> ()
                }

            let handle sync = 

                let _, cancel = map.[sync.Id]
                Async.Start(execute sync, cancel.Token)
    
            syncItems |> Seq.iter handle

        member x.Stop() =

            let handle (item, (cancellation:CancellationTokenSource)) =

                cancellation.Cancel()
                cancellation.Dispose()

                item |> log "Stopped"
            
            kvPairs |> Seq.iter(fun (_,v) -> handle v)

        member x.Stop(id: Id) = 
        
            kvPairs |> Seq.tryFind(fun v -> (fst v) = id)
                    |> function
                       | None -> ()
                       | Some (_,(_,c)) -> c.Cancel()

        member x.TryFind(id: Id) = 
            
                kvPairs |> Seq.tryFind(fun v -> (fst v) = id)
                        |> function
                           | None           -> None
                           | Some (_,(v,_)) -> Some <| DataSyncInstance(v)

        member x.Log()      = diagnostics
        member x.ClearLog() = diagnostics <- seq []

type MultiEngine(engines:IEngine seq) =

    member x.Start() = engines |> Seq.iter(fun v -> v.Start())
    member x.Stop()  = async { engines |> Seq.iter(fun v -> v.Stop()) }

    member x.Log() : (Id * LogItem) seq = 
    
        engines |> Seq.map(fun v -> v.Log()) 
                |> Seq.concat
                |> Seq.sortByDescending(fun (_,v) -> v.Timestamp)

    member x.ClearLog() = engines |> Seq.iter(fun engine -> engine.ClearLog()) 

    member x.TryFind(id:Id) =

        engines 
        |> Seq.map (fun v -> id |> v.TryFind)
        |> Seq.toList
        |> function
           | []   -> None
           | h::_ -> Some h