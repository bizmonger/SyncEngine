module SyncEngine.Tests

open NUnit.Framework
open FsUnit
open SyncEngine.TestAPI
open SyncEngine.TestAPI.Mock

let isError = function | Error _ -> true | Ok _ -> false

[<Test>]
let ``Bootstrap sync engine`` () =

    async {
    
        let! result = [ Mock.start someSyncItem1
                        Mock.start someSyncItem2
                      ] |> Async.Parallel

        result |> Array.exists isError |> should equal false

    } |> Async.RunSynchronously