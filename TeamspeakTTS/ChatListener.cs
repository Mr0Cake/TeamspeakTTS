using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TS3QueryLib.Core;
using TS3QueryLib.Core.Client;
using TS3QueryLib.Core.Client.Entities;
using TS3QueryLib.Core.Client.Responses;
using TS3QueryLib.Core.Client.Notification.EventArgs;
using TS3QueryLib.Core.Client.Notification.Enums;
using TS3QueryLib.Core.Common;
using TS3QueryLib.Core.Common.Responses;
using TS3QueryLib.Core.Communication;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace TeamspeakReader
{
    public class ChatListener
    {
        public Thread ConnectionThread;
        public ChatListener()
        {
            startThread();
            Keep_Alive_Timer.Elapsed += time_Elapsed;
            Retry_Connection_Timer.Elapsed += Reconnect;
        }

        public void startThread()
        {
            if (ConnectionThread == null || ConnectionThread.ThreadState == ThreadState.Stopped)
            {
                ConnectionThread = new Thread(Connect);
                ConnectionThread.Start();
            }
        }

        public IntPtr SkinHandle;
        public string FinishAction;
        //public static Thread ConnectionThread;
        private static System.Timers.Timer Keep_Alive_Timer = new System.Timers.Timer(30000);
        private static System.Timers.Timer Retry_Connection_Timer = new System.Timers.Timer(1000);
        public readonly object ThreadLocker = new object();
        public ConnectionState Connected = ConnectionState.Disconnected;
        public enum ConnectionState
        {
            Connecting,
            Connected,
            Disconnected
        }

        public static AsyncTcpDispatcher QueryDispatcher { get; set; }

        public static QueryRunner QueryRunner { get; set; }
        
        public TS3QueryLib.Core.Client.Responses.WhoAmIResponse currentUser { get; set; }
        public ListResponse<ChannelListEntry> channels { get; set; }
        public ListResponse<ClientListEntry> clients { get; set; }

        private Dictionary<uint, ClientListEntry> ChannelClientList { get; set; }

        public void Connect()
        {
            // do not connect when already connected or during connection establishing
            if (QueryDispatcher != null)
            {
                return;
            }
            lock (ThreadLocker)
            {
                Connected = ConnectionState.Connecting;
                QueryDispatcher = new AsyncTcpDispatcher("localhost", 25639);
                QueryDispatcher.BanDetected += QueryDispatcher_BanDetected;
                QueryDispatcher.ReadyForSendingCommands += QueryDispatcher_ReadyForSendingCommands;
                QueryDispatcher.ServerClosedConnection += QueryDispatcher_ServerClosedConnection;
                QueryDispatcher.SocketError += QueryDispatcher_SocketError;
                QueryDispatcher.NotificationReceived += QueryDispatcher_NotificationReceived;
                QueryDispatcher.Connect();
                Keep_Alive_Timer.Start();
            }
        }
        
        


        public void Disconnect()
        {
            lock (ThreadLocker)
            {
                if (QueryDispatcher != null)
                {
                    QueryDispatcher.Disconnect();
                    QueryDispatcher.DetachAllEventListeners();
                }

                //clear values
                QueryDispatcher = null;
                QueryRunner = null;
                Connected = ConnectionState.Disconnected;
                Retry_Connection_Timer.Start();
                //Thread.CurrentThread.Abort();
            }
        }
        

        
        /// <summary>
        /// Event that fires every second to check if there is a connection available
        /// If there is no connection the interval will rise with 0.5s until 8s
        /// </summary>
        private void Reconnect(object sender, ElapsedEventArgs e)
        {
            Retry_Connection_Timer.Interval = Retry_Connection_Timer.Interval < 8000 ? Retry_Connection_Timer.Interval + 500 : 8000;
            //Try again when disconnected
            if (Connected != ChatListener.ConnectionState.Connecting)
            {
                if (QueryDispatcher != null)
                {
                    QueryDispatcher.Disconnect();
                    QueryDispatcher.DetachAllEventListeners();
                }

                QueryDispatcher = null;
                QueryRunner = null;
                Connect();
            }

        }

        /// <summary>
        /// This will keep the connection to the telnet server alive
        /// Checks if you are connected to teamspeak and to a server and will reset the timer
        /// else
        /// Try reconnecting
        /// </summary>
        private void time_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Connected == ConnectionState.Connected && !(currentUser = QueryRunner.SendWhoAmI()).IsErroneous)
            {
                currentUser = QueryRunner.SendWhoAmI();
            }
            else
            {
                if (!Retry_Connection_Timer.Enabled)
                    Retry_Connection_Timer.Start();
                Keep_Alive_Timer.Stop();
            }

        }

        /// <summary>
        /// Ready for sending commands
        /// </summary>
        private void QueryDispatcher_ReadyForSendingCommands(object sender, System.EventArgs e)
        {
            if (!Keep_Alive_Timer.Enabled)
                Keep_Alive_Timer.Start();
            // you can only run commands on the queryrunner when this event has been raised first!
            try
            {
                QueryRunner = new QueryRunner(QueryDispatcher);
                //check if whoami returns a valid user, if erroneous teamspeak is not connected to a server
                //try connecting again later.
                if ((currentUser = QueryRunner.SendWhoAmI()).IsErroneous)
                {
                    Disconnect();
                    return;
                }
                Retry_Connection_Timer.Stop();
                Connected = ConnectionState.Connected;
                //QueryRunner.Notifications.ChannelTalkStatusChanged += Notifications_ChannelTalkStatusChanged;
                //QueryRunner.Notifications.ClientMoved += Notifications_ClientMoved;
                QueryRunner.Notifications.MessageReceived += Notifications_MessageReceived;
                
                QueryRunner.RegisterForNotifications(ClientNotifyRegisterEvent.Any);
                
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
        
        /// <summary>
        /// Event MessageReceived
        /// Sends TalkEvent to Text synthesiser
        /// </summary>
        private void Notifications_MessageReceived(object sender, TS3QueryLib.Core.Server.Notification.EventArgs.MessageReceivedEventArgs e)
        {
            TeamspeakReader.ChatReader.AddTextToQueue(e.InvokerNickname + " " + e.Message, e.InvokerNickname);
            
        }



        /// <summary>
        /// this event is raised when the connection to the server is lost.
        /// </summary>
        private void QueryDispatcher_ServerClosedConnection(object sender, System.EventArgs e)
        {
            Disconnect();
        }
        /// <summary>
        /// You have been banned from this server
        /// </summary>
        private void QueryDispatcher_BanDetected(object sender, EventArgs<SimpleResponse> e)
        {
            //ChannelName = "Banned";
            Disconnect();
        }

        /// <summary>
        /// This event is raised when a socket exception has occured
        /// ConnectionRefused:
        /// Teamspeak is not running
        /// Increment reconnect timer time Interval with 1 second until 15 seconds
        /// Disconnect
        /// other:
        /// Disconnect
        /// </summary>
        private void QueryDispatcher_SocketError(object sender, SocketErrorEventArgs e)
        {
            // do not handle connection lost errors because they are already handled by QueryDispatcher_ServerClosedConnection
            if (e.SocketError == SocketError.ConnectionReset)
                return;

            //if (e.SocketError == SocketError.ConnectionRefused)
                

            Disconnect();
        }

        /// <summary>
        /// This event will bind to all notifications.
        /// You can display all the events using 'e'
        /// </summary>
        private void QueryDispatcher_NotificationReceived(object sender, EventArgs<string> e)
        {

        }



    }
}
