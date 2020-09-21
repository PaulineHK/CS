using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Xml;

namespace lab9
{
    class Program
    {
        static void print(XmlNode node)
        {
            if (node.Name == "title")
            {
                Console.WriteLine("");
                Console.WriteLine(node.InnerText + "\n");
            }
            else if (node.Name == "description")
            {
                string temp = node.InnerText;
                if (temp.IndexOf('<') != -1)
                {
                    temp = temp.Substring(temp.IndexOf('>') + 1);
                    temp = temp.Remove(temp.IndexOf('<'));
                }
                Console.WriteLine(temp);
            }
            else if (node.Name == "pubDate")
            {
                Console.WriteLine("\nДата публикации: " + node.InnerText);
                Console.WriteLine("\n");
            }
        }
        static void Main(string[] args)
        {
            bool yes = false;
            string data = "";
            Console.WriteLine("Введите адрес RSS-ленты(news.tut.by/rss/all.rss):");
            string path = Console.ReadLine();
            string host = path.Remove(path.IndexOf('/'));
            string update = "";
            do
            {
                TcpClient client = new TcpClient();
                try
                {
                    client.Connect("www." + host, 80);
                    byte[] sendBytes = Encoding.UTF8.GetBytes("GET " + path.Substring(path.IndexOf('/')) + " HTTP/1.0\r\nHost:" + host + "\r\n\r\n");
                    NetworkStream tcpStream = client.GetStream();
                    tcpStream.Write(sendBytes, 0, sendBytes.Length);
                    NetworkStream ns = client.GetStream();
                    StreamReader sr = new StreamReader(ns);

                    while (!data.Contains("rss version"))
                        data = sr.ReadLine();

                    data += "\r\n";
                    data += sr.ReadToEnd();

                    if (update == "")
                        update = data.Substring(data.IndexOf("<lastBuildDate>") + 15, 31);
                    else if (data.Contains(update))
                    {
                        Console.Write("Обновлений нет");
                        client.Close();
                        break;
                    }

                    client.Close();
                }
                catch (SocketException e)
                {
                    Console.WriteLine("E: " + e.ToString());
                    client.Close();

                    break;
                }

                using (FileStream fstream = new FileStream("news.xml", FileMode.Create))
                {

                    byte[] array = Encoding.UTF8.GetBytes(data);
                    fstream.Write(array, 0, array.Length);
                    // Console.WriteLine("Новости получены");
                }

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load("news.xml");
                XmlElement xRoot = xDoc.DocumentElement;
                XmlNode channel = xRoot.FirstChild;

                foreach (XmlNode xnode in channel)
                {
                    if (xnode.Name == "lastBuildDate")
                        update = xnode.InnerText;

                    if (xnode.Name == "item")
                    {
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            print(childnode);
                        }

                    }
                    else print(xnode);
                }

                Console.WriteLine("1 - Проверить обновления\n0 - Выйти");
                int number = 2;
                while (number != 1 && number != 0)
                    Int32.TryParse(Console.ReadLine(), out number);
                if (number == 1) yes = true;
                else yes = false;

            } while (yes);

            Console.ReadKey();
        }
    }
}
