module AsyncResult
  let bind (f : 'a -> Async<Result<'b, 'c>>) (a : Async<Result<'a, 'c>>)  : Async<Result<'b, 'c>> = async {
      match! a with
      | Ok value ->
          let next : Async<Result<'b, 'c>> = f value
          return! next
      | Error err -> return (Error err)
  }

  let compose (f : 'a -> Async<Result<'b, 'e>>) (g : 'b -> Async<Result<'c, 'e>>) : 'a -> Async<Result<'c, 'e>> =
      fun x -> bind g (f x)

  let valueOrDefault f result =
    async {
      match! result with
      | Ok ok -> return ok
      | Error err -> return f err
    }

  let (>>=) a f = bind f a
  let (>=>) f g = compose f g
