namespace SyncEngine.TestAPI

open System
open SyncEngine.Operations
open SyncEngine.Language
open System.Timers

module Mock =

    let oneSecond   = TimeSpan(0,0,1)
    let someRequest = { Endpoint= "some_endpoint"; Submission= "some_submission" }

    let somePullOperation1 : Pull<int,string> = fun request -> async.Return <| Error "not implemented"
    let somePullOperation2 : Pull<string,int> = fun request -> async.Return <| Error "not implemented"

    let someSyncItem1 = {
        Id          = "some_sync_id_1"
        Request     = { Endpoint="some_endpoint"; Submission=123 }
        Execute     = somePullOperation1
        Interval    = oneSecond
        Subscribers = seq []
    }

    let someSyncItem2 = {
        Id          = "some_sync_id_2"
        Request     = { Endpoint="some_endpoint"; Submission="123" }
        Execute     = somePullOperation2
        Interval    = oneSecond
        Subscribers = seq []
    }

    let start<'submission,'response> : Start<'submission,'response> =

        fun v ->
        
            async {

                let execute () =

                    async {
                    
                        let! result = v.Execute v.Request
                        v.Subscribers |> Seq.iter (fun s -> s.RespondTo result)
                    }

                let miliseconds = (float) v.Interval.Seconds * 1000.0
                let timer = new Timer(miliseconds)
                timer.AutoReset <- true
                timer.Elapsed.Add (fun _ -> execute() |> Async.RunSynchronously)
                timer.Start()

                return Ok ()    
        }