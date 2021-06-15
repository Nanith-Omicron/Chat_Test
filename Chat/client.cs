using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Chat
{
    /// <summary>
    /// Occurs when a command received from a client.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">The received command object.</param>
    public delegate void CommandReceivedEventHandler(object sender, CommandEventArgs e);

    /// <summary>
    /// Occurs when a command had been sent to the remote client successfully.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">EventArgs.</param>
    public delegate void CommandSentEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Occurs when a command sending action had been failed.This is because disconnection or sending exception.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">EventArgs.</param>
    public delegate void CommandSendingFailedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// The class that contains information about received command.
    /// </summary>
    public class CommandEventArgs : EventArgs
    {
        private Command command;
        /// <summary>
        /// The received command.
        /// </summary>
        public Command Command
        {
            get { return command; }
        }

        /// <summary>
        /// Creates an instance of CommandEventArgs class.
        /// </summary>
        /// <param name="cmd">The received command.</param>
        public CommandEventArgs(Command cmd)
        {
            this.command = cmd;
        }
    }
    /// <summary>
    /// Occurs when a remote client had been disconnected from the server.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">The client information.</param>
    public delegate void DisconnectedEventHandler(object sender, ClientEventArgs e);
    /// <summary>
    /// Client event args.
    /// </summary>
    public class ClientEventArgs : EventArgs
    {
        private Socket socket;
        /// <summary>
        /// The ip address of remote client.
        /// </summary>
        public IPAddress IP
        {
            get { return ((IPEndPoint)this.socket.RemoteEndPoint).Address; }
        }
        /// <summary>
        /// The port of remote client.
        /// </summary>
        public int Port
        {
            get { return ((IPEndPoint)this.socket.RemoteEndPoint).Port; }
        }
        /// <summary>
        /// Creates an instance of ClientEventArgs class.
        /// </summary>
        /// <param name="clientManagerSocket">The socket of server side socket that comunicates with the remote client.</param>
        public ClientEventArgs(Socket clientManagerSocket)
        {
            this.socket = clientManagerSocket;
        }
    }
    public class Command
    {
        private CommandType cmdType;
        /// <summary>
        /// The type of command to send.If you wanna use the Message command type,create a Message class instead of command.
        /// </summary>
        public CommandType CommandType
        {
            get { return cmdType; }
            set { cmdType = value; }
        }

        private IPAddress senderIP;
        /// <summary>
        /// [Gets/Sets] The IP address of command sender.
        /// </summary>
        public IPAddress SenderIP
        {
            get { return senderIP; }
            set { senderIP = value; }
        }

        private string senderName;
        /// <summary>
        /// [Gets/Sets] The name of command sender.
        /// </summary>
        public string SenderName
        {
            get { return senderName; }
            set { senderName = value; }
        }

        private IPAddress target;
        /// <summary>
        /// [Gets/Sets] The targer machine that will receive the command.Set this property to IPAddress.Broadcast if you want send the command to all connected clients.
        /// </summary>
        public IPAddress Target
        {
            get { return target; }
            set { target = value; }
        }
        private string commandBody;
        /// <summary>
        /// The body of the command.This string is different in various commands.
        /// <para>Message : The text of the message.</para>
        /// <para>ClientLoginInform,SendClientList : "RemoteClientIP:RemoteClientName".</para>
        /// <para>***WithTimer : The interval of timer in miliseconds..The default value is 60000 equal to 1 min.</para>
        /// <para>IsNameExists : 'True' or 'False'</para>
        /// <para>Otherwise pass the "" or null.</para>
        /// </summary>
        public string MetaData
        {
            get { return commandBody; }
            set { commandBody = value; }
        }
        /// <summary>
        /// Creates an instance of command object to send over the network.
        /// </summary>
        /// <param name="type">The type of command.If you wanna use the Message command type,create a Message class instead of command.</param>
        /// <param name="targetMachine">The targer machine that will receive the command.Set this property to IPAddress.Broadcast if you want send the command to all connected clients.</param>
        /// <param name="metaData">
        /// The body of the command.This string is different in various commands.
        /// <para>Message : The text of the message.</para>
        /// <para>ClientLoginInform,SendClientList : "RemoteClientIP:RemoteClientName".</para>
        /// <para>***WithTimer : The interval of timer in miliseconds..The default value is 60000 equal to 1 min.</para>
        /// <para>IsNameExists : 'True' or 'False'</para>
        /// <para>Otherwise pass the "" or null or use the next overriden constructor.</para>
        /// </param>
        public Command(CommandType type, IPAddress targetMachine, string metaData)
        {
            this.cmdType = type;
            this.target = targetMachine;
            this.commandBody = metaData;
        }

        /// <summary>
        /// Creates an instance of command object to send over the network.
        /// </summary>
        /// <param name="type">The type of command.If you wanna use the Message command type,create a Message class instead of command.</param>
        /// <param name="targetMachine">The targer machine that will receive the command.Set this property to IPAddress.Broadcast if you want send the command to all connected clients.</param>
        public Command(CommandType type, IPAddress targetMachine)
        {
            this.cmdType = type;
            this.target = targetMachine;
            this.commandBody = "";
        }
    }
    public enum CommandType
    {
        /// <summary>
        /// Force the target to logoff from the application without prompt.Pass null or "" as command's Metadata.
        /// </summary>
        UserExit,
        /// <summary>
        /// Force the target to logoff from the application with prompt.Pass the timer interval of logoff action as command's Metadata in miliseconds.For example "20000".
        /// </summary>
        UserExitWithTimer,
        /// <summary>
        /// Force the target PC to LOCK without prompt.Pass null or "" as command's Metadata.
        /// </summary>
        PCLock,
        /// <summary>
        /// Force the target PC to LOCK with prompt.Pass the timer interval of LOCK action as command's Metadata in miliseconds.For example "20000".
        /// </summary>
        PCLockWithTimer,
        /// <summary>
        /// Force the target PC to RESTART without prompt.Pass null or "" as command's Metadata.
        /// </summary>
        PCRestart,
        /// <summary>
        /// Force the target PC to RESTART with prompt.Pass the timer interval of RESTART action as command's Metadata in miliseconds.For example "20000".
        /// </summary>
        PCRestartWithTimer,
        /// <summary>
        /// Force the target PC to LOGOFF without prompt.Pass null or "" as command's Metadata.
        /// </summary>
        PCLogOFF,
        /// <summary>
        /// Force the target PC to LOGOFF with prompt.Pass the timer interval of LOGOFF action as command's Metadata in miliseconds.For example "20000".
        /// </summary>
        PCLogOFFWithTimer,
        /// <summary>
        /// Force the target PC to SHUTDOWN without prompt.Pass null or "" as command's Metadata.
        /// </summary>
        PCShutDown,
        /// <summary>
        /// Force the target PC to SHUTDOWN with prompt.Pass the timer interval of SHUTDOWN action as command's Metadata in miliseconds.For example "20000".
        /// </summary>
        PCShutDownWithTimer,
        /// <summary>
        /// Send a text message to the server.Pass the body of text message as command's Metadata.
        /// </summary>
        Message,
        /// <summary>
        /// This command will sent to all clients when an specific client is had been logged in to the server.The metadata of this command is in this format : "RemoteClientIP:RemoteClientName"
        /// </summary>
        ClientLoginInform,
        /// <summary>
        /// This command will sent to all clients when an specific client is had been logged off from the server.
        /// </summary>
        ClientLogOffInform,
        /// <summary>
        /// This command will send to the new connected client with MetaData of 'True' or 'False' in replay to the same command that client did sent to the server as a question.
        /// </summary>
        IsNameExists,
        /// <summary>
        /// This command will send to the new connected client with MetaData in "RemoteClientIP:RemoteClientName" format in replay to the same command that client did sent to the server as a request.
        /// </summary>
        SendClientList,
        /// <summary>
        /// This is a free command that you can sent to the clients.
        /// </summary>
        FreeCommand
    }
    class client
    {
        Socket listener;

        public bool Connected { get { return this.listener != null ? this.listener.Connected : false; } }
        public IPAddress IP { get { return this.listener != null ? ((IPEndPoint)this.listener.RemoteEndPoint).Address : IPAddress.None; } }
        public int Port { get { return this.listener != null ? ((IPEndPoint)this.listener.RemoteEndPoint).Port : -1; } }

        IPAddress ipadress;
        int serverport ;
        string Name;
        public string ClientName
        {
            get { return this.Name; }
            set { this.Name = value; }
        }
        NetworkStream stream;
        private BackgroundWorker bwr;
        public client(Socket clientSocket)
        {
            this.listener = clientSocket;
            this.stream = new NetworkStream(this.listener);

            this.bwr = new BackgroundWorker();
            bwr.DoWork += new DoWorkEventHandler(StartReceive);
            bwr.RunWorkerAsync();
        }

 
        private void StartReceive(object sender, DoWorkEventArgs e)
        {

            while (Connected)
            {
                byte[] buffer = new byte[4];
                int readBytes = this.stream.Read(buffer, 0, 4);
                if (readBytes == 0)
                    break;
                CommandType cmdType = (CommandType)(BitConverter.ToInt32(buffer, 0));

                //Read the command's target size.
                string cmdTarget = "";
                buffer = new byte[4];
                readBytes = this.stream.Read(buffer, 0, 4);
                if (readBytes == 0)
                    break;
                int ipSize = BitConverter.ToInt32(buffer, 0);

                //Read the command's target.
                buffer = new byte[ipSize];
                readBytes = this.stream.Read(buffer, 0, ipSize);
                if (readBytes == 0)
                    break;
                cmdTarget = System.Text.Encoding.ASCII.GetString(buffer);


                string cmdMetaData = "";
                buffer = new byte[4];
                readBytes = this.stream.Read(buffer, 0, 4);
                if (readBytes == 0)
                    break;
                int metaDataSize = BitConverter.ToInt32(buffer, 0);

                buffer = new byte[metaDataSize];
                readBytes = this.stream.Read(buffer, 0, metaDataSize);
                if (readBytes == 0)
                    break;
                cmdMetaData = System.Text.Encoding.Unicode.GetString(buffer);

                Command cmd = new Command(cmdType, IPAddress.Parse(cmdTarget), cmdMetaData);
                cmd.SenderIP = this.IP;
                if (cmd.CommandType == CommandType.ClientLoginInform)
                    cmd.SenderName = cmd.MetaData.Split(new char[] { ':' })[1];
                else
                    cmd.SenderName = this.ClientName;
                this.OnCommandReceived(new CommandEventArgs(cmd));
            }
            this.OnDisconnected(new ClientEventArgs(this.listener));
            this.Disconnect();

        }

        private void bwSender_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null && ((bool)e.Result))
                this.OnCommandSent(new EventArgs());
            else
                this.OnCommandFailed(new EventArgs());

            ((BackgroundWorker)sender).Dispose();
            GC.Collect();
        }
        private void bwSender_DoWork(object sender, DoWorkEventArgs e)
        {
            Command cmd = (Command)e.Argument;
            e.Result = this.SendCommandToClient(cmd);
        }

        System.Threading.Semaphore semaphor = new System.Threading.Semaphore(1, 1);
        private bool SendCommandToClient(Command cmd)
        {

            try
            {
                semaphor.WaitOne();
                //Type
                byte[] buffer = new byte[4];
                buffer = BitConverter.GetBytes((int)cmd.CommandType);
                this.stream.Write(buffer, 0, 4);
                this.stream.Flush();

                //Sender IP
                byte[] senderIPBuffer = Encoding.ASCII.GetBytes(cmd.SenderIP.ToString());
                buffer = new byte[4];
                buffer = BitConverter.GetBytes(senderIPBuffer.Length);
                this.stream.Write(buffer, 0, 4);
                this.stream.Flush();
                this.stream.Write(senderIPBuffer, 0, senderIPBuffer.Length);
                this.stream.Flush();

                //Sender Name
                byte[] senderNameBuffer = Encoding.Unicode.GetBytes(cmd.SenderName.ToString());
                buffer = new byte[4];
                buffer = BitConverter.GetBytes(senderNameBuffer.Length);
                this.stream.Write(buffer, 0, 4);
                this.stream.Flush();
                this.stream.Write(senderNameBuffer, 0, senderNameBuffer.Length);
                this.stream.Flush();

                //Target
                byte[] ipBuffer = Encoding.ASCII.GetBytes(cmd.Target.ToString());
                buffer = new byte[4];
                buffer = BitConverter.GetBytes(ipBuffer.Length);
                this.stream.Write(buffer, 0, 4);
                this.stream.Flush();
                this.stream.Write(ipBuffer, 0, ipBuffer.Length);
                this.stream.Flush();

                //Meta Data.
                if (cmd.MetaData == null || cmd.MetaData == "")
                    cmd.MetaData = "\n";

                byte[] metaBuffer = Encoding.Unicode.GetBytes(cmd.MetaData);
                buffer = new byte[4];
                buffer = BitConverter.GetBytes(metaBuffer.Length);
                this.stream.Write(buffer, 0, 4);
                this.stream.Flush();
                this.stream.Write(metaBuffer, 0, metaBuffer.Length);
                this.stream.Flush();

                semaphor.Release();
                return true;
            }
            catch
            {
                semaphor.Release();
                return false;
            }
        }



        public void SendCommand(Command cmd)
        {
            if (this.listener.Connected)
            {
                BackgroundWorker bwSender = new BackgroundWorker();
                bwSender.DoWork += new DoWorkEventHandler(bwSender_DoWork);
                bwSender.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwSender_RunWorkerCompleted);
                bwSender.RunWorkerAsync(cmd);
            }
            else
                this.OnCommandFailed(new EventArgs());
        }

        public bool Disconnect()
        {
            if (this.listener.Connected)
            {
                try
                {
                    this.listener.Shutdown(SocketShutdown.Both);
                    this.listener.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
                return true;
        }


        public event CommandReceivedEventHandler CommandReceived;
        protected virtual void OnCommandReceived(CommandEventArgs e)
        {
            if (CommandReceived != null)
                CommandReceived(this, e);
        }
        public event CommandSentEventHandler CommandSent;

        protected virtual void OnCommandSent(EventArgs e)
        {
            if (CommandSent != null)
                CommandSent(this, e);
        }
        public event CommandSendingFailedEventHandler CommandFailed;
        protected virtual void OnCommandFailed(EventArgs e)
        {
            if (CommandFailed != null)
                CommandFailed(this, e);
        }
        public event DisconnectedEventHandler Disconnected;
        protected virtual void OnDisconnected(ClientEventArgs e)
        {
            if (Disconnected != null)
                Disconnected(this, e);
        }


    }
}
