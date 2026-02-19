# HadFun- Project Documentation

## 1. Overview

**HadFun-** is a C# .NET experimentation and learning sandbox project. It serves as a personal playground for exploring a wide range of C# language features, .NET APIs, concurrency patterns, design patterns, and utility implementations. The project is structured as a single console application targeting **.NET 10.0** with a dependency on **Newtonsoft.Json 13.0.2**.

Almost all code in [Program.cs](Program.cs) is **commented out** and organized into `#region` blocks, each demonstrating a different concept or experiment. The currently active code simply reads and prints the contents of [README.md](README.md) (which contains ASCII art).

The project contains three C# source files and several supporting data files.

---

## 2. Key Components and Their Interactions

### 2.1 Program.cs — The Experiment Catalog

[Program.cs](Program.cs) (1075 lines) is the monolithic entry point. It contains **~20 commented-out experiment regions**, each self-contained. Only the final region (`Random test`) is active. The experiments cover:

| Region | Lines (approx.) | Topic |
|---|---|---|
| **Proxy server** | 30–46 | Bootstraps `ProxyServer` to proxy Google through localhost |
| **Jason Bourne** | 48–72 | JSON manipulation with `JObject`, `JArray`, JSONPath, LINQ |
| **Beauty of C# 9.0** | 74–90 | `record` types, `with` expressions, value equality |
| **Json Validation** | 92–116 | JSON Schema validation with `Newtonsoft.Json.Schema` |
| **Encryption** | 118–224 | AES encrypt/decrypt round-trip using `System.Security.Cryptography` |
| **Records in C# 9.0** | 226–248 | `record` vs `class` with `IPerson` interface |
| **Hack into private variables** | 250–264 | Reflection to access private fields (`BindingFlags.NonPublic`) |
| **String is reference type but immutable** | 266–290 | Demonstrates string immutability vs reference-type mutability |
| **Game of Parallelism + Concurrency** | 292–310 | Sequential vs `AsParallel()` sum with `Stopwatch` benchmarking |
| **Game of Concurrency with CancellationToken** | 312–376 | Async HTTP calls with `CancellationTokenSource`, UI simulation |
| **Sleep sort** | 378–390 | Novelty sorting algorithm using `Thread.Sleep` |
| **Design Patterns — Memento** | 394–458 | Originator/Memento/Caretaker pattern |
| **Design Patterns — Builder** | 462–600 | Director/Builder/ConcreteBuilder/Product pattern |
| **Events & delegates** | 602–648 | Custom event/delegate pattern with `VideoDownloader` |
| **GitHub Copilot fun** | 650–736 | AI-generated code: `Person` class, prime check, reflection |
| **Check total free space of fixed drive** | 738–782 | `ConcurrentDictionary`, `Parallel.For`, `DriveInfo`, file I/O |
| **LINQ performance** | 784–830 | `FirstOrDefault` vs `Where().FirstOrDefault()`, `Count` vs `Where().Count()` |
| **Get memory address of an object** | 832–840 | Unsafe code, `__makeref`, pointer dereferencing |
| **ConcurrentQueue.ClearByItem** | 842–882 | Extension method to remove specific items from `ConcurrentQueue<T>` |
| **Regex vs MailAddress.TryCreate** | 884–896 | Email validation performance comparison |
| **Channel** | 898–916 | `Channel.CreateBounded<int>` producer/consumer |
| **Pattern matching on ITuple** | 918–940 | Custom `ITuple` implementation, positional pattern matching |
| **Double parsing** | 942–962 | Floating-point precision exploration |
| **YouTube blocklist** | 964–998 | Downloads ad-blocking filter lists and writes a combined blocklist |
| **for vs foreach vs LINQ performance** | 1000–1060 | Loop performance benchmarks including `Parallel.For`, `Parallel.ForEach`, `CollectionsMarshal.AsSpan` |
| **Random test** *(active)* | 1062–1075 | Reads and prints [README.md](README.md) |

### 2.2 ProxyServer.cs — HTTP Reverse Proxy

[ProxyServer.cs](ProxyServer.cs) implements a full **HTTP reverse proxy** using `HttpListener` and `HttpClient`. Key features:

- **Constructor**: Accepts a target URL and one or more listener prefixes. Configures `HttpListener` with the provided prefixes.
- **Request forwarding**: Asynchronously receives incoming HTTP requests via `BeginGetContext`, rebuilds them as `HttpRequestMessage` objects, and forwards them to the target URL.
- **Header handling**: Properly separates content headers from request headers. Supports rewriting the `Host` and `Referer` headers to point to the target server.
- **Text rewriting**: For `text/html` and `application/json` responses, optionally rewrites target host references back to proxy host references so the browser continues routing through the proxy.
- **Custom `TryReplace`**: A manual character-by-character string replacement that returns a boolean indicating whether a replacement was made (unlike `String.Replace`).
- **`IDisposable`**: Properly disposes the `HttpListener`.

### 2.3 Complex.cs — Complex Number Arithmetic

[Complex.cs](Complex.cs) implements a `Complex` number `struct` with:

- **Fields**: `_real` and `_imaginary` (both `float`).
- **Operator overloads**: `+`, `-`, `*`, `/`, `==`, `!=`.
- **Static methods**: `Add`, `Subtract`, `Multiply`, `Divide`, `Magnitude`.
- **Overrides**: `Equals`, `GetHashCode`, `ToString`.

### 2.4 Supporting Files

| File | Purpose |
|---|---|
| [Blocklist.txt](Blocklist.txt) | ~115K lines of EasyList/uBlock Origin ad-blocking filter rules. Output of the "YouTube blocklist" experiment. |
| [layout.json](layout.json) | Sample JSON representing a form layout with controls, table layouts, and field pointers. Used by the "Jason Bourne" experiment. |
| [test.json](test.json) | Another layout definition in a compact array-based format (watchlist form). |
| [Untitled-1.json](Untitled-1.json) | XML-schema-typed JSON (`ArrayOfAnyType`) representing the same layout structure in a different serialization format. |
| [HadFun.html](HadFun.html) | A joke HTML page: "Press NO if you can?" — the "NO" button moves away from the cursor on hover. |
| [README.md](README.md) | ASCII art of a skull (no actual project documentation). |

---

## 3. Code Smells and Refactoring Opportunities

### 3.1 Monolithic Entry Point

**Severity: High**

[Program.cs](Program.cs) is a 1075-line file containing ~20 unrelated experiments in a single top-level statements file. All code sits at the global scope, separated only by `#region` directives and comment markers.

**Recommendation**: Extract each experiment into its own class or static method (e.g., `Experiments/EncryptionDemo.cs`, `Experiments/DesignPatterns/MementoDemo.cs`). Use a menu-driven `Program.cs` that lets the user select which experiment to run.

### 3.2 Commented-Out Code as Documentation

**Severity: High**

The entire project's codebase is commented out. This is effectively dead code that cannot be compiled or verified. Over time, these snippets will silently rot as APIs evolve.

**Recommendation**: Either un-comment and organize the experiments into runnable modules, or extract them into separate files/projects. Consider using `#if` preprocessor directives or a command-line argument selector instead of mass commenting.

### 3.3 Lack of Separation of Concerns

**Severity: Medium**

`ProxyServer.cs` and `Complex.cs` are well-structured standalone components, but they live alongside the experimental `Program.cs` with no namespace hierarchy, no project layering, and no test project.

**Recommendation**: Introduce namespaces (e.g., `HadFun.Networking`, `HadFun.Math`). Create a separate unit test project for `Complex` and `ProxyServer`.

### 3.4 No Unit Tests

**Severity: Medium**

Despite the project name being "Test" (from [Test.csproj](Test.csproj)), there are zero actual test cases. `Complex` has well-defined arithmetic that is ideal for unit testing but has none.

**Recommendation**: Add an xUnit/NUnit test project. The `Complex` struct in particular has clear expected-output semantics for each operator.

### 3.5 Static `HttpClient` in ProxyServer

**Severity: Low**

`ProxyServer` uses `private static readonly HttpClient _client = new HttpClient()`. While reuse is correct (avoiding socket exhaustion), the instance is never disposed and cannot be configured per-proxy (e.g., different timeouts or handlers).

**Recommendation**: Accept an `HttpClient` (or `IHttpClientFactory`) via dependency injection.

### 3.6 `async void` in ProxyServer.ProcessRequest

**Severity: Medium**

The `ProcessRequest(IAsyncResult)` callback method on [line 63 of ProxyServer.cs](ProxyServer.cs#L63) is `async void`. Exceptions thrown in `async void` methods crash the process because they cannot be observed.

**Recommendation**: Wrap the body in a `try/catch` block, or restructure to use `Task`-based patterns instead of `BeginGetContext`/`EndGetContext`.

### 3.7 Float vs Double in Complex

**Severity: Low**

`Complex` uses `float` for real/imaginary components. The BCL's `System.Numerics.Complex` uses `double`. Using `float` leads to precision loss for many arithmetic operations, and the `== 0.0f` comparison in division is fragile for floating-point.

**Recommendation**: Use `double` for fields, or implement `IEquatable<Complex>` with an epsilon comparison.

### 3.8 Duplicate Naming

**Severity: Low**

The project name is "Test" ([Test.csproj](Test.csproj), [Test.sln](Test.sln)) while the root namespace is `HadFun-` (which is not a valid C# identifier due to the hyphen). This causes confusion and may trigger build warnings.

**Recommendation**: Align project name and namespace. Remove the hyphen from the namespace or use an underscore.

---

## 4. Functional Inconsistencies and Potential Bugs

### 4.1 ProxyServer `TryReplace` — Partial Match Bug

In [ProxyServer.cs](ProxyServer.cs#L177-L226), the `TryReplace` method uses a manual character-scanning approach. When a partial match fails (characters matched `oldValue[0..offset]` but then diverged), the code replays the buffered characters:

```csharp
for (int j = 0; j < offset; j++)
{
    sb.Append(input[i - offset + j]);
}
sb.Append(c);
offset = 0;
```

This fails to re-examine the replayed characters for overlapping matches. For example, searching for `"aab"` in `"aaab"` would miss the match because the first `a` starts a match attempt, the second `a` continues it, the third `a` breaks the pattern, and the replayed characters are dumped without re-scanning. This is a correctness bug compared to `String.Replace`.

### 4.2 ProxyServer — No Error Handling in ProcessRequest

The `ProcessRequest(HttpListenerContext)` method in [ProxyServer.cs](ProxyServer.cs#L70) performs HTTP I/O but has no `try/catch`. Network failures, DNS resolution errors, or target server timeouts will propagate as unhandled exceptions in an `async void` context, potentially crashing the application.

### 4.3 Complex Division — Float Equality Check

In [Complex.cs](Complex.cs#L84-L85):

```csharp
if (c2._real == 0.0f && c2._imaginary == 0.0f)
```

Direct floating-point equality comparison is unreliable. A near-zero complex number (e.g., `1e-45f`) would pass this check but could still cause extreme results or overflow.

### 4.4 Complex.GetHashCode — XOR Collision

In [Complex.cs](Complex.cs#L53):

```csharp
return _real.GetHashCode() ^ _imaginary.GetHashCode();
```

XOR is symmetric, so `new Complex(a, b)` and `new Complex(b, a)` produce the same hash code, increasing collision rates in hash-based collections.

**Recommendation**: Use `HashCode.Combine(_real, _imaginary)`.

### 4.5 Copilot-Generated `IsComplex` — Logically Incorrect

In the "GitHub Copilot fun" region ([Program.cs](Program.cs#L710-L730)), the `IsComplex` method is supposed to identify complex numbers but returns `true` for any non-zero input, which is mathematically meaningless — all numbers with an imaginary component are complex, and the method's logic is tautological.

### 4.6 YouTube Blocklist — `watch.Start()` Called Twice

In the "YouTube blocklist" region ([Program.cs](Program.cs#L997)):

```csharp
watch.Start();  // This should be watch.Stop()
Console.WriteLine("Elapsed time: " + watch.ElapsedMilliseconds / 1000 + " seconds");
```

`watch.Start()` is called instead of `watch.Stop()`. The stopwatch is already running, and calling `Start()` again on a running `Stopwatch` is a no-op in .NET, so the elapsed time will still include subsequent execution time. This appears to be a copy-paste bug — it should be `watch.Stop()`.

### 4.7 ConcurrentQueue.ClearByItem — Lock Undermines Concurrency

In [Program.cs](Program.cs#L864-L880), the `ClearByItem` extension method uses `lock (queue)` on the `ConcurrentQueue`. This defeats the purpose of using a lock-free concurrent collection. Other threads accessing the queue concurrently will not honor this lock because `ConcurrentQueue` does not use external locking internally.

---

## 5. Assumptions and Limitations

1. **Experimentation only**: This project is clearly a personal learning sandbox, not a production codebase. Most code is commented out and intended to be un-commented selectively for testing.

2. **No CI/CD or build verification**: Since all experiments are commented out, the project compiles to a trivial console app. There is no automated testing, no build pipeline, and no way to verify the experiments still compile without manual intervention.

3. **Target framework**: The project targets `net10.0` (.NET 10), which as of the documented date (February 2025) is a preview. Some APIs like `CollectionsMarshal.AsSpan` require recent .NET versions.

4. **Unsafe code enabled**: `AllowUnsafeBlocks` is set to `true` in the project file to support the "Get memory address of an object" experiment, which uses raw pointer manipulation. This reduces the safety guarantees of the runtime.

5. **External dependencies**: The "YouTube blocklist" experiment downloads from external URLs (easylist.to, GitHub raw content). These URLs may change or become unavailable.

6. **Single-user design**: The `ProxyServer` uses `HttpListener` which requires URL reservation on Windows (`netsh http add urlacl`). Running without admin privileges on the configured prefixes will fail.

7. **Data files**: The JSON files ([layout.json](layout.json), [test.json](test.json), [Untitled-1.json](Untitled-1.json)) appear to represent UI form layouts from a proprietary application (APL control types, field pointers, buffer indices). Their format is not documented and they seem to be test fixtures for the JSON manipulation experiments.

8. **Invalid root namespace**: The `RootNamespace` in [Test.csproj](Test.csproj) is set to `HadFun-`, which contains a hyphen — an invalid character in C# identifiers. This will cause issues if code tries to use the namespace explicitly.
