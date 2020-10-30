namespace SyncEngine.Diagnostics

open SyncEngine.Language

module Operations =
    
    type Log<'submission,'response> = SyncState<'submission,'response> -> AsyncResult<unit       , ErrorDescription>
    type DataSyncItemLog            = Id                               -> AsyncResult<Log option , ErrorDescription>
    type Report                     = unit                             -> AsyncResult<Log seq    , ErrorDescription>
    type Flush                      = unit                             -> AsyncResult<unit       , ErrorDescription>