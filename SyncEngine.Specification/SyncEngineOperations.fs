namespace SyncEngine

open Language

module Operations =

    type Pull<'submission,'response>  = Request<'submission>                -> AsyncResult<'response, ErrorDescription>
    type Start<'submission,'response> = DataSyncItem<'submission,'response> -> AsyncResult<unit     , ErrorDescription>