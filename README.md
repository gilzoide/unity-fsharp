# F# for Unity
Support F# scripting in Unity: create F# scripts with the `.fs` file extension and they'll be built automatically using the [.NET SDK](https://dotnet.microsoft.com/).


## Features
- Automatically installs the [.NET SDK](https://dotnet.microsoft.com/) locally inside your `Library` folder
- Automatically compiles F# scripts (`.fs` files) to a DLL usable by Unity
  + The DLL and its dependencies are generated at `Assets/FSharpOutput`.
    Consider ignoring this folder in your VCS (e.g.: adding to `.gitignore` file in Git repos).
  + The DLL references the same DLLs as `Assembly-CSharp` does.
- `MonoBehaviour`/`ScriptableObject` class names do not need to have the same name as their source files.
  You can also declare several of them in a single file.


## Creating scripts in F#
```fs
// 1. Add a namespace to your file
namespace MyFSharpNamespace

// 2. Import the UnityEngine and other namespaces as necessary
open UnityEngine

// 3. Create classes that inherit from MonoBehaviour, as usual
type MyFSharpComponent() =
  inherit MonoBehaviour()

  // Use mutable serialized fields to edit them in the Inspector
  [<SerializeField>]
  let mutable serializedFloat = 5f
  
  [<SerializeField>]
  let mutable prefab: GameObject = null

  [<SerializeField>]
  let mutable intArray: array<int> = [||]

  // In F#, lists are immutable by default and not serialized by Unity
  // Use ResizeArray (a.k.a. System.Collections.Generic.List) to serialize lists
  [<SerializeField>]
  let mutable intList = new ResizeArray<int>()

  // Declare member functions (a.k.a. methods)
  member this.Start() =
    // In F#, we mutate fields with `<-` instead of `=`
    serializedFloat <- 10f
    
    let child = Object.Instantiate(prefab, this.transform)
```
