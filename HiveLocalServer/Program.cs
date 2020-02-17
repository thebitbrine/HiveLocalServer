using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TheBitBrine;

namespace HiveLocalServer
{
    class Program
    {
        static void Main()
        {
            new Program().Run();
            Console.ReadKey();
        }
        private QuickMan API;
        public void Run()
        {
            API = new QuickMan();
            var Endpoints = new Dictionary<string, Action<HttpListenerContext>>
            {
                { "worker/api", HiveAPI }
            };
            MacMapper();
            API.Start(80, Endpoints, 200);
            new Thread(DisplayStats) { IsBackground = true }.Start();
        }

        public string FindMac(string Input)
        {
            string Mac = "";
            for (int i = 0; i < Input.Length; i++)
            {
                if (Input.Length - i > 16)
                    if (char.IsLetterOrDigit(Input[i]) && char.IsLetterOrDigit(Input[i + 1]) && Input[i + 2] == ':' || Input[i + 2] == '-')
                        if (char.IsLetterOrDigit(Input[i + 3]) && char.IsLetterOrDigit(Input[i + 4]) && Input[i + 5] == ':' || Input[i + 5] == '-')
                            if (char.IsLetterOrDigit(Input[i + 6]) && char.IsLetterOrDigit(Input[i + 7]) && Input[i + 8] == ':' || Input[i + 8] == '-')
                                if (char.IsLetterOrDigit(Input[i + 9]) && char.IsLetterOrDigit(Input[i + 10]) && Input[i + 11] == ':' || Input[i + 11] == '-')
                                    if (char.IsLetterOrDigit(Input[i + 12]) && char.IsLetterOrDigit(Input[i + 13]) && Input[i + 14] == ':' || Input[i + 14] == '-')
                                        if (char.IsLetterOrDigit(Input[i + 15]) && char.IsLetterOrDigit(Input[i + 16]))
                                            Mac = Input.Substring(i, 17);

            }
            return Mac;
        }


        List<string> AvailableDevices = new List<string>();
        public bool Display = false;
        public Stopwatch Time = new Stopwatch();
        public void DisplayStats()
        {
            while (true)
            {
                if (!Time.IsRunning)
                    Time.Start();
                if (Time.ElapsedMilliseconds > 120000)
                {
                    Console.Clear();
                    AvailableDevices.Clear();
                    //Display = !Display;
                    Time.Reset();
                }
                if (!Display)
                {
                    try
                    {
                        Console.Clear();
                        Console.WriteLine("Available Devices:");
                        AvailableDevices.Sort();
                        foreach (var Device in AvailableDevices.ToArray())
                        {
                            if (Device.Contains("Hello"))
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            else
                                if (Device.Contains("*"))
                                Console.ForegroundColor = ConsoleColor.Red;
                            else
                                Console.ForegroundColor = ConsoleColor.Gray;

                            Console.WriteLine(Device.Replace("*", ""));
                        }
                    }
                    catch { }
                }
                Thread.Sleep(500);
            }
        }

        public static string WalletText = File.ReadAllText("Rigs\\wallet.map");
        public static List<WalletObject> WalletList = JsonConvert.DeserializeObject<List<WalletObject>>(WalletText);
        public List<MacMap> MacMapList = new List<MacMap>();
        public void HiveAPI(HttpListenerContext Context)
        {


            string id_rig = Context.Request.QueryString["id_rig"];
            string methtod = Context.Request.QueryString["method"];
            if (!string.IsNullOrWhiteSpace(methtod))
            {
                List<KeyValuePair<string, string>> PostBody;
                string RawPostBody = "";
                string Mac = "";

                using (var reader = new StreamReader(Context.Request.InputStream, Context.Request.ContentEncoding))
                {
                    RawPostBody = reader.ReadToEnd();
                    PostBody = LazyJson(RawPostBody);

                }

                Mac = FindMac(RawPostBody);

                if (methtod.ToLower() == "hello")
                {
                    string newValue1 = "";
                    string newValue2 = "";

                    WalletObject walletObject1 = WalletList.FirstOrDefault(x => id_rig.ToLower().Contains(x.Worker.ToLower()));
                    if (walletObject1 != null)
                    {
                        newValue1 = walletObject1.Wallet;
                        newValue2 = walletObject1.Pool;
                    }
                    WalletObject walletObject2 = WalletList.FirstOrDefault(x => x.Worker.ToLower() == id_rig.ToLower());
                    if (walletObject2 != null)
                    {
                        newValue1 = walletObject2.Wallet;
                        newValue2 = walletObject2.Pool;
                    }

                    string worker = MacMapList.Where(x => x.Mac.ToUpper() == Mac.ToUpper()).First().Worker;
                    id_rig = string.IsNullOrWhiteSpace(worker) ? Mac.Replace(":", "-") : worker;

                    if (!Directory.Exists($"Rigs\\{id_rig}"))
                        Directory.CreateDirectory($"Rigs\\{id_rig}");

                    if (!File.Exists($"Rigs\\{id_rig}\\rig.conf"))
                    {
                        File.Copy($"Rigs\\+Default+\\rig.conf", $"Rigs\\{id_rig}\\rig.conf");
                        File.Copy($"Rigs\\+Default+\\wallet.conf", $"Rigs\\{id_rig}\\wallet.conf");
                        File.Copy($"Rigs\\+Default+\\amd-oc.conf", $"Rigs\\{id_rig}\\amd-oc.conf");
                        File.Copy($"Rigs\\+Default+\\nvidia-oc.conf", $"Rigs\\{id_rig}\\nvidia-oc.conf");
                        File.Copy($"Rigs\\+Default+\\octofan.conf", $"Rigs\\{id_rig}\\octofan.conf");
                        File.Copy($"Rigs\\+Default+\\autofan.conf", $"Rigs\\{id_rig}\\autofan.conf");
                        Console.WriteLine($"Setting default settings for ID: {id_rig}, IP: {Context.Request.RemoteEndPoint.Port}");

                        File.WriteAllText($"Rigs\\{id_rig}\\rig.conf", File.ReadAllText($"Rigs\\{id_rig}\\rig.conf").Replace("WORKER_NAME=hellothere", $"WORKER_NAME={id_rig}"));
                        File.WriteAllText($"Rigs\\{id_rig}\\rig.conf", File.ReadAllText($"Rigs\\{id_rig}\\rig.conf").Replace("RIG_ID=1", $"RIG_ID={id_rig}"));
                        File.WriteAllText($"Rigs\\{id_rig}\\rig.conf", File.ReadAllText($"Rigs\\{id_rig}\\rig.conf").Replace("%localhost%", $"http://{Context.Request.UserHostAddress}"));
                        File.WriteAllText("Rigs\\" + id_rig + "\\wallet.conf", File.ReadAllText("Rigs\\" + id_rig + "\\wallet.conf").Replace("%WALLET%", newValue1));
                        File.WriteAllText("Rigs\\" + id_rig + "\\wallet.conf", File.ReadAllText("Rigs\\" + id_rig + "\\wallet.conf").Replace("%POOL%", newValue2));
                    }


                    string Response = File.ReadAllText("Rigs\\hello.response");
                    string Config = Format(File.ReadAllText($"Rigs\\{id_rig}\\rig.conf").Replace("%localhost%", $"http://{Context.Request.UserHostAddress}"));
                    string RigName = id_rig;
                    string Wallet = Format(File.ReadAllText($"Rigs\\{id_rig}\\wallet.conf").Replace("%WORKER_NAME%", id_rig).Replace("%WALLET%", newValue1).Replace("%POOL%", newValue2));
                    string AMD_OC = Format(File.ReadAllText($"Rigs\\{id_rig}\\amd-oc.conf"));
                    string Nvidia_OC = Format(File.ReadAllText($"Rigs\\{id_rig}\\nvidia-oc.conf"));
                    string Octofan = Format(File.ReadAllText($"Rigs\\{id_rig}\\octofan.conf"));
                    string Autofan = Format(File.ReadAllText($"Rigs\\{id_rig}\\autofan.conf"));

                    Response = Response.Replace("%name%", RigName).Replace("%config%", Config).Replace("%wallet%", Wallet).Replace("%amd-oc%", AMD_OC).Replace("%nvidia-oc%", Nvidia_OC).Replace("%octofan%", Octofan).Replace("%autofan%", Autofan);
                    API.Respond(Response, Context);

                    if (Display)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Said hello to \"{RigName.ToUpper()}\" ({PostBody.Find(x => x.Key == "kernel").Value})");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    if (!AvailableDevices.Contains($"{id_rig.ToUpper()},\tHello,\t{Context.Request.RemoteEndPoint.Address}"))
                        AvailableDevices.Add($"{id_rig.ToUpper()},\tHello,\t{Context.Request.RemoteEndPoint.Address}");

                }

                if (methtod.ToLower() == "stats")
                {
                    API.Respond("{\"result\":{\"command\":\"OK\"}}", Context);

                    if (!AvailableDevices.Contains($"{id_rig.ToUpper()},\tOk,\t{Context.Request.RemoteEndPoint.Address}"))
                        AvailableDevices.Add($"{id_rig.ToUpper()},\tOk,\t{Context.Request.RemoteEndPoint.Address}");
                }

                if (methtod.ToLower() == "message")
                {
                    string MessageType = PostBody.Where(x => x.Key.Contains("type")).FirstOrDefault().Value.Replace(" ", "");
                    string Data = FormatJson(RawPostBody).Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
                    Log(id_rig, MessageType, Data);
                    AvailableDevices.Add($"{id_rig.ToUpper()},\t{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MessageType.ToLower())}*,\t{Context.Request.RemoteEndPoint.Address}");
                    API.Respond("{\"result\":{\"command\":\"OK\"}}", Context);
                }
            }
        }

        public void MacMapper()
        {
            foreach (MacMap macMap in (List<MacMap>)JsonConvert.DeserializeObject<List<MacMap>>(File.ReadAllText("Rigs\\mac.map")))
            {
                macMap.Mac = macMap.Mac.TrimStart(' ');
                macMap.Mac = macMap.Mac.TrimEnd(' ');
                macMap.Mac = macMap.Mac.Replace("-", ":");
                macMap.Worker = macMap.Worker.TrimStart(' ');
                macMap.Worker = macMap.Worker.TrimEnd(' ');
                MacMapList.Add(macMap);
            }
        }

        public class MacMap
        {
            public string Worker;
            public string Mac;
        }

        public class WalletObject
        {
            public string Worker { get; set; }
            public string Wallet { get; set; }
            public string Pool { get; set; }
        }

        public void Log(string id_rig, string type, string data)
        {
            string Filename = $"{DateTime.Now.Ticks}_{type.ToUpper()}_{id_rig}.json";
            if (Touch($"Rigs\\{id_rig}\\Logs\\{Filename}"))
            {
                File.WriteAllText($"Rigs\\{id_rig}\\Logs\\{Filename}", data);
            }
        }

        public string FormatJson(string json)
        {
            const string indentString = " ";
            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, --indentation)) + ch : ch.ToString()
                select lineBreak ?? (openChar.Length > 1 ? openChar : closeChar);

            return string.Concat(result);
        }

        public string Format(string Text)
        {
            return Text.Replace("\r", "").Replace("\n", "\\n").Replace("\"", "\\\"");
        }

        public string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (!string.IsNullOrWhiteSpace(strSource) && strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return null;
            }
        }

        public List<KeyValuePair<string, string>> LazyJson(string Raw)
        {
            var BBQ = new List<KeyValuePair<string, string>>();
            char[] Chars = new char[] { '[', ']', '{', '}', '\"' };
            foreach (string MediumRare in Raw.Split(','))
            {
                string Cooked = MediumRare.Replace(":[", "\n");
                foreach (var Char in Chars) Cooked = Cooked.Replace(Char.ToString(), "");
                if (Cooked.Contains(':') && !Cooked.Contains("\n")) BBQ.Add(new KeyValuePair<string, string>(Cooked.Split(':')[0], Cooked.Split(':')[1]));
            }
            return BBQ;
        }

        public bool Touch(string Path)
        {
            try
            {
                StringBuilder PathCheck = new StringBuilder();
                string[] Direcories = Path.Split(System.IO.Path.DirectorySeparatorChar);
                foreach (var Directory in Direcories)
                {
                    PathCheck.Append(Directory);
                    string InnerPath = PathCheck.ToString();
                    if (System.IO.Path.HasExtension(InnerPath) == false)
                    {
                        PathCheck.Append("\\");
                        if (System.IO.Directory.Exists(InnerPath) == false) System.IO.Directory.CreateDirectory(InnerPath);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(InnerPath, "");
                    }
                }
                if (IsDirectory(Path) == true && System.IO.Directory.Exists(PathCheck.ToString()) == true) { return true; }
                if (IsDirectory(Path) == false && System.IO.File.Exists(PathCheck.ToString()) == true) { return true; }
            }
            catch { }
            return false;
        }
        public bool IsDirectory(string Path)
        {
            try
            {
                System.IO.FileAttributes attr = System.IO.File.GetAttributes(Path);
                if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                    return true;
                else
                    return false;
            }
            catch
            {
                if (System.IO.Path.HasExtension(Path) == true) return true;
                else return false;
            }
        }


    }
}
