module Environment

let getOrElse key elseVal =
    match System.Environment.GetEnvironmentVariable(key) with
    | null -> elseVal
    | res -> res
