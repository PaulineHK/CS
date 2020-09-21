using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.IO;

namespace lab10
{
    class Program
    {

        private static void write(string str, NetworkStream stream)
        {
            byte[] ans = System.Text.Encoding.UTF8.GetBytes(str);
            stream.Write(ans, 0, ans.Length);
        }

        private static string read(StreamReader reader)
        {
            return reader.ReadLine();
        }

        static void Main(string[] args)
        {
            string ok = "+OK";
            string err = "-ERR";
            string rn = "\r\n";
            TcpListener server = null;
            TcpClient client = null;
            NetworkStream stream = null;
            StreamReader reader = null;
            bool quit = false;
            try
            {
                server = new TcpListener(System.Net.IPAddress.Loopback, 2020);
                server.Start();

                Console.WriteLine("Server start");
                while (!server.Pending()) { }
                Console.WriteLine("Client connected");

                client = server.AcceptTcpClient();
                stream = client.GetStream();
                reader = new StreamReader(stream);

                write("+OK POP3 server ready" + rn, stream);

                string message = "";
                string path = "./mails/";
                string[] mails = Directory.GetDirectories(path);
                List<string> command;
                string mail = "";

                //USER
                while (true)
                {
                    message = read(reader);
                    command = new List<string>(message.Split());

                    while (command.IndexOf("") != -1)//удаление пробелов
                        command.RemoveAt(command.IndexOf(""));

                    if (command[0].ToUpper() != "QUIT")
                    {
                        if (command[0].ToUpper() != "USER")
                            write(err + " Unknow command" + rn, stream);
                        else if (command.Count != 2)
                            write(err + " Invalid parameters" + rn, stream);
                        else if (!mails.Contains(path + command[1]))
                            write(err + " Unknow mail" + rn, stream);
                        else
                        {
                            mail = command[1];
                            break;
                        }
                    }
                    else
                    {
                        write(ok + rn, stream);

                        reader.Close();
                        stream.Close();
                        client.Close();
                        server.Stop();
                    }
                }
                write(ok + " Mail is found" + rn, stream);

                List<string> mail_pass = new List<string>(File.ReadAllLines("passwords.txt"));
                int i = 0;
                bool pass_ok = false;

                //PASS
                while (!pass_ok)
                {
                    message = read(reader);
                    command = new List<string>(message.Split());
                    while (command.IndexOf("") != -1)//удаление пробелов
                        command.RemoveAt(command.IndexOf(""));

                    if (command[0].ToUpper() != "QUIT")
                    {
                        if (command[0].ToUpper() != "PASS")
                            write(err + " Unknow command" + rn, stream);
                        else if (command.Count != 2)
                            write(err + " Invalid parameters" + rn, stream);
                        else
                        {
                            for (i = 0; i < mail_pass.Count; i++)
                            {
                                if (mail_pass[i].CompareTo(mail + ":" + command[1]) == 0)
                                {
                                    pass_ok = true;
                                    break;
                                }
                            }
                            if (pass_ok) break;
                            write(err + " Wrong password" + rn, stream);
                        }
                    }
                    else
                    {
                        write(ok + rn, stream);

                        reader.Close();
                        stream.Close();
                        client.Close();
                        server.Stop();
                    }
                }
                List<string> letters = new List<string>(Directory.GetFiles("./mails/" + mail));
                write(ok + " Letters: " + letters.Count + rn, stream);
                List<string> del_letters = new List<string>();
                while (!quit)
                {
                    message = read(reader);
                    command = new List<string>(message.Split());
                    while (command.IndexOf("") != -1)//удаление пробелов
                        command.RemoveAt(command.IndexOf(""));
                    switch (command[0].ToUpper())
                    {
                        case "STAT":
                            {
                                if (command.Count == 1)
                                {
                                    long sum = 0;
                                    for (i = 0; i < letters.Count; i++)
                                        sum += new FileInfo(letters[i]).Length;
                                    write(ok + " Letters: " + letters.Count + " Size: " + sum + rn, stream);
                                }
                                else
                                    write(err + " Invalid parameter" + rn, stream);
                                break;
                            }
                        case "LIST":
                            {
                                if (command.Count == 1)
                                {

                                    long sum = 0;
                                    for (i = 0; i < letters.Count; i++)
                                        sum += new FileInfo(letters[i]).Length;
                                    write(ok + " Letters: " + letters.Count + " Size: " + sum + rn, stream);
                                    for (i = 0; i < letters.Count; i++)
                                        write((i + 1) + " " + new FileInfo(letters[i]).Length + rn, stream);
                                }
                                else if (command.Count == 2)
                                {
                                    int number = 0;
                                    if (Int32.TryParse(command[1], out number) && number <= letters.Count && number > 0)
                                        write(ok + " " + command[1] + " " + new FileInfo(letters[number - 1]).Length + rn, stream);
                                    else
                                        write(err + " No such message" + rn, stream);
                                }
                                else
                                    write(err + " Invalid parameters" + rn, stream);
                                break;
                            }
                        case "RETR":
                            {
                                if (command.Count != 2)
                                    write(err + " Invalid parameter" + rn, stream);
                                else
                                {
                                    int number = 0;
                                    if (Int32.TryParse(command[1], out number) && number <= letters.Count && number > 0)
                                    {

                                        StreamReader letter = new StreamReader(letters[number - 1]);
                                        write(ok + rn + rn + letter.ReadToEnd() + "." + rn + rn, stream);
                                        letter.Close();
                                    }
                                    else write(err + " No such message" + rn, stream);

                                }
                                break;
                            }
                        case "TOP":
                            {
                                if (command.Count == 2 || command.Count == 3)
                                {
                                    int number = 0;
                                    if (Int32.TryParse(command[1], out number) && number <= letters.Count && number > 0)
                                    {
                                        write(ok + rn, stream);
                                        int lines = 0;
                                        if (command.Count == 3)
                                            if (!Int32.TryParse(command[2], out lines))
                                                write(err + " Thierd parameter is incorrect" + rn, stream);
                                        StreamReader letter = new StreamReader(letters[number - 1]);
                                        bool end = false;
                                        string str = "";
                                        while (!end)
                                        {
                                            while ((str = letter.ReadLine()) != null)
                                            {
                                                write(str + rn, stream);
                                                if (str == "")
                                                    for (i = 0; i < lines && (str = letter.ReadLine()) != null; i++)
                                                        write(str + rn, stream);
                                                else continue;
                                                write("." + rn + rn, stream);
                                                end = true;
                                                break;
                                            }
                                        }
                                        letter.Close();
                                    }
                                    else write(err + " No such message" + rn, stream);
                                }
                                else write(err + " Invalid parameters" + rn, stream);
                                break;
                            }
                        case "DELE":
                            {
                                if (command.Count != 2)
                                    write(err + " Invalid parameter" + rn, stream);
                                else
                                {
                                    int number = 0;
                                    if (Int32.TryParse(command[1], out number) && number <= letters.Count && number > 0)
                                    {
                                        del_letters.Add(letters[number - 1]);
                                        letters.RemoveAt(number - 1);
                                        write(ok + rn, stream);
                                    }
                                    else
                                        write(err + " No such message" + rn, stream);
                                }
                                break;
                            }
                        case "QUIT":
                            {
                                if (command.Count == 1)
                                {
                                    if (del_letters.Count != 0)
                                    {
                                        FileInfo file;
                                        for (i = 0; i < del_letters.Count; i++)
                                        {
                                            file = new FileInfo(del_letters[i]);
                                            file.Delete();
                                        }

                                    }
                                    write(ok + rn, stream);
                                    quit = true;
                                }
                                else write(err + " Unknow command" + rn, stream);
                                break;
                            }
                        default:
                            {
                                write(err + " Unknow command" + rn, stream);
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                Console.ReadKey();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
                if (server != null)
                    server.Stop();
            }

        }
    }
}
