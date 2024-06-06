# Implementation of Quine–McCluskey Method
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512bd4?logo=dotnet)](#)

This libaray and runner requires .NET 8.0. To download .NET 8.0, visit [Download Page](https://dotnet.microsoft.com/en-us/download).

## Getting Started

### Run Project
**Windows**  
```bat
run.bat
```

**Linux, macOS**  
```sh
sh ./run.sh
```

### Testing
**Windows**
```bat
test.bat
```

**Linux, macOS**
```
sh ./test.sh
```

### Import the Library

1. Copy [`QuineMcCluskey`](./QuineMcCluskey/) to your project.
2. Run `dotnet add <your-proejct-csproj-file> reference QuineMcCluskey/QuineMccluskey.csproj`. for more details, visit [.NET tutorials about creating a library and adding a project reference](https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio-code).
3. Import library's modules to your project.

* for example usage, read [`Runner/Program.cs`](./Runner/Program.cs).

## Library Modules
[**`QuineMcCluskey`**](./QuineMcCluskey/)  
* `class QuineMcCluskeyWorker`

[**`QuineMcCluskey.Commons`**](./QuineMcCluskey/Commons/)  
* `enum Bit`
* `class DefaultDictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>`

[**`QuineMcCluskey.Exceptions`**](./QuineMcCluskey/Exceptions/)  
* `class TermDiffCountNot1Error : System.Exception`
* `class BitLenVarCountNotMatchError : System.Exception`

[**`QuineMcCluskey.Term`**](./QuineMcCluskey/Term/)  
* `class Term : IComparable`
* `record TermDiff`

## Caution
Since I had to write this code in a short time, I wrote it without considering many things.

In particular, The efficiency and consistency of the algorithm are concerned. Testing has only been done for a very small number of cases, so don't trust it.
