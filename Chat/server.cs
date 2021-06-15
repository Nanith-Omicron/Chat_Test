using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Chat
{
   public class server
    {
        List<Socket> Clients;
        Socket ns;
        public Socket GetServer { get { return ns; } }
        public server(string ipa = "127.0.0.1",int port = 8000)
        {
            var s = IPAddress.Parse(ipa);
            IPEndPoint ip = new IPEndPoint(s, port);
            ns = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ns.Bind(ip);
            ns.Listen(10);
            Console.WriteLine(" Create a server at : " + s + " port : " + port);
            Clients.Add(ns.Accept());
        }

    }
}
