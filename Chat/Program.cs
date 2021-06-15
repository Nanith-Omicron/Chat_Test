using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;

namespace Chat
{
    class Program
    {

        private System.Collections.Generic.List<client> clients;
        private BackgroundWorker bwListener;
        private Socket listenerSocket;
        private IPAddress serverIP;
        private int serverPort;
        static bool STOPPED = false;
        static void Main(string[] args)
        {

            Program lol = new Program();
            lol.clients = new List<client>();
            lol.serverPort = args.Length > 1 ? lol.serverPort = int.Parse(args[1]) : 8000;
            lol.serverIP = args.Length > 0 ? lol.serverIP = IPAddress.Parse(args[1]) :IPAddress.Parse("25.55.251.48");

            lol.bwListener = new BackgroundWorker();
            lol.bwListener.WorkerReportsProgress = true;
            lol.bwListener.DoWork += new DoWorkEventHandler(lol.StartToListen);
            lol.bwListener.RunWorkerAsync();

            Console.WriteLine("Omicron server's Creator 1.00! Server has being created! Listening on port " + lol.serverIP + " " + lol.serverPort + "\nPress enter to yeet away.");


            Console.ReadLine();
            STOPPED = true;
            lol.DisconnectServer();
        }

        private void StartToListen(object sender, DoWorkEventArgs e)
        {
            this.listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.listenerSocket.Bind(new IPEndPoint(this.serverIP, this.serverPort));
            this.listenerSocket.Listen(200);
          
               
            
            while (!e.Cancel)
            {
                if (STOPPED) e.Cancel = true;
                this.CreateNewClientManager(this.listenerSocket.Accept());
            }
         
            return;
        }

        private void CreateNewClientManager(Socket socket)
        {
            client newClientManager = new client(socket);
            newClientManager.CommandReceived += new CommandReceivedEventHandler(CommandReceived);
            newClientManager.Disconnected += new DisconnectedEventHandler(ClientDisconnected);
            this.CheckForAbnormalDC(newClientManager);
            this.clients.Add(newClientManager);
            this.UpdateConsole("just got involved into this mess", newClientManager.IP, newClientManager.Port);
        }
        private void CheckForAbnormalDC(client mngr)
        {
            if (this.RemoveClientManager(mngr.IP))
                this.UpdateConsole("Disconnected.", mngr.IP, mngr.Port);
        }
        void ClientDisconnected(object sender, ClientEventArgs e)
        {
            if (this.RemoveClientManager(e.IP))
                this.UpdateConsole("has being yeet away.", e.IP, e.Port);
        }
        private bool RemoveClientManager(IPAddress ip)
        {
            lock (this)
            {
                int index = this.IndexOfClient(ip);
                if (index != -1)
                {
                    string name = this.clients[index].ClientName;
                    this.clients.RemoveAt(index);

                    //Inform all clients that a client had been disconnected.
                    Command cmd = new Command(CommandType.ClientLogOffInform, IPAddress.Broadcast);
                    cmd.SenderName = name;
                    cmd.SenderIP = ip;
                    this.BroadCastCommand(cmd);
                    return true;
                }
                return false;
            }
        }
        private void CommandReceived(object sender, CommandEventArgs e)
        {
             
            //Send the command to all other remote clients.
            if (e.Command.CommandType == CommandType.ClientLoginInform)
                this.SetManagerName(e.Command.SenderIP, e.Command.MetaData);

            //If the client asked for existance of a name,answer to it's question.
            if (e.Command.CommandType == CommandType.IsNameExists)
            {
                bool isExixsts = this.IsNameExists(e.Command.SenderIP, e.Command.MetaData);
                this.SendExistanceCommand(e.Command.SenderIP, isExixsts);
                return;
            }
            //If the client asked for list of a logged in clients,replay to it's request.
            else if (e.Command.CommandType == CommandType.SendClientList)
            {
                this.SendClientListToNewClient(e.Command.SenderIP);
                return;
            }

            if (e.Command.Target.Equals(IPAddress.Broadcast))
                this.BroadCastCommand(e.Command);
            else
                this.SendCommandToTarget(e.Command);

        }

        private void SendExistanceCommand(IPAddress targetIP, bool isExists)
        {
            Command cmdExistance = new Command(CommandType.IsNameExists, targetIP, isExists.ToString());
            cmdExistance.SenderIP = this.serverIP;
            cmdExistance.SenderName = "Server";
            this.SendCommandToTarget(cmdExistance);
        }
        private void SendClientListToNewClient(IPAddress newClientIP)
        {
            foreach (client mngr in this.clients)
            {
                if (mngr.Connected && !mngr.IP.Equals(newClientIP))
                {
                    Command cmd = new Command(CommandType.SendClientList, newClientIP);
                    cmd.MetaData = mngr.IP.ToString() + ":" + mngr.ClientName;
                    cmd.SenderIP = this.serverIP;
                    cmd.SenderName = "Server";
                    this.SendCommandToTarget(cmd);
                }
            }
        }

        private string SetManagerName(IPAddress remoteClientIP, string nameString)
        {
            int index = this.IndexOfClient(remoteClientIP);
            if (index != -1)
            {
                string name = nameString.Split(new char[] { ':' })[1];
                this.clients[index].ClientName = name;
                return name;
            }
            return "";
        }

        private int IndexOfClient(IPAddress ip)
        {
            int index = -1;
            foreach (client cMngr in this.clients)
            {
                index++;
                if (cMngr.IP.Equals(ip))
                    return index;
            }
            return -1;
        }

        private bool IsNameExists(IPAddress remoteClientIP, string nameToFind)
        {
            foreach (client mngr in this.clients)
                if (mngr.ClientName == nameToFind && !mngr.IP.Equals(remoteClientIP))
                    return true;
            return false;
        }

        private void BroadCastCommand(Command cmd)
        {
            foreach (client mngr in this.clients)
                if (!mngr.IP.Equals(cmd.SenderIP))
                    mngr.SendCommand(cmd);
        }

        private void SendCommandToTarget(Command cmd)
        {
            foreach (client mngr in this.clients)
                if (mngr.IP.Equals(cmd.Target))
                {
                    mngr.SendCommand(cmd);
                    return;
                }
        }
        private void UpdateConsole(string status, IPAddress IP, int port)
        {
            Console.WriteLine("Client {0}{1}{2} has been {3} ( {4}|{5} )", IP.ToString(), ":", port.ToString(), status, DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
        }
        public void DisconnectServer()
        {
            if (this.clients != null)
            {
                foreach (client mngr in this.clients)
                    mngr.Disconnect();
                if(bwListener.WorkerSupportsCancellation)
                this.bwListener.CancelAsync();
                this.bwListener.Dispose();
                this.listenerSocket.Close();
                GC.Collect();
            }
        }

    }
}
