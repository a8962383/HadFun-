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

#region Proxy server
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

#region Game of thread
// int[] array = Enumerable.Range(0, short.MaxValue).ToArray();

// Console.WriteLine(array.Sum());
// Console.WriteLine(array.AsParallel().Sum());

// int m = 500000;
// var s1 = Stopwatch.StartNew();
// for (int i = 0; i < m; i++) array.Sum();
// s1.Stop();

// var s2 = Stopwatch.StartNew();
// for (int i = 0; i < m; i++) array.AsParallel().Sum();
// s2.Stop();

// Console.WriteLine(((double)(s1.Elapsed.TotalMilliseconds * 1000000) / m).ToString("0.00 ns"));
// Console.WriteLine(((double)(s2.Elapsed.TotalMilliseconds * 1000000) / m).ToString("0.00 ns"));
#endregion

#region Json Validation
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

#region Current experiment

Console.WriteLine("Goodbye world!!!");

#endregion

Console.ReadKey();