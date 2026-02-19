# HadFun- Feature Plan & Impact Assessment

> Based on analysis documented in [DOCUMENTATION.md](DOCUMENTATION.md).

---

## Table of Contents

1. [Feature 1 — Experiment Runner Framework](#feature-1--experiment-runner-framework)
2. [Feature 2 — ProxyServer Hardening](#feature-2--proxyserver-hardening)
3. [Feature 3 — Complex Struct Modernization](#feature-3--complex-struct-modernization)
4. [Feature 4 — Unit Test Infrastructure](#feature-4--unit-test-infrastructure)
5. [Feature 5 — Project Identity and Build Hygiene](#feature-5--project-identity-and-build-hygiene)
6. [Feature 6 — Blocklist Downloader as a Reusable Tool](#feature-6--blocklist-downloader-as-a-reusable-tool)
7. [Summary Matrix](#summary-matrix)

---

## Feature 1 — Experiment Runner Framework

### Description

Replace the monolithic, all-commented-out [Program.cs](Program.cs) (1075 lines, ~20 `#region` blocks) with a structured experiment runner. Each experiment becomes its own class implementing a common `IExperiment` interface, and `Program.cs` becomes a menu-driven selector.

### Risks

| Risk | Severity | Mitigation |
|---|---|---|
| **Experiments won't compile.** Several commented-out regions reference undefined types (e.g., `PersonRecord`, `Person`, `IPerson`, `Test`, `Test1`) or use `.Dump()` methods (LINQPad-style) that don't exist in console apps. Un-commenting will produce dozens of compile errors. | High | Triage each region individually; fix or discard non-compiling experiments before extraction. |
| **Type name collisions.** At least 3 different `Person` classes are defined across different regions (Records, Json Validation, GitHub Copilot fun). They cannot coexist in the same namespace. | High | Place each experiment in its own namespace (e.g., `HadFun.Experiments.Records`, `HadFun.Experiments.CopilotFun`). |
| **Global-scope closures.** Some experiments capture top-level variables (e.g., `asyncCallCounter`, `watch`) in local functions. These closures will need refactoring when moving to instance methods. | Medium | Convert captured variables to fields or method parameters in the experiment class. |
| **Unsafe code dependency.** The "Get memory address" experiment requires `AllowUnsafeBlocks`. This flag remains project-wide regardless of which experiment runs. | Low | Acceptable for a sandbox project. Document the requirement. |

### Necessary Refactoring

1. **Create `IExperiment` interface** — `Task RunAsync()` + `string Name { get; }` + `string Description { get; }`.
2. **Extract each `#region` into `Experiments/<Name>Experiment.cs`** — 20 files, one per region.
3. **Fix compile errors per experiment** — resolve missing types, duplicate names, `.Dump()` calls.
4. **Rewrite `Program.cs`** — enumerate implementations via reflection or explicit registration; present a numbered menu; run the selected experiment.
5. **Move supporting types** (e.g., `Editor`, `LastEditorState`, `StateContainer`, `Builder`, `Director`, `Product`, `Video`, `VideoDownloader`, `EmailService`) into their experiment files or a shared `Models/` folder.

### Recommendations

- Start with the easiest experiments (Sleep sort, String immutability, Encryption) to build momentum.
- Skip or archive experiments that depend on external services (`CallUriAsync` hitting `engineering.fb.com`) unless you mock them.
- Use `--experiment <name>` CLI argument as an alternative to the interactive menu for CI/scripting.

### Potential Issues

- The "Jason Bourne" region references `firstChild`, `children`, and `product` variables that are **never declared** — the experiment is incomplete and will not compile as-is.
- The "Game of Concurrency with CancellationToken" region creates a **new `HttpClient` per request** inside the loop, which will exhaust sockets on .NET < 6 (mitigated on .NET 10 by `SocketsHttpHandler` pooling, but still bad practice).
- The "Json Validation" region uses `JsonSchemaGenerator` and `JsonSchema` from Newtonsoft.Json.Schema, which was **moved to a separate NuGet package** (`Newtonsoft.Json.Schema`). The current project only references `Newtonsoft.Json`. It will not compile without adding the schema package.

### Effort Estimate

| Task | Effort |
|---|---|
| Design IExperiment + runner | 2–3 hours |
| Extract & fix 20 experiments | 8–12 hours (avg. 30–40 min each, some trivial, some need debugging) |
| Rewrite Program.cs menu | 1–2 hours |
| **Total** | **~11–17 hours** |

### Rewrite vs. Refactor Verdict

**Refactor.** The experiments are self-contained snippets. Extraction is mechanical — the structure is already there via `#region` blocks. A full rewrite would discard the learning value of the original code.

---

## Feature 2 — ProxyServer Hardening

### Description

Make [ProxyServer.cs](ProxyServer.cs) production-quality: fix the `TryReplace` bug, add error handling, support HTTPS, and allow dependency-injected `HttpClient`.

### Risks

| Risk | Severity | Mitigation |
|---|---|---|
| **`async void` crash path.** The [callback on line 63](ProxyServer.cs#L63) is `async void`. Any unhandled exception in the forwarding pipeline (`SendAsync`, stream copy, etc.) will terminate the process. | High | Wrap the entire `ProcessRequest(HttpListenerContext)` body in `try/catch` with logging. |
| **`TryReplace` overlapping-match bug.** Searching for `"aab"` in `"aaab"` misses the match (see [DOCUMENTATION.md §4.1](DOCUMENTATION.md)). In production, this means some HTML rewrites silently fail, breaking proxied pages. | Medium | Replace custom `TryReplace` with `string.Replace` + a simple `!= originalString` check, or use `Regex.Replace`. The performance difference is negligible for HTML payloads. |
| **No HTTPS support.** `HttpListener` can handle HTTPS but requires a certificate bound to the port via `netsh http add sslcert`. Without this, the proxy cannot intercept HTTPS traffic. | Medium | Document the limitation. For full HTTPS interception, consider migrating to Kestrel or YARP. |
| **Static `HttpClient` shared across all proxy instances.** If two `ProxyServer` instances target different hosts, they share timeout/handler config. | Low | Accept `HttpClient` via constructor. Existing static instance becomes the default. |

### Necessary Refactoring

1. **Add `try/catch` in `ProcessRequest(IAsyncResult)`** — log exceptions, close the context response gracefully.
2. **Fix `TryReplace`** — either replace with `string.Replace` + equality check, or fix the offset-replay logic to re-scan for overlapping matches (KMP-style backtracking).
3. **Add constructor overload** accepting `HttpClient` — `public ProxyServer(Uri targetUrl, HttpClient client, params string[] prefixes)`.
4. **Add `CancellationToken` support** — allow graceful shutdown via `StopAsync(CancellationToken)` instead of just `Stop()`.
5. **Add logging** — accept an `ILogger` or `Action<string>` for request/error logging.

### Recommendations

- If the proxy is intended for anything beyond local debugging, consider replacing `HttpListener` with **YARP** (Yet Another Reverse Proxy) or **Kestrel middleware**. YARP handles connection pooling, load balancing, health checks, and header transforms out of the box.
- For the current scope (local debugging proxy), the fixes above are sufficient.

### Effort Estimate

| Task | Effort |
|---|---|
| Error handling + try/catch | 1–2 hours |
| Fix TryReplace | 1–2 hours |
| HttpClient DI | 1 hour |
| CancellationToken support | 1–2 hours |
| Logging | 1 hour |
| **Total** | **~5–9 hours** |

### Rewrite vs. Refactor Verdict

**Refactor** for the current use case (local dev proxy). **Rewrite with YARP** if production reverse-proxy functionality is ever needed — the `HttpListener` approach doesn't scale.

---

## Feature 3 — Complex Struct Modernization

### Description

Bring [Complex.cs](Complex.cs) up to modern C# standards: fix the hash code collision, use `double` precision, implement `IEquatable<Complex>`, `IFormattable`, and generic math interfaces.

### Risks

| Risk | Severity | Mitigation |
|---|---|---|
| **Breaking change: `float` → `double`.** Any code storing `Complex` values as `float` or serializing them will break. | Medium | Since no code currently uses `Complex` (all experiments are commented out), this is safe to change now. |
| **Semantic change in equality.** Moving from exact `float` equality to epsilon-based comparison changes behavior for edge cases. | Low | Document the tolerance. Provide both `Equals` (epsilon) and `ExactEquals` (bitwise) methods. |

### Necessary Refactoring

1. **Change fields from `float` to `double`** — aligns with `System.Numerics.Complex`.
2. **Fix `GetHashCode`** — replace `XOR` with `HashCode.Combine(_real, _imaginary)`.
3. **Implement `IEquatable<Complex>`** — strongly-typed `Equals(Complex other)` with optional epsilon.
4. **Implement `IFormattable`** — support format strings (e.g., `"G"`, `"F2"`).
5. **Add missing operations** — `Conjugate`, `Phase`/`Argument`, `FromPolarCoordinates`, `Pow`, `Sqrt`, `Abs` (alias for `Magnitude`).
6. **Consider implementing `INumber<Complex>`** (C# 11+ generic math) — enables use with generic algorithms.
7. **Add XML doc comments** to all public members (some already have them, most don't).

### Recommendations

- If the goal is learning, keep this custom `Complex` and evolve it. If the goal is utility, **delete it and use `System.Numerics.Complex`** instead — it has all of the above and more, with hardware-accelerated math.
- Add a benchmark experiment comparing this custom `Complex` vs `System.Numerics.Complex`.

### Effort Estimate

| Task | Effort |
|---|---|
| float → double + IEquatable | 1 hour |
| GetHashCode fix | 10 minutes |
| Additional operations | 2–3 hours |
| IFormattable + INumber<Complex> | 2–3 hours |
| XML docs | 1 hour |
| **Total** | **~6–8 hours** |

### Rewrite vs. Refactor Verdict

**Refactor.** The struct is small (132 lines), well-organized, and has correct arithmetic. The changes are additive.

---

## Feature 4 — Unit Test Infrastructure

### Description

Add a test project with unit tests for `Complex` (arithmetic, edge cases, equality) and `ProxyServer` (header rewriting, `TryReplace`, error handling).

### Risks

| Risk | Severity | Mitigation |
|---|---|---|
| **ProxyServer is hard to test.** It depends on `HttpListener` (requires admin/URL reservation) and makes real HTTP calls via a static `HttpClient`. | Medium | Refactor `ProxyServer` to accept `HttpClient` first (Feature 2). Use `HttpMessageHandler` mocks for unit tests. For integration tests, use `TestServer` from ASP.NET Core. |
| **Test project adds complexity.** A separate `.csproj` with its own dependencies (xUnit, Moq, etc.) increases the solution footprint. | Low | Acceptable trade-off for a learning project. |

### Necessary Refactoring

1. **Create `HadFun.Tests/HadFun.Tests.csproj`** — xUnit + `Moq` or `NSubstitute`.
2. **Add project reference** to the main project.
3. **Write `ComplexTests.cs`** — test each operator, edge cases (divide by zero, magnitude, hash code collisions).
4. **Write `ProxyServerTests.cs`** — mock `HttpClient`, test header rewriting logic, `TryReplace` correctness.
5. **Add `TryReplace` test cases** that specifically cover the overlapping-match bug documented in §4.1.

### Recommendations

- Use **xUnit** (most popular in .NET ecosystem, built-in `Theory`/`InlineData` for parameterized tests).
- Start with `Complex` tests — they're pure math with no I/O dependencies.
- Use **BenchmarkDotNet** instead of manual `Stopwatch` loops for the performance experiments (not a unit test, but a natural evolution of the benchmarking experiments).

### Effort Estimate

| Task | Effort |
|---|---|
| Test project scaffolding | 30 minutes |
| ComplexTests (15–20 test cases) | 2–3 hours |
| ProxyServer refactoring for testability | 2–3 hours |
| ProxyServerTests (10–15 test cases) | 3–4 hours |
| **Total** | **~8–11 hours** |

### Rewrite vs. Refactor Verdict

**New work** — no tests exist today.

---

## Feature 5 — Project Identity and Build Hygiene

### Description

Fix the project/solution naming, invalid namespace, and build configuration issues.

### Risks

| Risk | Severity | Mitigation |
|---|---|---|
| **Renaming the project breaks file paths.** Renaming `Test.csproj` → `HadFun.csproj` requires updating [Test.sln](Test.sln), `bin/`/`obj/` paths, and any CI references. | Low | Delete `bin/` and `obj/`, rename files, update solution. Rebuild. |
| **`RootNamespace` contains a hyphen.** `HadFun-` is not a valid C# identifier. The compiler auto-escapes it in generated files but it will cause issues if any code explicitly references the namespace. | Low | Change to `HadFun` (no hyphen). |

### Necessary Refactoring

1. **Rename `Test.csproj`** → `HadFun.csproj`.
2. **Rename `Test.sln`** → `HadFun.sln`.
3. **Fix `RootNamespace`** — `HadFun-` → `HadFun`.
4. **Add a `.gitignore`** — the `bin/` and `obj/` directories are checked in. Add a standard .NET `.gitignore`.
5. **Add a proper `README.md`** — replace the ASCII art with actual project documentation (or rename the ASCII art to `skull.txt` and create a real README).
6. **Remove `Blocklist.txt` from source control** — it's 115K lines of generated output. Add it to `.gitignore` and regenerate on demand.

### Effort Estimate

| Task | Effort |
|---|---|
| Rename project/solution | 30 minutes |
| Fix namespace | 10 minutes |
| Add .gitignore | 10 minutes |
| Write proper README.md | 30 minutes |
| Cleanup Blocklist.txt | 10 minutes |
| **Total** | **~1.5–2 hours** |

### Rewrite vs. Refactor Verdict

**Refactor.** Trivial mechanical changes.

---

## Feature 6 — Blocklist Downloader as a Reusable Tool

### Description

Extract the "YouTube blocklist" experiment (lines 964–998 of [Program.cs](Program.cs)) into a standalone CLI tool or class that downloads, merges, and formats ad-blocking filter lists.

### Risks

| Risk | Severity | Mitigation |
|---|---|---|
| **External URL dependency.** The tool downloads from 17+ URLs (easylist.to, GitHub raw). Any URL change or outage breaks the tool. | Medium | Add URL configuration (JSON/YAML config file). Add retry logic with `Polly`. Log failures per-URL without aborting the entire run. |
| **Memory pressure.** The current code concatenates all filter lists into a single `string`, then splits and reformats. For 115K+ lines, this creates significant GC pressure. | Low | Use `StreamReader` line-by-line processing instead of full-string concatenation. |
| **Bug: `watch.Start()` instead of `watch.Stop()`** on [line 997](Program.cs#L997). | Low | Fix during extraction. |

### Necessary Refactoring

1. **Create `BlocklistDownloader.cs`** — async method accepting a list of URLs and an output path.
2. **Stream-based processing** — download each list and write lines directly to the output file.
3. **Configurable URL list** — read from a config file or accept as parameters.
4. **Retry/logging** — use `Polly` or simple retry loops for transient HTTP failures.
5. **Fix the `watch.Start()` bug.**

### Effort Estimate

| Task | Effort |
|---|---|
| Extract to class | 1 hour |
| Stream-based rewrite | 1–2 hours |
| Config file + retry | 1–2 hours |
| CLI argument parsing | 30 minutes |
| **Total** | **~3.5–5.5 hours** |

### Rewrite vs. Refactor Verdict

**Rewrite.** The current code is a quick script concatenating strings. A proper tool needs streaming I/O, error handling, and configurability — starting fresh is cleaner.

---

## Summary Matrix

| # | Feature | Effort | Risk | Rewrite? | Priority |
|---|---|---|---|---|---|
| 1 | Experiment Runner Framework | 11–17h | High | Refactor | **P1** — unlocks all other work |
| 2 | ProxyServer Hardening | 5–9h | High | Refactor (YARP if production) | **P2** — fixes crash paths |
| 3 | Complex Modernization | 6–8h | Low | Refactor | **P3** — quality improvement |
| 4 | Unit Test Infrastructure | 8–11h | Medium | New | **P2** — enables safe refactoring |
| 5 | Project Identity & Build Hygiene | 1.5–2h | Low | Refactor | **P1** — quick win, do first |
| 6 | Blocklist Downloader Tool | 3.5–5.5h | Medium | Rewrite | **P4** — nice-to-have |

### Recommended Execution Order

```
P1: Feature 5 (Build Hygiene)          →  1.5–2h    — clears foundational issues
P1: Feature 1 (Experiment Runner)      →  11–17h    — makes the codebase usable
P2: Feature 4 (Unit Tests)             →  8–11h     — enables safe changes to #2 and #3
P2: Feature 2 (ProxyServer Hardening)  →  5–9h      — fixes critical bugs
P3: Feature 3 (Complex Modernization)  →  6–8h      — polish
P4: Feature 6 (Blocklist Tool)         →  3.5–5.5h  — if desired
```

**Total estimated effort: ~35–52 hours** (roughly 1–1.5 weeks of focused development).
