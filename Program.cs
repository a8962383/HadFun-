// =============================================================================
// HadFun- :: Program.cs
// =============================================================================
//
// Entry point for the HadFun- experimentation sandbox. This file contains ~20
// self-contained code experiments organized into #region blocks. All experiments
// are commented out except "Random test" at the bottom, which simply prints the
// README.md ASCII art.
//
// To run a specific experiment, uncomment its region and comment out the others.
//
// Experiments included:
//   - Proxy server              : HTTP reverse proxy via ProxyServer class
//   - Jason Bourne              : JSON manipulation (JObject, JSONPath, LINQ)
//   - Beauty of C# 9.0          : Records, 'with' expressions, value equality
//   - Json Validation           : JSON Schema validation (Newtonsoft.Json.Schema)
//   - Encryption                : AES encrypt/decrypt round-trip
//   - Records in C# 9.0         : record vs class, IPerson interface
//   - Hack into private vars    : Reflection with BindingFlags.NonPublic
//   - String immutability       : Reference-type vs value-type semantics
//   - Parallelism + Concurrency : Sequential vs AsParallel() benchmarking
//   - CancellationToken         : Async HTTP with cancellation
//   - Sleep sort                : Novelty sorting via Thread.Sleep
//   - Design Patterns           : Memento, Builder
//   - Events & delegates        : Custom event/delegate pattern
//   - GitHub Copilot fun        : AI-generated Person class & utilities
//   - Drive free space           : ConcurrentDictionary + Parallel.For + DriveInfo
//   - LINQ performance          : FirstOrDefault vs Where().FirstOrDefault()
//   - Memory address            : Unsafe __makeref pointer dereferencing
//   - ConcurrentQueue extension : ClearByItem<T> extension method
//   - Regex vs MailAddress      : Email validation performance comparison
//   - Channel                   : Bounded channel producer/consumer
//   - Pattern matching ITuple   : Custom ITuple + positional patterns
//   - Double parsing            : Floating-point precision exploration
//   - YouTube blocklist         : Ad-block filter list downloader/merger
//   - Loop performance          : for / foreach / Parallel / Span benchmarks
//   - Random test (ACTIVE)      : Reads and prints README.md
// =============================================================================

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using System.Threading.Channels;
using System.Runtime.InteropServices;

#region Proxy server
// Demonstrates the ProxyServer class — starts a local HTTP reverse proxy
// that forwards all requests to Google and rewrites host references in
// HTML/JSON responses so the browser routes through the proxy.
//
// object[] inputArray = new object[10];
// string[] resultArray = Array.ConvertAll(inputArray, x => x.ToString());

// using (var server = new ProxyServer("https://www.google.com", "http://localhost:5050/", "http://127.0.0.1:5050/"))
// {
//     server.Start();
//     Console.WriteLine("Press ESC to stop server.");
//     while (true)
//     {
//         var key = Console.ReadKey(true);
//         if (key.Key == ConsoleKey.Escape)
//             break;
//     }
//     server.Stop();
// }
#endregion

#region Jason Bourne
// Explores Newtonsoft.Json JObject/JArray manipulation, JSONPath queries,
// and LINQ-to-JSON for navigating and mutating a layout.json structure.
//
// dynamic jObj = JObject.Parse(File.ReadAllText("layout.json"));
// JArray layoutArray = (JArray)jObj.Layout;
// dynamic layout = (JObject)layoutArray.First;

// firstChild["ProductName"] = "Last Child";
// var lastChild = (JObject)children.Last();
// lastChild["ProductName"] = "Last Child";
// lastChild.Property("Price").Remove();
// Console.WriteLine(layout.Form);

// //Using JSONPath
// var lastChildJsonPath = (JObject)product.SelectToken("$.Children[?(@.ProductName == 'Last Child')]");
// lastChildJsonPath.Add("New Property", "New JToken");
// Console.WriteLine(lastChildJsonPath);

// //Using LINQ
// var fChildLinq = (JObject)product["Children"].Where(j => j.SelectToken("ProductName").ToString().Equals("First Child")).SingleOrDefault();
// fChildLinq.Add("New Property", "New JToken");
// // Console.WriteLine(fChildLinq);
// Console.WriteLine(product);

#endregion

#region Beauty of C# 9.0
// Demonstrates C# 9 record types, the 'with' expression for non-destructive
// mutation, and value-based equality semantics.
//
// var p1 = new PersonRecord("MD", "RN");
// var p2 = p1 with { FirstName = "Paul" }; //this is crazy
// var p3 = p1 with { FirstName = "MD" }; //this is crazy


// var p1 = new Person("MD", "RN");
// var p2 = p1;
// p2.FirstName = "Paul";
// var p3 = p1;
// p3.FirstName = "MD";

// Console.WriteLine(p1 == p2); //this is crazy
// Console.WriteLine(p1 == p3); //this is crazy

#endregion

#region Json Validation
// Generates a JSON Schema from a C# type, validates JObjects against it,
// and performs deep-equality comparison with JToken.DeepEquals.
// NOTE: Requires the separate Newtonsoft.Json.Schema NuGet package.
//
// JsonSchemaGenerator generator = new JsonSchemaGenerator();

// JsonSchema schema = generator.Generate(typeof(Person));

// JObject jBond = JObject.Parse(@"{
//   'FirstName': 'James',
//   'LastName' : 'Bond',
//   'Child': {
//     'Id': 1
//   }
// }");

// JObject jBondFalse = JObject.Parse(@"{
//   'FirstName': 'James',
//   'LastName': 'Bond',
//   'Child': {
//     'Id': 1
//   }
// }");

// IList<string> errorMessages;
// bool valid = jBondFalse.IsValid(schema, out errorMessages);

// Console.WriteLine("{0} - {1}", valid ? "Valid Json" : "Invalid Json", errorMessages.Count > 0 ? errorMessages[0] : 0);
// Console.WriteLine(JToken.DeepEquals(jBond, jBondFalse));

#endregion

#region Encryption
// AES symmetric encryption round-trip: encrypts a plaintext string into
// a byte array, then decrypts it back. Uses a 16-byte key derived from
// the plaintext itself (for demonstration only — not a secure practice).
//
// string original = "Everything else is Schnullibulli!";
// byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
// byte[] codexKey = Encoding.ASCII.GetBytes("Everything else is Schnullibulli!", 0, 16);

// // Create a new instance of the Aes
// // class.  This generates a new key and initialization
// // vector (IV).
// using (Aes myAes = Aes.Create())
// {

//     // Encrypt the string to an array of bytes.
//     byte[] encrypted = EncryptStringToBytes_Aes(original, codexKey, myAes.IV);

//     // Decrypt the bytes to a string.
//     string roundtrip = DecryptStringFromBytes_Aes(encrypted, codexKey, myAes.IV);

//     //Display the original data and the decrypted data.
//     Console.WriteLine("Original:   {0}", original);
//     Console.WriteLine("Round Trip: {0}", roundtrip);
// }

// static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
// {
//     // Check arguments.
//     if (plainText == null || plainText.Length <= 0)
//         throw new ArgumentNullException("plainText");
//     if (Key == null || Key.Length <= 0)
//         throw new ArgumentNullException("Key");
//     if (IV == null || IV.Length <= 0)
//         throw new ArgumentNullException("IV");
//     byte[] encrypted;

//     // Create an Aes object
//     // with the specified key and IV.
//     using (Aes aesAlg = Aes.Create())
//     {
//         aesAlg.Key = Key;
//         aesAlg.IV = IV;

//         // Create an encryptor to perform the stream transform.
//         ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

//         // Create the streams used for encryption.
//         using (MemoryStream msEncrypt = new MemoryStream())
//         {
//             using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
//             {
//                 using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
//                 {
//                     //Write all data to the stream.
//                     swEncrypt.Write(plainText);
//                 }
//                 encrypted = msEncrypt.ToArray();
//             }
//         }
//     }

//     // Return the encrypted bytes from the memory stream.
//     return encrypted;
// }

// static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
// {
//     // Check arguments.
//     if (cipherText == null || cipherText.Length <= 0)
//         throw new ArgumentNullException("cipherText");
//     if (Key == null || Key.Length <= 0)
//         throw new ArgumentNullException("Key");
//     if (IV == null || IV.Length <= 0)
//         throw new ArgumentNullException("IV");

//     // Declare the string used to hold
//     // the decrypted text.
//     string plaintext = null;

//     // Create an Aes object
//     // with the specified key and IV.
//     using (Aes aesAlg = Aes.Create())
//     {
//         aesAlg.Key = Key;
//         aesAlg.IV = IV;

//         // Create a decryptor to perform the stream transform.
//         ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

//         // Create the streams used for decryption.
//         using (MemoryStream msDecrypt = new MemoryStream(cipherText))
//         {
//             using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
//             {
//                 using (StreamReader srDecrypt = new StreamReader(csDecrypt))
//                 {

//                     // Read the decrypted bytes from the decrypting stream
//                     // and place them in a string.
//                     plaintext = srDecrypt.ReadToEnd();
//                 }
//             }
//         }
//     } 

//     return plaintext;
// }

#endregion

#region Records in C# 9.0
// Compares positional record declarations with traditional class-based
// implementations of a Person type, highlighting value equality behavior.
//
// public record PersonRecord(string FirstName, string LastName);
// public interface IPerson
// {
//     string FirstName { get; set; }
//     string LastName { get; set; }
// }

// public class Person : IPerson
// {
//     public string FirstName { get; set; }
//     public string LastName { get; set; }

//     public Person(string FirstName, string LastName)
//     {
//         this.FirstName = FirstName;
//         this.LastName = LastName;
//     }
// }
#endregion

#region Hack into private variables
// Uses System.Reflection to access private fields (BindingFlags.NonPublic)
// on an object instance, demonstrating that encapsulation is a compile-time
// guarantee, not a runtime one.
//
// var myInstance = new MyClass();
// var fieldInfo = typeof(MyClass).GetField("_secret", BindingFlags.NonPublic | BindingFlags.Instance);
// var secret = fieldInfo.GetValue(myInstance);
// foreach (var item in secret as IEnumerable)
// {
//     Console.WriteLine(item);
// }
// Console.ReadKey();

// public class MyClass
// {
//     private byte[] _secret = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
// }

#endregion

#region string is reference type but it's immutable
// Illustrates that while string is a reference type, its immutability means
// reassignment doesn't affect other references. Contrasts with a mutable
// class and a C# 9 record using the 'with' expression.
//
// string str = "str";
// string str1 = str + "_str1";
// str = string.Empty;
// Console.WriteLine(str1);

// Test x = new Test() { name = "MDRN" };
// Test y = x;
// x.name = string.Empty;
// Console.WriteLine("y.name: " + y.name);

// Test1 xx = new Test1("MDRN");
// Test1 yy = xx with {name = "NEWMDRN"}; 
// Console.WriteLine("yy.name: " + yy.name);

// int a = 5;
// int b = a + 4;
// a = 0;
// Console.WriteLine(b);

// public class Test
// {
//     public string name { get; set; }
// }

// public record Test1(string name);

#endregion

#region Game of Parallelism + Concurrency
// Benchmarks sequential Sum() vs PLINQ AsParallel().Sum() over a short.MaxValue
// range, measuring per-iteration nanosecond cost with Stopwatch.
//
// int[] array = Enumerable.Range(0, short.MaxValue).ToArray();

// Console.WriteLine("Seq. sum: " + array.Sum());
// Console.WriteLine("Par. sum: " + array.AsParallel().Sum());

// int m = 5000;
// var s1 = Stopwatch.StartNew();
// for (int i = 0; i < m; i++) array.Sum();
// s1.Stop();

// var s2 = Stopwatch.StartNew();
// for (int i = 0; i < m; i++) array.AsParallel().Sum();
// s2.Stop();

// Console.WriteLine("Seq. execution time: " + ((double)(s1.Elapsed.TotalMilliseconds * 1000000) / m).ToString("0.00 ns"));
// Console.WriteLine("Par. execution time: " + ((double)(s2.Elapsed.TotalMilliseconds * 1000000) / m).ToString("0.00 ns"));
// Console.ReadKey();
#endregion

#region Game of Concurrency with CancellationToken
// Simulates a UI thread making async HTTP calls with a CancellationTokenSource
// that cancels after 2 seconds, demonstrating cooperative cancellation and
// the interaction between UI responsiveness and background tasks.
//
// "Calling Facebook Code.".Dump();
// "UI thread is free now...".Dump();
// int uiCallCounter = 0;
// int asyncCallCounter = 0;
// var tokenSource = new CancellationTokenSource();
// var token = tokenSource.Token;
// Stopwatch watch = new Stopwatch();
// watch.Start();

// try
// {
//     while (watch.ElapsedMilliseconds < 5001)
//     {
//         CallUriAsync("https://engineering.fb.com/", token);
//         DummyButtonClickButton();
//         Thread.Sleep(100);
//         uiCallCounter++;

//         if (watch.ElapsedMilliseconds > 2000)
//         {
//             if (!token.IsCancellationRequested)
//             {
//                 Console.WriteLine("******Async operation for \"CallUriAsync\" has been cancelled******\n");
//                 tokenSource.Cancel();
//             }
//         }
//     }
// }
// catch (OperationCanceledException ex)
// {
//     Console.WriteLine(ex.Message);
// }
// finally
// {
//     tokenSource.Dispose();
// }

// watch.Stop();
// Console.WriteLine(Environment.NewLine + "# of dummy clicks => " + uiCallCounter);
// Console.WriteLine("# of async calls  => " + asyncCallCounter + Environment.NewLine);
// Console.ReadKey();

// void DummyButtonClickButton() => Console.Write("==>");

// async Task CallUriAsync(string uri, CancellationToken token)
// {
//     string content = string.Empty;
//     try
//     {
//         if (token.IsCancellationRequested)
//         {
//             // TODO: cleanup code

//             // return;
//             token.ThrowIfCancellationRequested();
//         }
//         content = await new HttpClient().GetStringAsync(uri);
//         Console.WriteLine("FB Code has a content length of " + content.Length);
//         asyncCallCounter++;
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine(ex.Message);
//     }
// }

#endregion

#region Sleep sort
// A joke sorting algorithm: each number sleeps for n*10ms, then prints.
// Numbers naturally appear in ascending order (assuming no scheduling jitter).
//
// var input = new[] { 1, 9, 2, 1, 3, 10, 83, 65, 23 };

// foreach (var n in input)
// {
//     Task.Run(() =>
//     {
//         Thread.Sleep(n * 10);
//         Console.WriteLine(n);
//     });
// }

// Console.ReadKey();

#endregion

#region Design Patterns

#region Behavioral Patterns

#region Memento
// Implements the Memento behavioral design pattern:
//   - Editor (Originator) — holds mutable state and can save/restore snapshots.
//   - LastEditorState (Memento) — immutable snapshot of editor state.
//   - StateContainer (Caretaker) — stores the last saved memento.
//
// Editor editor = new Editor();
// editor.Text = "1";

// // Store internal state in Manager/Caretaker/Organizer/StateContainer
// StateContainer stateContainer = new StateContainer();
// stateContainer.LastEditorState = editor.GetState();

// // Continue changing editor
// editor.Text = "2";
// stateContainer.LastEditorState = editor.GetState();

// editor.Text = "3";
// // Restore saved state
// editor.SetState(stateContainer.LastEditorState);

// // Wait for user
// Console.ReadKey();


// /// <summary>
// /// The 'Originator' class
// /// </summary>
// class Editor
// {
//     private string _text;

//     // Property
//     public string Text
//     {
//         get { return _text; }
//         set
//         {
//             _text = value;
//             Console.WriteLine("EditorText = " + _text);
//         }
//     }

//     // Creates memento 
//     public LastEditorState GetState()
//     {
//         return (new LastEditorState(_text));
//     }

//     // Restores original state
//     public void SetState(LastEditorState state)
//     {
//         Console.WriteLine("Restoring state...");
//         Text = state.Text;
//     }
// }

// /// <summary>
// /// The 'Memento' class
// /// </summary>
// class LastEditorState
// {
//     // Gets or sets state
//     public string Text { get; }

//     public LastEditorState(string text)
//     {
//         this.Text = text;
//     }
// }

// /// <summary>
// /// The 'Caretaker' class
// /// </summary>
// class StateContainer
// {
//     public LastEditorState LastEditorState { get; set; }
// }

#endregion

#endregion

#region Structural  Patterns

#endregion

#region Creational Patterns

#region Builder
// Implements the Builder creational design pattern:
//   - Director — orchestrates the construction steps.
//   - Builder (abstract) — defines the interface for building parts.
//   - ConcreteBuilder1/2 — produce different product configurations.
//   - Product — the assembled result.
//
// // Create director and builders
// Director director = new Director();

// Builder b1 = new ConcreteBuilder1();
// Builder b2 = new ConcreteBuilder2();

// // Construct two products
// director.Construct(b1);
// Product p1 = b1.GetResult();
// p1.Show();

// director.Construct(b2);
// Product p2 = b2.GetResult();
// p2.Show();

// Console.ReadKey();

// /// <summary>
// /// The 'Director' class
// /// </summary>
// class Director
// {
//     // Builder uses a complex series of steps
//     public void Construct(Builder builder)
//     {
//         builder.BuildPartA();
//         builder.BuildPartB();
//     }
// }

// /// <summary>
// /// The 'Builder' abstract class
// /// </summary>
// abstract class Builder
// {
//     public abstract void BuildPartA();
//     public abstract void BuildPartB();
//     public abstract Product GetResult();
// }

// /// <summary>
// /// The 'ConcreteBuilder1' class
// /// </summary>
// class ConcreteBuilder1 : Builder
// {
//     private Product _product = new Product();

//     public override void BuildPartA()
//     {
//         _product.Add("PartA");
//     }

//     public override void BuildPartB()
//     {
//         _product.Add("PartB");
//     }

//     public override Product GetResult()
//     {
//         return _product;
//     }
// }

// /// <summary>
// /// The 'ConcreteBuilder2' class
// /// </summary>
// class ConcreteBuilder2 : Builder
// {
//     private Product _product = new Product();

//     public override void BuildPartA()
//     {
//         _product.Add("PartX");
//     }

//     public override void BuildPartB()
//     {
//         _product.Add("PartY");
//     }

//     public override Product GetResult()
//     {
//         return _product;
//     }
// }

// /// <summary>
// /// The 'Product' class
// /// </summary>
// public class Product
// {
//     private List<string> _parts = new List<string>();

//     public void Add(string part)
//     {
//         _parts.Add(part);
//     }

//     public void Show()
//     {
//         Console.WriteLine("\nProduct Parts -------");
//         foreach (string part in _parts)
//             Console.WriteLine(part);
//     }
// }

#endregion

#endregion

#endregion

#region Events & delegates
// Demonstrates the C# event/delegate pattern:
//   1. VideoDownloader defines a delegate and event.
//   2. EmailService subscribes to the event.
//   3. When a video finishes downloading, the event fires and EmailService reacts.
//
// var video = new Video() { Title = "Mission Impossible 1", Uri = "https://code.jquery.com/git/jquery-git.slim.min.js" };
// var videoDownloader = new VideoDownloader();

// var emailService = new EmailService();
// videoDownloader.VideoDownloaded += emailService.OnVideoDownloaded;

// videoDownloader.Download(video);

// public class Video
// {
//     public string Title { get; set; }
//     public string Uri { get; set; }
// }

// public class VideoDownloader
// {
//     //1. Define delegate
//     //2. Define event
//     //3. Raise event

//     public delegate void VideoEncodedEventHandler(object source, EventArgs args);
//     public event VideoEncodedEventHandler VideoDownloaded;
//     public void Download(Video video)
//     {
//         System.Console.WriteLine("VideoDownloader: Downloading \"" + video.Title + "\" using URI...");
//         Thread.Sleep(2500);

//         OnVideoDownloaded(video);
//     }

//     public void OnVideoDownloaded(Video video)
//     {
//         if (VideoDownloaded != null)
//             VideoDownloaded(this, EventArgs.Empty);
//     }
// }

// public class EmailService
// {
//     public void OnVideoDownloaded(object source, EventArgs args)
//     {
//         System.Console.WriteLine("EmailService: Sending email for...");
//     }
// }

#endregion

#region GitHub Copilot fun
// Code generated entirely by GitHub Copilot from comment prompts.
// Includes: Person CRUD helpers, prime check, complex-number identification
// (logically incorrect), and tongue-in-cheek framework opinions.
//
// // Person class with name, age, and email
// public class Person
// {
//     public string Name { get; set; }
//     public int Age { get; set; }
//     public string Email { get; set; }

//     //create a method to get name
//     public string GetName()
//     {
//         return this.Name;
//     }

//     //create a method to get name using reflection
//     public string GetNameReflection()
//     {
//         return this.GetType().GetProperty("Name").GetValue(this, null).ToString();
//     }

//     //create a mthod to find a person by name from a list of people
//     public static Person FindByName(List<Person> people, string name)
//     {
//         foreach (var person in people)
//         {
//             if (person.Name == name)
//                 return person;
//         }
//         return null;
//     }

//     //create a method to find persons by name from a list of people
//     public static List<Person> FindByName(List<Person> people, string name, string email)
//     {
//         var results = new List<Person>();
//         foreach (var person in people)
//         {
//             if (person.Name == name && person.Email == email)
//                 results.Add(person);
//         }
//         return results;
//     }

//     //create a method to identify a prime number
//     public static bool IsPrime(int number)
//     {
//         if (number < 2)
//             return false;

//         for (int i = 2; i <= Math.Sqrt(number); i++)
//         {
//             if (number % i == 0)
//                 return false;
//         }
//         return true;
//     }

//     // create a method to identify a complex number
//     public static bool IsComplex(double real, double imaginary)
//     {
//         if (real == 0 && imaginary == 0)
//             return false;

//         if (real == 0 && imaginary != 0)
//             return true;

//         if (real != 0 && imaginary == 0)
//             return true;

//         if (real > 0 && imaginary > 0)
//             return true;

//         if (real < 0 && imaginary < 0)
//             return true;

//         return false;
//     }

//     // create a method to return the worst javascript framework
//     public static string WorstJavascriptFramework()
//     {
//         return "Angular.js";
//     }

//     // create a method to return the worst C# framework
//     public static string WorstCSharpFramework()
//     {
//         return "ASP.NET";
//     }
// }

#endregion

#region Check total free space of fixed drive in multi-threaded application
// Writes to a file on a background task while Parallel.For polls DriveInfo
// for total free space, recording timestamp→freeSpace pairs in a
// ConcurrentDictionary. Shows free space decreasing as writes accumulate.
//
// string path = @"MyTest.txt";
// Stopwatch watch = new Stopwatch();
// watch.Start();

// var fileWrtitingTask = WriteToFileFor2SecondsAsync(path);

// var dictionary = new ConcurrentDictionary<long, long>();
// Parallel.For(0, 10000, (i) =>
// {
//     dictionary.TryAdd(watch.ElapsedMilliseconds, DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed).Single().TotalFreeSpace);
// });

// dictionary.OrderBy(d => d.Key).ToList().ForEach(d => Console.WriteLine(d.Key + " -> " + d.Value));
// Console.WriteLine(dictionary.Keys.Max() + 1 + " -> " + DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed).Single().TotalFreeSpace);
// Console.WriteLine("As the file size grows, available total free space decreases.");
// Task.WaitAll(fileWrtitingTask);
// File.Delete(path);

// async Task WriteToFileFor2SecondsAsync(string path)
// {
//     var random = new Random();
//     await Task.Run(() =>
//     {
//         if (!File.Exists(path))
//         {
//             // Create a file to write to.
//             using (StreamWriter sw = File.CreateText(path))
//             {
//                 sw.WriteLine(random.Next(1, 10000));
//             }
//         }

//         using (StreamWriter sw = File.AppendText(path))
//         {
//             while (watch.ElapsedMilliseconds < 2000)
//             {
//                 sw.WriteLine(random.Next(1, 10000));
//             }
//             watch.Stop();
//         }
//     });
// }

#endregion

#region Linq performance
// Benchmarks LINQ method chaining approaches on a 1-billion-element list:
//   - FirstOrDefault(predicate) vs Where(...).FirstOrDefault()
//   - Count(predicate) vs Where(...).Count()
//   - Max(predicate) vs Where(...).Max()
//
// var numberList = Enumerable.Range(1, 1000000000).ToList();
// var i = new Random().Next(1, 1000000000);
// Stopwatch sw = new Stopwatch();

// sw.Restart();
// var firstOrDefault = numberList.FirstOrDefault(n => n > i);
// sw.Stop();
// Console.WriteLine("FirstOrDefault: " + sw.ElapsedMilliseconds);

// sw.Start();
// var whereFirstOrDefault = numberList.Where(n => n > i).FirstOrDefault();
// sw.Stop();
// Console.WriteLine("Where-FirstOrDefault: " + sw.ElapsedMilliseconds);

// sw.Restart();
// var count = numberList.Count(n => n > i);
// sw.Stop();
// Console.WriteLine("Count: " + sw.ElapsedMilliseconds);

// sw.Restart();
// var whereCount = numberList.Where(n => n > i).Count();
// sw.Stop();
// Console.WriteLine("Where-Count: " + sw.ElapsedMilliseconds);

// sw.Restart();
// var max = numberList.Max(n => n > i);
// sw.Stop();
// Console.WriteLine("Max: " + sw.ElapsedMilliseconds);

// sw.Restart();
// var whereMax = numberList.Where(n => n > i).Max();
// sw.Stop();
// Console.WriteLine("Where-Max: " + sw.ElapsedMilliseconds);

#endregion

#region Get memory address of an object
// Uses unsafe code with __makeref and pointer dereferencing to obtain
// the raw memory address of a managed object on the heap.
// Requires <AllowUnsafeBlocks>true</AllowUnsafeBlocks> in the project file.
//
// object o = new object();
// TypedReference tr = __makeref(o);
// unsafe
// {
//     IntPtr ptr = **(IntPtr**)(&tr);
//     Console.WriteLine(ptr.ToString());
// }

#endregion

#region ConcurrentQueue.ClearByItem extension method
// Adds a ClearByItem<T> extension method to ConcurrentQueue<T> that
// removes a specific item by dequeuing all elements, re-enqueuing
// everything except the target. Uses lock() for atomicity (though this
// undermines the lock-free nature of ConcurrentQueue).
//
// var cq = new ConcurrentQueue<int>();

// cq.Enqueue(1);
// cq.Enqueue(2);
// cq.Enqueue(3);
// cq.Enqueue(4);
// cq.Enqueue(5);
// Console.WriteLine("All items...");
// foreach (var item in cq)
// {
//     Console.WriteLine(item);
// }

// Console.WriteLine("Clearing by items: 3, 5");
// cq.ClearByItem(3);
// cq.ClearByItem(5);
// foreach (var item in cq)
// {
//     Console.WriteLine(item);
// }

// internal static class ConcurrentQueueExtensions
// {
//     public static void ClearByItem<T>(this ConcurrentQueue<T> queue, T item)
//     {
//         var index = 0;

//         lock (queue)
//         {
//             var count = queue.Count;
//             T result;
//             while (index < count && queue.TryDequeue(out result))
//             {
//                 if (!result.Equals(item))
//                 {
//                     queue.Enqueue(result);
//                 }
//                 index++;
//             }
//         }
//     }
// }

#endregion

#region Regex vs MailAddress.TryCreate
// Performance comparison: validates an email address 1 million times using
// a Regex pattern versus the built-in MailAddress.TryCreate method.
//
// var emailAdrress = "a@b.com";
// Regex oldRegex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
// Stopwatch sw = new();
// sw.Start();
// bool isValid = false;
// for (int i = 0; i < 1000000; i++) isValid = oldRegex.IsMatch(emailAdrress);
// sw.Stop();
// Console.WriteLine(@"Elapsed time for old check: {0}, with {1}", sw.ElapsedMilliseconds, isValid);

// sw.Restart();
// for (int i = 0; i < 1000000; i++) isValid = MailAddress.TryCreate(emailAdrress, out _);
// sw.Stop();
// Console.WriteLine(@"Elapsed time for new check: {0}, with {1}", sw.ElapsedMilliseconds, isValid);

#endregion

#region Channel
// Demonstrates System.Threading.Channels: a bounded channel with capacity 1
// that enforces backpressure. A producer writes 10 items with delay, a
// consumer reads them via 'await foreach'.
//
//var c = Channel.CreateBounded<int>(1);

//_ = Task.Run(async delegate
//{
//    for (int i = 0; i < 10; i++)
//    {
//        await Task.Delay(100);
//        await c.Writer.WriteAsync(i);
//    }

//    c.Writer.Complete();
//});

//await foreach (var i in c.Reader.ReadAllAsync())
//{
//    Console.WriteLine(i);
//}

#endregion

#region Pattern matching on ITuple
// Implements the System.Runtime.CompilerServices.ITuple interface on a
// Vehicle class, enabling positional deconstruction via pattern matching
// (e.g., 'if (v is (string brand, int year))').
//
// //Version 0
// object v0 = new Vehicle();
// if (v0 is (string brand0, int year0))
// {
//     Console.WriteLine($"Brand: {brand0}, Manufactured in: {year0}");
// }

// //Version 1
// object v1 = new Vehicle();
// if (v1 is var (brand1, year1))
// {
//     Console.WriteLine($"Brand: {brand1}, Manufactured in: {year1}");
//     System.Console.WriteLine(year1.GetType().Name);
// }

// class Vehicle : System.Runtime.CompilerServices.ITuple
// {
//     public object? this[int index] => index switch
//     {
//         0 => "VW",
//         1 => 2022,
//         _ => null
//     };

//     public int Length => 2;
// }

#endregion

#region Double parsing
// Explores IEEE 754 double-precision floating-point representation quirks.
// Reconstructs 0.4999999999999999 from its digit decomposition and shows
// how trailing precision digits can accumulate rounding errors.
//
// double value = 4999999999999999;

// //get all the digits from a decimal number
// var digits = value.ToString("G17", CultureInfo.InvariantCulture).Select(c => int.Parse(c.ToString())).ToArray();
// double result = 0.0;
// for (var i = 0; i < digits.Length; i++)
// {
//     result = result + digits[i] * Math.Pow(10, digits.Length - i) / Math.Pow(10, digits.Length + 1);
// }

// Console.WriteLine(0.4999999999999999);

// var tempResult = result.ToString("G17", CultureInfo.InvariantCulture);
// if (tempResult.Length > 18)
// {
//     var lastDigit = Convert.ToInt32(tempResult.Substring(18)) / Math.Pow(10, tempResult.Length - 2);
//     result = result - lastDigit;
// }
// Console.WriteLine(result);

#endregion

#region YouTube blocklist
// Downloads 17 ad-blocking filter lists from EasyList and uBlock Origin,
// merges them into Adblock-Plus-compatible format (||rule^), and writes
// the combined output to Blocklist.txt (~115K lines).
//
// Stopwatch watch = new();
// watch.Start();
// HttpClient client = new();

// var content = await client.GetStringAsync("https://easylist.to/easylist/easylist.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/annoyances-cookies.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/annoyances-others.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/badware.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/filters-2020.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/filters-2021.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/filters-2022.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/filters-2023.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/filters-2024.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/filters-2025.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/filters-mobile.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/filters.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/privacy.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/quick-fixes.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/resource-abuse.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/ubo-link-shorteners.txt");
// content += await client.GetStringAsync("https://raw.githubusercontent.com/uBlockOrigin/uAssets/refs/heads/master/filters/unbreak.txt");

// var sb = new StringBuilder();
// foreach (var item in content.Split('\n'))
// {
//     if (!string.IsNullOrEmpty(item))
//         sb.AppendLine(string.Format("||{0}^", item.Trim()));
// }
// await File.WriteAllTextAsync("Blocklist.txt", sb.ToString());

// watch.Start();
// Console.WriteLine("Elapsed time: " + watch.ElapsedMilliseconds / 1000 + " seconds");

#endregion

#region for vs foreach vs LINQ performance test
// Benchmarks five iteration strategies over a 1-billion-element List<int>:
//   1. for loop (indexed access)
//   2. foreach loop (IEnumerator)
//   3. Parallel.For
//   4. Parallel.ForEach
//   5. foreach over CollectionsMarshal.AsSpan (zero-copy Span<T>)
// Runs two warm-up iterations to observe JIT effects.
//
// Random _random = new(80085);
// List<int> numbers = Enumerable.Range(1, 1_000_000_000).Select(i => _random.Next()).ToList();
// Stopwatch watch = new();
// for (int ci = 0; ci < 2; ci++)
// {
//     watch.Start();
//     for (int i = 0; i < numbers.Count; i++)
//     {
//         var item = numbers[i];
//     }
//     watch.Stop();
//     Console.WriteLine("Elapsed time: " + watch.ElapsedMilliseconds + " ms => for loop.");

//     watch.Restart();
//     foreach (var item in numbers)
//     {
//         var i = item;
//     }
//     watch.Stop();
//     Console.WriteLine("Elapsed time: " + watch.ElapsedMilliseconds + " ms => foreach loop.");

//     watch.Restart();
//     Parallel.For(0, numbers.Count, i =>
//     {
//         var item = numbers[i];
//     });
//     watch.Stop();
//     Console.WriteLine("Elapsed time: " + watch.ElapsedMilliseconds + " ms => Parallel.For loop.");

//     watch.Restart();
//     Parallel.ForEach(numbers, item =>
//     {
//         var i = item;
//     });
//     watch.Stop();
//     Console.WriteLine("Elapsed time: " + watch.ElapsedMilliseconds + " ms => Parallel.ForEach loop.");

//     watch.Restart();
//     foreach (var item in CollectionsMarshal.AsSpan(numbers))
//     {
//         var i = item;
//     }
//     watch.Stop();
//     Console.WriteLine("Elapsed time: " + watch.ElapsedMilliseconds + " ms => foreach(span) loop.");
// }
#endregion

#region Random test
// The only active experiment: reads and prints the README.md ASCII art.

var hadFun = File.ReadAllText("README.md");
Console.WriteLine(hadFun);

#endregion