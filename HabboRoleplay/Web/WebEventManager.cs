
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;
using log4net;
using Newtonsoft.Json;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Web.Outgoing.Default;
using Plus.HabboHotel.Roleplay.Web.Outgoing.Misc;
using System.Security.Cryptography.X509Certificates;
using Plus.HabboHotel.Roleplay.Web.Incoming.General;
using Plus.HabboHotel.Cache;
using System.Collections.Concurrent;
using Plus.HabboRoleplay.Misc;
using System.Threading;
using Plus.HabboRoleplay.Web.Outgoing.Statistics;
using Plus.HabboHotel.Roleplay.Web.OutGoing.Misc;
using Plus.HabboHotel.Roleplay.Web.Incoming.Interactions;
using Plus.Core;
using Plus.HabboRoleplay.Web.Util.ChatRoom;

namespace Plus.HabboHotel.Roleplay.Web
{
    /// <summary>
    /// WebSocketUser class.
    /// </summary>
    public class WebSocketUser
    {
        /// <summary>
        /// The username string.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The ID int.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The Closing bool.
        /// </summary>
        public bool Closing { get; set; }

        /// <summary>
        /// The IWebSocketConnection void.
        /// </summary>
        public IWebSocketConnection Connection { get; set; }

        /// <summary>
        /// WebSocketUser construct.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Username"></param>
        /// <param name="Connection"></param>
        public WebSocketUser(int ID, string Username, IWebSocketConnection Connection)
        {
            this.ID = ID;
            this.Username = Username;
            this.Closing = false;
            this.Connection = Connection;
        }

        /// <summary>
        /// Dispose void to dispose socket connections.
        /// </summary>
        public void Dispose()
        {
            this.ID = 0;
            this.Username = null;
            this.Closing = true;
        }
    }

    /// <summary>
    /// WebEventManager class.
    /// </summary>
    public sealed class WebEventManager
    {
        /// <summary>
        /// log4net.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Roleplayer.Web.WebEventManager");

        /// <summary>
        /// _webSocketServer.
        /// </summary>
        public WebSocketServer _webSocketServer;

        /// <summary>
        /// Concurrent dictionary containing websocket connections.
        /// </summary>
        public ConcurrentDictionary<IWebSocketConnection, WebSocketUser> _webSockets;

        /// <summary>
        /// Concurrent dictionary containing web events.
        /// </summary>
        private ConcurrentDictionary<string, IWebEvent> _webEvents;

        /// <summary>
        /// WebEventManager function.
        /// </summary>
        public WebEventManager()
        {
            string IP = PlusEnvironment.GetConfig().data["ws.tcp.bindip"];
            int Port = int.Parse(PlusEnvironment.GetConfig().data["ws.tcp.port"]);

            this._webSocketServer = new WebSocketServer("ws://" + IP + ":" + Port);
            this._webSockets = new ConcurrentDictionary<IWebSocketConnection, WebSocketUser>();
            this._webEvents = new ConcurrentDictionary<string, IWebEvent>();
            this.RegisterIncoming();
            this.RegisterOutgoing();
        }

        /// <summary>
        /// Initializes the websocket connection.
        /// </summary>
        public void Init()
        {
            this._webSocketServer.ListenerSocket.NoDelay = true;
            this._webSocketServer.Start(ConnectingSocket =>
            {
                ConnectingSocket.OnOpen = () => this.OnSocketAdd(ConnectingSocket);
                ConnectingSocket.OnClose = () => this.OnSocketRemove(ConnectingSocket);
                ConnectingSocket.OnMessage = SocketData => this.OnSocketMessage(ConnectingSocket, SocketData);
                ConnectingSocket.OnError = SocketError => this.OnSocketError(SocketError.ToString());
            });
        }

        /// <summary>
        /// Registers the incoming web events.
        /// </summary>
        public void RegisterIncoming()
        {
            this._webEvents.TryAdd("event_retrieveconnectingstatistics", new RetrieveStatsWebEvent());
            this._webEvents.TryAdd("event_pong", new PongWebEvent());
            this._webEvents.TryAdd("event_walk", new WalkWebEvent());
            this._webEvents.TryAdd("event_chatroom", new ChatRoomWebEvent());
    }

        /// <summary>
        /// Registers the outgoing web events.
        /// </summary>
        public void RegisterOutgoing()
        {
            // Misc
            this._webEvents.TryAdd("event_sendjsalert", new SendNotificationWebEvent());
            this._webEvents.TryAdd("event_htmlpage", new HtmlPageWebEvent());
            this._webEvents.TryAdd("event_updateonlinecount", new OnlineCountWebEvent());

            // Roleplay
            this._webEvents.TryAdd("event_characterbar", new RetrieveUStatsWebEvent());
            this._webEvents.TryAdd("event_atm", new ATMWebEvent());
            this._webEvents.TryAdd("event_timerdialogue", new TimerDialogueWebEvent());
            this._webEvents.TryAdd("event_captcha", new CaptchaWebEvent());
        }

        /// <summary>
        /// On socket data received
        /// </summary>
        /// <param name="InteractingSocket"></param>
        /// <param name="SentData"></param>
        private void OnSocketMessage(IWebSocketConnection InteractingSocket, string SentData)
        {
            try
            {
                var ReceivedData = JsonConvert.DeserializeObject<WebEvent>(SentData);

                if (string.IsNullOrEmpty(ReceivedData.EventName))
                    return;

                GameClient InteractingClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(ReceivedData.UserId);

                if (InteractingClient == null)
                    return;

                if (InteractingClient.LoggingOut)
                    return;

                if (!this._webSockets.ContainsKey(InteractingSocket))
                    return;
                
                IWebEvent webEvent = null;

                if (_webEvents.TryGetValue(ReceivedData.EventName, out webEvent))
                {
                    webEvent.Execute(InteractingClient, ReceivedData.ExtraData, InteractingSocket);
                    return;
                }

                log.Debug("Evento da Web não reconhecido: '" + ReceivedData.EventName + "'");

            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException)
                    return;

                this.OnSocketError(ex.Message);
            }
        }

        /// <summary>
        /// OnSocketAdd.
        /// </summary>
        /// <param name="NewUser"></param>
        public void OnSocketAdd(IWebSocketConnection ConnectingSocket)
        {
            if (ConnectingSocket == null)
                return;

            if (!ConnectingSocket.IsAvailable)
                return;

            if (ConnectingSocket.ConnectionInfo == null)
                return;

            if (string.IsNullOrEmpty(ConnectingSocket.ConnectionInfo.Path))
                return;

            if (!ConnectingSocket.ConnectionInfo.Path.Trim().Contains("/"))
                return;

            if ((string.IsNullOrEmpty(ConnectingSocket.ConnectionInfo.Path.Trim().Split('/')[1])))
                return;

            try
            {
                WebSocketUser outUser;

                int ConnectingUsersID = this.GetSocketsUserID(ConnectingSocket);
                List<GameClient> PotentialConnectedClients = this.GetSocketsClient(ConnectingSocket);

                if (PotentialConnectedClients.Count > 0)
                {
                    this.DeactivateSocket(ConnectingSocket);
                    return;
                }

                WebSocketUser User = null;
                if (this._webSockets.ContainsKey(ConnectingSocket))
                    this._webSockets.TryRemove(ConnectingSocket, out User);

                this._webSockets.TryAdd(ConnectingSocket, new WebSocketUser(ConnectingUsersID, "", ConnectingSocket));


            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException)
                    return;

                this.OnSocketError(ex.Message);
            }
        }

        /// <summary>
        /// OnSocketRemove.
        /// </summary>
        /// <param name="User"></param>
        public void OnSocketRemove(IWebSocketConnection User)
        {
            WebSocketUser outUser;

            if (!this._webSockets.ContainsKey(User))
                return;

            try
            {
                this.CloseSimilarSockets(GetSocketsUserID(User));
            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException)
                    return;

                this.OnSocketError(ex.Message);
            }
        }

        /// <summary>
        /// Closes any similar sockets with the target user ID
        /// </summary>
        /// <param name="Id"></param>
        public void CloseSimilarSockets(int Id)
        {
            List<IWebSocketConnection> SocketsToClose = GetSimilarSockets(Id);

            foreach (IWebSocketConnection Socket in SocketsToClose)
            {
                this.DeactivateSocket(Socket);
            }
        }

        public List<IWebSocketConnection> GetSimilarSockets(int Id)
        {
            List<IWebSocketConnection> SimilarSockets = new List<IWebSocketConnection>();

            foreach (KeyValuePair<IWebSocketConnection, WebSocketUser> AvailableSockets in this._webSockets)
            {
                if (AvailableSockets.Value == null)
                    continue;

                if (AvailableSockets.Key == null)
                    continue;

                if (this.GetSocketsUserID(AvailableSockets.Key) == Id)
                    SimilarSockets.Add(AvailableSockets.Key);
            }

            return SimilarSockets;
        }

        /// <summary>
        /// OnSocketError.
        /// </summary>
        /// <param name="Error"></param>
        public void OnSocketError(string Error)
        {
            Logging.LogWebSocketError(Error);
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="EventName"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public bool ExecuteWebEvent(GameClient Client, string EventName, string ReceivedData)
        {
            try
            {
                #region User client null checks

                if (ReceivedData.StartsWith("{"))
                {
                    // JSON
                }
                else
                {
                    if (!ReceivedData.Contains("bypass"))
                    {
                        if (Client == null)
                            return false;

                        if (Client.LoggingOut)
                            return false;

                        if (!this.SocketReady(Client))
                            return false;
                    }
                }

                if (string.IsNullOrEmpty(EventName))
                    return false;

                #endregion

                #region Socket null checks

                IWebSocketConnection InteractingSocket = Client.GetRoleplay().WebSocketConnection;

                if (!this.SocketReady(InteractingSocket))
                    return false;

                #endregion

                #region Execute event
                IWebEvent webEvent = null;
                if (this._webEvents.TryGetValue(EventName, out webEvent))
                {
                    if (!this._webSockets[InteractingSocket].Closing || !InteractingSocket.IsAvailable)
                        webEvent.Execute(Client, ReceivedData, InteractingSocket);
                    return true;
                }
                #endregion

            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException)
                    return false;

                this.OnSocketError(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Broadcasts the web event.
        /// </summary>
        /// <param name="EventName"></param>
        /// <param name="Data"></param>
        public void BroadCastWebEvent(string EventName, string Data)
        {
            foreach (GameClient User in PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                if (User == null)
                    continue;

                if (User.LoggingOut)
                    continue;

                this.ExecuteWebEvent(User, EventName, Data);
            }
        }

        /// <summary>
        /// Sends the given user web data.
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Data"></param>
        public void SendDataDirect(GameClient User, string Data)
        {
            if (!this.SocketReady(User, true))
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
                return;

            if (!this.SocketReady(User.GetRoleplay().WebSocketConnection))
                return;

            try
            {
                User.GetRoleplay().WebSocketConnection.Send(Data);
            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException)
                    return;

                this.OnSocketError(ex.Message);
            }
        }

        /// <summary>
        /// Broadcasts the web data.
        /// </summary>
        /// <param name="Data"></param>
        public void BroadCastWebData(string Data)
        {
            foreach (GameClient User in PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                if (!this.SocketReady(User, true))
                    continue;

                if (!this.SocketReady(User.GetRoleplay().WebSocketConnection))
                    continue;

                try
                {
                    User.GetRoleplay().WebSocketConnection.Send(Data);
                }
                catch (Exception ex)
                {
                    if (ex is System.IO.IOException)
                        return;

                    this.OnSocketError(ex.Message);
                }
            }
        }

        /// <summary>
        /// SocketReady.
        /// </summary>
        /// <param name="Socket"></param>
        /// <returns></returns>
        public bool SocketReady(IWebSocketConnection Socket)
        {
            if (Socket == null)
                return false;

            if (!this._webSockets.ContainsKey(Socket))
                return false;

            if (this._webSockets[Socket].Closing)
                return false;

            if (!Socket.IsAvailable)
                return false;


            return true;
        }

        /// <summary>
        /// Checks if the socket is ready to be interacted with
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public bool SocketReady(GameClient User, bool Logout = false)
        {
            if (User == null)
                return false;

            if (User.GetHabbo() == null)
                return false;

            if (User.GetRoleplay() == null)
                return false;

            if (!this.SocketReady(User.GetRoleplay().WebSocketConnection))
                return false;

            if (Logout)
            {
                if (User.LoggingOut)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the user connection.
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public IWebSocketConnection GetUsersConnection(GameClient User)
        {

            IWebSocketConnection UsersSocket = null;

            if (User == null)
                return null;

            if (User.GetHabbo() == null)
                return null;

            if (User.LoggingOut)
                return null;

            UsersSocket = _webSockets
                            .Where(MySockets => this.GetSocketsUserID(MySockets.Key) == User.GetHabbo().Id)
                            .Where(MySockets => SocketReady(MySockets.Key)).FirstOrDefault().Key;


            if (!this.SocketReady(UsersSocket))
                return null;

            return UsersSocket;
        }

        /// <summary>
        /// Retrieves the sockets userID from its ConnectionInformation
        /// </summary>
        /// <param name="Socket"></param>
        /// <returns></returns>
        public int GetSocketsUserID(IWebSocketConnection Socket)
        {
            if (Socket == null)
                return 0;

            if (Socket.ConnectionInfo == null)
                return 0;

            if (Socket.ConnectionInfo.Path == null)
                return 0;

            if (String.IsNullOrEmpty(Socket.ConnectionInfo.Path))
                return 0;

            return Convert.ToInt32(Socket.ConnectionInfo.Path.Trim().Split('/')[1]);
        }

        /// <summary>
        /// Gets the client assosciated with the targeted socket
        /// </summary>
        /// <param name="Socket"></param>
        /// <returns></returns>
        public List<GameClient> GetSocketsClient(IWebSocketConnection Socket)
        {

            int SocketsUserId = this.GetSocketsUserID(Socket);

            List<GameClient> RunningClients = PlusEnvironment.GetGame().GetClientManager().GetClients.ToList().
               Where(Client => Client != null).
               Where(Client => Client.LoggingOut != true).
               Where(Client => Client.GetHabbo() != null).
               Where(Client => Client.GetHabbo().Id == SocketsUserId).
               Where(Client => this.SocketReady(Client)).ToList();

            return RunningClients;
        }

        /// <summary>
        /// Gets _websockets dictionary
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<IWebSocketConnection, WebSocketUser> GetConnectedUsers()
        {
            return this._webSockets;
        }

        /// <summary>
        /// Completely shuts down a targeted Socket
        /// </summary>
        /// <param name="Socket"></param>
        public void DeactivateSocket(IWebSocketConnection Socket)
        {

            if (this._webSockets.ContainsKey(Socket))
            {
                WebSocketUser user = null;
                this._webSockets[Socket].Closing = true;
                this._webSockets[Socket].Dispose();
                this._webSockets.TryRemove(Socket, out user);
            }

            Socket.Close();
        }

        /// <summary>
        /// Closes any sockets assosciated with the character/userID 
        /// </summary>
        /// <param name="SocketUserID"></param>
        public void CloseSocketByGameClient(int SocketUserID)
        {
            if (SocketUserID == 0)
                return;

            try
            {
                this.CloseSimilarSockets(SocketUserID);
            }
            catch (Exception e)
            {

            }
        }
    }
}
