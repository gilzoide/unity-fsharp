namespace UnityEngine

[<AutoOpen>]
module Globals =
  let isNotNull (o: objnull) =
    match o with
    | null -> false
    | :? Object as unityObject -> Object.op_Implicit unityObject
    | _ -> true

  let isNull (o: objnull) = isNotNull o |> not
