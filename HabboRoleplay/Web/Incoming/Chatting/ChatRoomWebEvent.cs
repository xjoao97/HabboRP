using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Cache;
using log4net;
using Plus.HabboRoleplay.Web.Util.ChatRoom;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users.Effects;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Roleplay.Web.Incoming.General
{
    /// <summary>
    /// PongWebEvent class.
    /// </summary>
    class ChatRoomWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes ChatRoomWebEvet Socket Data
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            #region Null Checks
            if (Client.GetRoleplay().BannedFromChatting)
            {
                if (Client.GetRoleplay().WebSocketConnection != null)
                {
                    Client.GetRoleplay().SendTopAlert("Você está proibido de entrar/interagir com grupos de whatsapp");
                }
                else
                {
                    Client.SendWhisper("Você está proibido de entrar/interagir com grupos de whatsapp!", 1);
                }
            }
            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
            {
                if (Client == null)
                    return;

                if (Client.LoggingOut)
                    return;

                Client.SendWhisper("Ocorreu um erro, você não está conectado ao websocket! Entre em contato com um membro da equipe se esse problema persistir!", 1);

                return;
            }

            if (Client == null)
                return;

            if (Client.LoggingOut)
                return;

            if (string.IsNullOrEmpty(Data))
                return;

            #endregion

            #region Variables

            Dictionary<object, object> ReturnedData = JsonConvert.DeserializeObject<Dictionary<object, object>>(Data);

            string Action = null;
            string ChatName = null;

            if (ReturnedData.ContainsKey("action"))
            Action = Convert.ToString(ReturnedData["action"]);

            if (ReturnedData.ContainsKey("chatname"))
            ChatName = Convert.ToString(ReturnedData["chatname"]);

            if (String.IsNullOrEmpty(Action) || String.IsNullOrEmpty(ChatName))
                return;

            WebSocketChatRoom InteractingChatRoom = WebSocketChatManager.GetChatByName(ChatName);

            if (InteractingChatRoom == null)
            {
                InteractingChatRoom.SendGreyChatAlert(Client, "<font color='red'><b>Expulso do grupo; Não autorizado</b></font>");
                return;
            }
            #endregion

            switch (Action.ToLower())
            {

                #region Joined ChatRoom
                case "joinedchat":
				case "entrarchat":

                    if (!WebSocketChatManager.AuthenticatedInChatRoom(Client, InteractingChatRoom.ChatName))
                    {
                        InteractingChatRoom.SendGreyChatAlert(Client, "<font color='red'><b>Expulso do grupo; Não autorizado</b></font>");
                        return;
                    }

                    if (!Client.GetRoleplay().ChatRooms.ContainsKey(InteractingChatRoom.ChatName))
                    {
                        if (Client.GetRoleplay().WebSocketConnection == null)
                        {
                            Client.SendWhisper("Erro! Sua conexão de Socket está desconectada! Entre em contato com um membro da equipe se esse problema persistir ou tente reentrar.", 1);
                            return;
                        }
                        Client.GetRoleplay().ChatRooms.TryAdd(ChatName, WebSocketChatManager.GetChatByName(InteractingChatRoom.ChatName));
                        InteractingChatRoom.BuildChatDIV(Client);
                        InteractingChatRoom.AuthoriseChatJoin(Client);
                        InteractingChatRoom.BroadCastChatData(Client, JsonConvert.SerializeObject(new Dictionary<object, object>()
                        {
                            { "event", "chatManager" },
                            { "chatname", ChatName },
                            { "action", "newjoinedchat" },
                            { "chatusername", Client.GetHabbo().Username },
                            { "chatuserfigure", Client.GetHabbo().Look }
                        }), true);

                    }
                    else
                    {
                        if (Client.GetRoleplay().WebSocketConnection == null)
                        {
                            Client.SendWhisper("Erro! Sua conexão de Socket está desconectada! Entre em contato com um membro da equipe se esse problema persistir ou tente reentrar.", 1);
                            return;
                        }

                        InteractingChatRoom.BuildChatDIV(Client);
                    }
                    break;
                #endregion

                #region Left Chatroom
                case "leavechat":
				case "sairchat":
				case "sair":
				case "quit":
                    WebSocketChatManager.Disconnect(Client, InteractingChatRoom.ChatName, false, null);
                    break;
                #endregion

                #region Compose Chat Box
                case "buildchat":
				case "fazerchat":
				case "criarchat":
                    InteractingChatRoom.BuildChatDIV(Client);
                    break;
                #endregion

                #region Sent Chat Message
                case "onchat":
				case "emchat":
                    {

                        #region Variables
                        if (!ReturnedData.ContainsKey("chatusername") || !ReturnedData.ContainsKey("chatmessage"))
                            return;

                        string ChatUsername = Convert.ToString(ReturnedData["chatusername"]);
                        string ChatMessage = Convert.ToString(ReturnedData["chatmessage"]);

                        #endregion

                        #region Conditions

                        #region Permanant bans
                        if (Client.GetRoleplay().BannedFromChatting)
                        {

                            if (Client.GetRoleplay().WebSocketConnection == null)
                            {
                                Client.SendWhisper("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!", 1);
                            }
                            else
                            {
                                Client.GetRoleplay().SendTopAlert("Você está proibido permanentemente de entrar em qualquer grupo de Whatsapp!");
                            }

                            return;
                        }
                        #endregion

                        if (InteractingChatRoom.ExploitingAction(Client))
                        {
                            return;
                        }

                        if (InteractingChatRoom.MutedRoom && !InteractingChatRoom.CanBypassRestrictions(Client))
                        {
                            InteractingChatRoom.SendGreyChatAlert(Client, "<font color='red'><b>Todo mundo neste grupo foi silenciado!</b></font>");
                            return;
                        }

                        if (ChatMessage.Length > 800)
                        {
                            InteractingChatRoom.WarnUser(Client, "<font color='red'>Falha no envio da mensagem. <b>Razão</b>; Mensagem muito longa.</font>");
                            return;
                        }

                        if (ChatUsername.ToLower() != Client.GetHabbo().Username.ToLower())
                        {
                            WebSocketChatManager.Disconnect(Client, InteractingChatRoom.ChatName, true, "Expulso do grupo; Pare de tentar explorar o sistema");
                            return;
                        }


                        if (InteractingChatRoom == null)
                        {
                            WebSocketChatManager.Disconnect(Client, InteractingChatRoom.ChatName, true, "Expulso do grupo; Este grupo não existe!");
                            return;
                        }

                        if (!WebSocketChatManager.AuthenticatedInChatRoom(Client, InteractingChatRoom.ChatName))
                        {
                            WebSocketChatManager.Disconnect(Client, InteractingChatRoom.ChatName, true, "Você não está mais neste chat!");
                            return;
                        }

                        if (InteractingChatRoom.MutedUsers.ContainsKey(Client.GetHabbo().Id))
                        {

                            int MuteTimeSeconds = Convert.ToInt32(InteractingChatRoom.MutedUsers[Client.GetHabbo().Id] - PlusEnvironment.GetUnixTimestamp());
                            string MuteTimeFormatted = "";

                            if (MuteTimeSeconds < 60) { MuteTimeFormatted = MuteTimeFormatted + " segundos."; }
                            else if (MuteTimeSeconds > 60 && MuteTimeSeconds < (60 * 60)) { MuteTimeFormatted = (MuteTimeSeconds / 60) + " minuto(s)."; }
                            else { MuteTimeFormatted = ((((MuteTimeSeconds) / 60)) / 60) + " horas."; }

                            if (Client.GetRoleplay().WebSocketConnection == null)
                                Client.SendWhisper("Você está silenciado deste grupo por " + MuteTimeFormatted + "!", 1);
                            else
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<font color='red'><b>Você está mutado por " + MuteTimeFormatted + "</b></font>");
                            }

                            return;
                        }

                        if (InteractingChatRoom.BannedUsers.ContainsKey(Client.GetHabbo().Id))
                            return;

                        int MuteTime;
                        InteractingChatRoom.IncrementAndCheckFlood(Client, out MuteTime);

                        if (Client.GetRoleplay().socketChatFloodTime > 0)
                        {
                            InteractingChatRoom.SendGreyChatAlert(Client, "O Sistema mutou você! <font color='red'><b>Razão</b>: <b>Flood excessivo</b></font>");
                            return;
                        }

                        #endregion

                        #region Execute

                        /*
						#region Text stuff

                        int TextCost;
                        bool CanAfford;

                        if (Client.GetRoleplay().PhoneType == 1)
                            TextCost = 3;
                        else if (Client.GetRoleplay().PhoneType == 2)
                            TextCost = 2;
                        else if (Client.GetRoleplay().PhoneType == 3)
                            TextCost = 1;
                        else
                            TextCost = 3;

                        if (Client.GetHabbo().Duckets < TextCost)
                            CanAfford = false;
                        else
                            CanAfford = true;

                        if (!CanAfford)
                        {
                            InteractingChatRoom.SendGreyChatAlert(Client, "<font color='red'><b>Você não pode enviar uma mensagem de texto, você está sem Crédito de Celular!</b></font>");
                            return;
                        }
						*/

                        /*Client.GetHabbo().Duckets -= TextCost;
                        Client.GetHabbo().UpdateDucketsBalance();
						*/

                        //Client.SendWhisper("You have successfully sent a text message to the '" + InteractingChatRoom.ChatName + "' WhatsHolo Group Chat with your " + RoleplayManager.GetPhoneName(Client) + "!", 1);
                        //Client.SendMessage(new RoomNotificationComposer("text_message", "message", "Você enviou uma mensagem para o grupo '" + InteractingChatRoom.ChatName + "'!"));

                        if (Client.GetRoomUser() != null)
                        {
                            if (Client.GetRoomUser().CurrentEffect != 65)
                                Client.GetRoomUser().ApplyEffect(EffectsList.CellPhone);
                            Client.GetRoleplay().TextTimer = 5;
                        }

                        #endregion

                        if (Client.GetHabbo().Translating)
                        {
                            string LG1 = Client.GetHabbo().FromLanguage.ToLower();
                            string LG2 = Client.GetHabbo().ToLanguage.ToLower();

                            ReturnedData["chatmessage"] = PlusEnvironment.TranslateText(ChatMessage, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]";
                        }
                        else
                            ReturnedData["chatmessage"] = ChatMessage;

                        InteractingChatRoom.BroadCastNewChat(Client, ReturnedData);
                        InteractingChatRoom.AddChatLog(Client, ChatMessage);

                        #endregion

                        break;
                    }

                #region Join New ChatRoom
                case "requestjoin":
                    {
                        string chatType = InteractingChatRoom.GetChatType();

                        #region Check chat joined count
                        if (Client.GetRoleplay().ChatRooms.Count > 3)
                        {
                            Client.SendWhisper("Você só pode entrar em 5 chats por vez!", 1);
                            return;
                        }
                        #endregion

                        #region Check phone
                        if (!Client.GetRoleplay().PhoneApps.Contains("whatsapp"))
                        {
                            Client.SendWhisper("Você precisa do Aplicativo Whatsapp para entrar em um Grupo! Digite :baixar whatsapp para instalar!", 1);
                            return;
                        }
                        #endregion

                        #region Check Ban


                        #region Permanant bans
                        if (Client.GetRoleplay().BannedFromChatting)
                        {

                            if (Client.GetRoleplay().WebSocketConnection == null)
                            {
                                Client.SendWhisper("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!", 1);
                            }
                            else
                            {
                                Client.GetRoleplay().SendTopAlert("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!");
                            }

                            return;
                        }
                        #endregion

                        if (InteractingChatRoom.BannedUsers.ContainsKey(Client.GetHabbo().Id))
                        {

                            if (Client == null) return;
                            if (Client.GetRoleplay() == null) return;


                            #region Permanant bans
                            if (Client.GetRoleplay().BannedFromChatting)
                            {

                                if (Client.GetRoleplay().WebSocketConnection == null)
                                {
                                    Client.SendWhisper("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!", 1);
                                }
                                else
                                {
                                    Client.GetRoleplay().SendTopAlert("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!");
                                }

                                return;
                            }
                            #endregion

                            double BanExpire = InteractingChatRoom.BannedUsers[Client.GetHabbo().Id];
                            double CurrentTime = PlusEnvironment.GetUnixTimestamp();
                            bool Unban = false;
                            
                            if (BanExpire <= CurrentTime)
                            {
                                Unban = true;
                                double OutRemove = 0;
                                InteractingChatRoom.BannedUsers.TryRemove(Client.GetHabbo().Id, out OutRemove);
                            }

                            if (!Unban)
                            {

                                int BanTimeSeconds = Convert.ToInt32(BanExpire - CurrentTime);
                                string BanTimeFormatted = "";

                                if (BanTimeSeconds < 60) { BanTimeFormatted = BanTimeSeconds + " segundos."; }
                                else if (BanTimeSeconds > 60 && BanTimeSeconds  < (60 * 60)) { BanTimeFormatted = (BanTimeSeconds / 60) + " minutos."; }
                                else { BanTimeFormatted = ((((BanTimeSeconds) / 60)) / 60) + " horas.";  }

                                if (Client.GetRoleplay().WebSocketConnection == null)
                                    Client.SendWhisper("Você está banido por este grupo! Expira em: " + BanTimeFormatted, 1);
                                else
                                    Client.GetRoleplay().SendTopAlert("Você está banido por este grupo! Expira em: " + BanTimeFormatted);
                                return;
                            }
                        }
                        #endregion

                        #region Password
                        if (chatType == "password")
                        {
                            // requires pw
                            if (Client.GetRoleplay().WebSocketConnection == null)
                            {
                                Client.SendWhisper("Não é possível solicitar a participação no grupo, a conexão do WebSocket está desligada. Entre em contato com um membro da equipe ou tente reentrar do jogo!", 1);
                                return;
                            }

                            if (InteractingChatRoom.ChatOwner != Client.GetHabbo().Id && !InteractingChatRoom.ChatAdmins.Contains(Client.GetHabbo().Id) && Client.GetHabbo().VIPRank < 2)
                            {
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, JsonConvert.SerializeObject(new Dictionary<object, object>()
                                 {
                                    { "event", "chatManager" },
                                    { "chatname", InteractingChatRoom.ChatName},
                                    { "action", "request_chatpassword" },
                                 }));
                                return;
                            }
                        }
                        #endregion

                        #region Gang
                        if (chatType == "gang")
                        {
                            if (Client.GetRoleplay().WebSocketConnection == null)
                            {
                                Client.SendWhisper("Não é possível solicitar a participação no grupo, a conexão do WebSocket está desligada. Entre em contato com um membro da equipe ou tente reentrar do jogo!", 1);
                                return;
                            }
                            if (Client.GetRoleplay().GangId != Convert.ToInt32(InteractingChatRoom.ChatValues["gang"]) && !InteractingChatRoom.CanBypassRestrictions(Client, true))
                            {
                                Client.GetRoleplay().SendTopAlert("Você deve estar na gangue pertencente a este quarto!");
                                return;
                            }
                        }
                        #endregion

                        #region Locked
                        if (chatType == "locked")
                        {
                            if (Client.GetRoleplay().WebSocketConnection == null)
                            {
                                Client.SendWhisper("Não é possível solicitar a participação no grupo, a conexão do WebSocket está desligada. Entre em contato com um membro da equipe ou tente reentrar do jogo!", 1);
                                return;
                            }

                            if (InteractingChatRoom.ChatName.Equals("vip-chat") && Client.GetHabbo().VIPRank > 0)
                            {
                                if (InteractingChatRoom.OnUserJoin(Client))
                                {
                                    if (Client.GetRoleplay().WebSocketConnection == null)
                                    {
                                        Client.SendWhisper("Não é possível solicitar a participação no grupo, a conexão do WebSocket está desligada. Entre em contato com um membro da equipe ou tente reentrar do jogo!", 1);
                                        return;
                                    }
                                    InteractingChatRoom.BeginChatJoin(Client);
                                }
                                else
                                    Client.GetRoleplay().SendTopAlert("Ocorreu um erro, incapaz de participar deste grupo!");
                                return;
                            }

                            if (InteractingChatRoom.ChatOwner != Client.GetHabbo().Id && !InteractingChatRoom.ChatAdmins.Contains(Client.GetHabbo().Id) && Client.GetHabbo().VIPRank < 2)
                            {
                                Client.GetRoleplay().SendTopAlert("Este chat está trancado!");
                                return;
                            }

                        }
                        #endregion

                        #region Available

                        if (InteractingChatRoom.OnUserJoin(Client))
                        {
                            if (Client.GetRoleplay().WebSocketConnection == null)
                            {
                                Client.SendWhisper("Não é possível solicitar a participação no grupo, a conexão do WebSocket está desligada. Entre em contato com um membro da equipe ou tente reentrar do jogo!", 1);
                                return;
                            }
                            InteractingChatRoom.BeginChatJoin(Client);
                        }
                        else
                            Client.GetRoleplay().SendTopAlert("Ocorreu um erro, incapaz de participar deste grupo!");

                        #endregion

                        break;
                    }
                #endregion

                #region Check ChatRoom Password
                case "checkpassword":
				case "versenha":

                    #region Permanant bans
                    if (Client.GetRoleplay().BannedFromChatting)
                    {

                        if (Client.GetRoleplay().WebSocketConnection == null)
                        {
                            Client.SendWhisper("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!", 1);
                        }
                        else
                        {
                            Client.GetRoleplay().SendTopAlert("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!");
                        }

                        return;
                    }
                    #endregion

                    if (!ReturnedData.ContainsKey("password")) return;
                    if (!InteractingChatRoom.ChatValues.ContainsKey("password")) return;

                    string Password = Convert.ToString(ReturnedData["password"]);
                    string ActualPassword = Convert.ToString(InteractingChatRoom.ChatValues["password"]);

                    if (Password == ActualPassword && !string.IsNullOrEmpty(ActualPassword))
                    {

                        if (Client.GetRoleplay().WebSocketConnection == null)
                        {
                            Client.SendWhisper("Não é possível solicitar a participação no grupo, a conexão do WebSocket está desligada. Entre em contato com um membro da equipe ou tente reentrar do jogo!", 1);
                            return;
                        }

                        if (InteractingChatRoom.OnUserJoin(Client))
                            InteractingChatRoom.BeginChatJoin(Client);
                    }
                    else
                    {
                        Client.GetRoleplay().SendTopAlert("Senha de grupo incorreta!");
                        return;
                    }

                    break;
                #endregion

                #region Sent Chat Command
                case "onchatcommand":
				case "cchat":
                    {

                        #region Prevent Bypasses

                        #region Permanant bans
                        if (Client.GetRoleplay().BannedFromChatting)
                        {

                            if (Client.GetRoleplay().WebSocketConnection == null)
                            {
                                Client.SendWhisper("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!", 1);
                            }
                            else
                            {
                                Client.GetRoleplay().SendTopAlert("Você está proibido permanentemente de entrar em qualquer grupo de whatsapp!");
                            }

                            return;
                        }
                        #endregion

                        if (InteractingChatRoom.ExploitingAction(Client))
                            return;

                        if (InteractingChatRoom.MutedUsers.ContainsKey(Client.GetHabbo().Id))
                            return;

                        if (InteractingChatRoom.BannedUsers.ContainsKey(Client.GetHabbo().Id))
                            return;

                        if (InteractingChatRoom.MutedRoom && !InteractingChatRoom.CanBypassRestrictions(Client))
                            return;

                        #endregion

                        string Command = Convert.ToString(ReturnedData["chatmessage"]);

                        #region User Commands

                        #region Clear chat
                        if (Command.StartsWith("/limpar"))
                        {

                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<object, object>()
                            {
                                { "event", "chatManager" },
                                { "action", "clearchat" },
                                { "chatname", InteractingChatRoom.ChatName }
                            }));

                            return;
                        }
                        #endregion

                        #region Youtube

                        if (Command.StartsWith("/yt "))
                        {

                            #region Set Params / variables
                            string YoutubeVideo = ((System.Text.RegularExpressions.Regex.Split(Command, "/yt ")[1]).Trim());
                            string YoutubeVidEnd = "";
                            #endregion

                            #region Conditions
                            if (!YoutubeVideo.StartsWith("h") || (!YoutubeVideo.Contains(".com") && !YoutubeVideo.Contains("be")))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "Link do youtube inválido!: ' " + YoutubeVideo + " '");
                                return;
                            }
                            #endregion

                            #region Set youtube link
                            if (YoutubeVideo.Contains("youtu.be"))
                            {
                                YoutubeVidEnd = Regex.Split(YoutubeVideo, "be/")[1];
                            }
                            else if (YoutubeVideo.Contains("?v="))
                            {
                                YoutubeVidEnd = Regex.Split(YoutubeVideo, "v=")[1];
                            }
                            else if (YoutubeVideo.Contains("embed/"))
                            {
                                YoutubeVidEnd = Regex.Split(YoutubeVideo, "embed/")[1];
                            }

                            if (YoutubeVidEnd == "")
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "Link do youtube inválido!");
                                return;
                            }
                            #endregion

                            #region Execute
                            Dictionary<object, object> Params = new Dictionary<object, object>()
                            {
                                { "chatmessage", "blank" },
                                { "chatname", InteractingChatRoom.ChatName },
                                { "chatusername", Client.GetHabbo().Username },
                                { "chatEmbedType", "youtube" },
                                { "chatEmbedLink", YoutubeVidEnd },
                                { "chatEmbedExtra", "none" }
                            };

                            InteractingChatRoom.BroadCastChatHTML(Client, Params);
                            return;
                            #endregion

                        }

                        #endregion

                        #region Screenshot
                        if (Command.StartsWith("/f "))
                        {

                            #region Set params / variables
                            string Picture = ((System.Text.RegularExpressions.Regex.Split(Command, "/f ")[1]).Trim());
                            string PictureSite = "";
                            #endregion

                            #region Get Image type
                            if (Picture.Contains("prntscr.com"))
                            {
                                PictureSite = "prntscr";
                            }

                            if (Picture.EndsWith(".png") || Picture.EndsWith(".gif") || Picture.EndsWith(".jpeg") || Picture.EndsWith(".jpg"))
                            {
                                PictureSite = "raw";
                            }

                            if (PictureSite == "")
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "Você não pode enviar esta foto. Use o prntscr.com ou envie fotos que termine com <b>.png</b>, <b>.jpg</b>, ou <b>.gif</b>");
                                return;
                            }
                            #endregion

                            #region Execute
                            Dictionary<object, object> Params = new Dictionary<object, object>()
                            {
                                { "chatmessage", "blank" },
                                { "chatname", InteractingChatRoom.ChatName },
                                { "chatusername", Client.GetHabbo().Username },
                                { "chatEmbedType", "image" },
                                { "chatEmbedLink",  Picture},
                                { "chatEmbedExtra", PictureSite }
                            };

                            InteractingChatRoom.BroadCastChatHTML(Client, Params);
                            #endregion

                            return;
                        }
                        #endregion

                        #region Online users
                        if (Command.ToLower().StartsWith("/onlines") || Command.ToLower().StartsWith("/usuarios"))
                        {

                            string OnlineUsers = "";

                            foreach (GameClient OnlineUser in InteractingChatRoom.ChatUsers.Keys)
                            {
                               
                                if (OnlineUser == null)
                                    continue;

                                if (OnlineUser.GetHabbo() == null)
                                    continue;

                                OnlineUsers += OnlineUser.GetHabbo().Username + "<br/>";
                            }

                            string OnlineUsersRet = "";
                            OnlineUsersRet += "<br/><b><-----></b><br/>";
                            OnlineUsersRet += "<b>Lista de usuários online neste grupo</b><br/>";
                            OnlineUsersRet += "<b><-----></b><br/>";
                            OnlineUsersRet += OnlineUsers;
                            OnlineUsersRet += "<b><-----></b><br/>";
                            InteractingChatRoom.SendGreyChatAlert(Client, OnlineUsersRet);

                            return;
                        }
                        #endregion

                        #region Chat Admins
                        if (Command.ToLower().StartsWith("/administradores"))
                        {

                            string ChatAdmins = "";
                            string OwnerName = "";

                            foreach (int ChatAdmin in InteractingChatRoom.ChatAdmins)
                            {

                                using (UserCache ChatAdminData = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(ChatAdmin))
                                {
                                    if (ChatAdminData == null)
                                        continue;
                                   

                                    ChatAdmins += ChatAdminData.Username + "<br/>";
                                }


                            }

                            using (UserCache ChatAdminData = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(InteractingChatRoom.ChatOwner))
                            {
                                if (ChatAdminData != null)
                                {
                                    if (!String.IsNullOrEmpty(ChatAdminData.Username))
                                        OwnerName = ChatAdminData.Username;
                                    else
                                        OwnerName = "Nenhum";
                                }
                                else
                                    OwnerName = "Nenhum";
                            }

                                string ChatUsersRet = "";
                            ChatUsersRet += "<br/><b><-----></b><br/>";
                            ChatUsersRet += "<b>Lista de administradores do grupo</b><br/>";
                            ChatUsersRet += "<b><-----></b><br/>";
                            ChatUsersRet += "<b><font color='red'>" + OwnerName + "</font></b><br/>";
                            ChatUsersRet += ChatAdmins;
                            ChatUsersRet += "<b><-----></b><br/>";
                            InteractingChatRoom.SendGreyChatAlert(Client, ChatUsersRet);

                            return;
                        }
                        #endregion

                        #region Commands
                        if (Command.ToLower().StartsWith("/comandos"))
                        {

                            string Commands = "";

                            Commands += "<center><br/><b><-----></b><br/>";
                            Commands += "<b>Lista de Comandos</b><br/>";
                            Commands += "<b><-----></b><br/>";
                            Commands += "<b>/comandos</b>.<br/>";
                            Commands += "<b>/limpar</b>.<br/>";
                            Commands += "<b>/yt</b> [link do youtube].<br/>";
                            Commands += "<b>/f</b> [link da foto].<br/>";
                            Commands += "<b>/onlines</b>.<br/>";
                            Commands += "<b>/administradores</b><br/></center>";

                            if (Client.GetHabbo().Id == InteractingChatRoom.ChatOwner || InteractingChatRoom.ChatAdmins.Contains(Client.GetHabbo().Id) || Client.GetHabbo().Rank > 4)
                            {
                                Commands += "<center><b>/mutartodos</b>.<br/>";
                                Commands += "<b>/desmutartodos</b>.<br/>";
                                Commands += "<b>/expulsartodos</b><br/>";
                                Commands += "<b>/expulsar [usuário]</b>.<br/>";
                                Commands += "<b>/banir [usuário] [minutos]</b>.<br/>";
                                Commands += "<b>/mutar [usuário] [minutos]</b>.<br/>";
                                Commands += "<b>/desbanir [usuário]</b>.<br/>";
                                Commands += "<b>/desmutar [usuário]</b>.<br/>";
                                Commands += "<b>/banidos</b>.<br/>";
                                Commands += "<b>/mutados</b>.<br/>";
                                Commands += "<b>/comandos</b>.<br/>";
                                Commands += "<b>/trancar</b>.<br/>";
                                Commands += "<b>/destrancar</b>.<br/></center>";

                                if (Client.GetHabbo().Id == InteractingChatRoom.ChatOwner || Client.GetHabbo().VIPRank > 1)
                                {
                                    
                                    Commands += "<center><b>/daradmin [usuário]</b>.<br/>";
                                    Commands += "<b>/removeradmin [usuário]</b>.<br/>";
                                    Commands += "<b>/senha [senha]</b>.<br/>";
                                    Commands += "<b>/removersenha</b>.<br/>";
                                    Commands += "<b>/chatgangue</b>.<br/>";
                                    Commands += "<b>/naogangue</b>.<br/></center>";
                                    if (Client.GetHabbo().VIPRank > 1) { Commands += "<font color='green'><b>[Você tem direitos especiais, você pode usar todos esses comandos]</b></font><br/>"; }
                                }

                            }

                            Commands += "<b><-----></b><br/>";

                            InteractingChatRoom.SendGreyChatAlert(Client, Commands);

                        }
                        #endregion

                        #endregion

                        #region Admin Commands

                        #region Block un-authorised users
                        if (Client.GetHabbo().Id != InteractingChatRoom.ChatOwner && !InteractingChatRoom.ChatAdmins.Contains(Client.GetHabbo().Id) && Client.GetHabbo().Rank < 5)
                        {
                            InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode usar este comando porque você não é um administrador desse grupo!</b></font>");
                            return;
                        }
                        #endregion

                        #region Kick
                        if (Command.ToLower().StartsWith("/expulsar "))
                        {

                            #region Set Params / Variables
                            string TargetUser = Regex.Split(Command, "/expulsar ")[1].Trim();
                            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetUser);
                            #endregion

                            #region Conditions
                            if (TargetUser.Contains("["))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Nome de usuário Inválido! Remova o '[' </font></b>");
                                return;
                            }
                            if (TargetSession == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</b></font>");
                                return;
                            }

                            if (!InteractingChatRoom.ChatUsers.ContainsKey(TargetSession))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</b></font>");
                                return;
                            }

                            if (TargetSession.GetHabbo().Id == InteractingChatRoom.ChatOwner)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode expulsar um administrador do grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo().Id == Client.GetHabbo().Id)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode expulsar você mesmo!</font></b>");
                                return;
                            }
                            if (TargetSession.GetHabbo().Rank > 4)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode expulsar um membro da Equipe!</font></b>");
                                return;
                            }
                            #endregion

                            #region Execute
                            InteractingChatRoom.BroadCastChatWarning(Client, TargetSession.GetHabbo().Username + " foi expulso do grupo por " + Client.GetHabbo().Username + "!");
                            WebSocketChatManager.Disconnect(TargetSession, InteractingChatRoom.ChatName, true, "Você foi expulso do grupo: '" + InteractingChatRoom.ChatName + "'");
                            
                            return;
                            #endregion

                        }
                        #endregion

                        #region Kick All

                        if (Command.ToLower().StartsWith("/expulsartodos"))
                        {

                            #region Conditions

                            if (!InteractingChatRoom.CanBypassRestrictions(Client, true))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }

                            #endregion

                            #region Execute

                            if (InteractingChatRoom.ChatUsers == null)
                                return;


                            InteractingChatRoom.BroadCastChatWarning(Client, "Todos do grupo, exceto os administradores, foram expulsos!");

                            foreach (GameClient ChatUser in InteractingChatRoom.ChatUsers.Keys)
                            {
                                if (ChatUser == null)
                                    continue;

                                if (ChatUser.LoggingOut)
                                    continue;

                                if (ChatUser.GetRoleplay() == null)
                                    continue;

                                if (ChatUser.GetRoleplay().WebSocketConnection == null)
                                    continue;

                                if (ChatUser.GetHabbo() == null)
                                    continue;

                                if (InteractingChatRoom.CanBypassRestrictions(ChatUser))
                                    continue;

                                WebSocketChatManager.Disconnect(ChatUser, InteractingChatRoom.ChatName, true, "Todos foram expulsos do grupo: '" + InteractingChatRoom.ChatName + "'");
                            }

                            #endregion

                            return;
                        }
                        #endregion

                        #region Ban
                        if (Command.ToLower().StartsWith("/banir "))
                        {

                            #region Set Variables / Params
                            int ParamsLength = Regex.Split(Regex.Split(Command, "/banir ")[1], " ").Length + 1;
                            
                            string TargetUser = Regex.Split(Regex.Split(Command, "/banir ")[1], " ")[0];
                            int BanTime = 1;

                            if (ParamsLength < 3)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Comando inválido! Use :banir [usuário] [minutos]</font></b>");
                                return;
                            }

                            if (!int.TryParse(Regex.Split(Regex.Split(Command, "/banir ")[1], " ")[1], out BanTime))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Tempo inválido!</font></b>");
                                return;
                            }

                            if (BanTime <= 0)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Tempo inválido!</font></b>");
                                return;
                            }

                            Double Expire = (PlusEnvironment.GetUnixTimestamp() + (Convert.ToDouble(BanTime) * 60));

                            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetUser);
                            
                            #endregion

                            #region Conditions

                            if (TargetUser.Contains("["))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Usuário inválido! Remova o '[' </font></b>");
                                return;
                            }

                            if (TargetSession == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo() == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (!InteractingChatRoom.ChatUsers.ContainsKey(TargetSession))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (InteractingChatRoom.BannedUsers.ContainsKey(TargetSession.GetHabbo().Id))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário já está banido!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo().Id == InteractingChatRoom.ChatOwner)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode banir um administrador do grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo().Id == Client.GetHabbo().Id)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode banir você mesmo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo().Rank > 4)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'> Você não pode banir um membro da Equipe!</font></b>");
                                return;
                            }

                            #endregion

                            #region Execute
                            InteractingChatRoom.BannedUsers.TryAdd(TargetSession.GetHabbo().Id, Expire);

                            string BanTimeFormatted = BanTime + " minutos.";
                            InteractingChatRoom.BroadCastChatWarning(Client, TargetSession.GetHabbo().Username + " foi banido do grupo por " + Client.GetHabbo().Username + "!");
                            WebSocketChatManager.Disconnect(TargetSession, InteractingChatRoom.ChatName, true, "Você foi banido do grupo: '" + InteractingChatRoom.ChatName + "', por " + Client.GetHabbo().Username + " por " + BanTimeFormatted);

                            int OutInt = 0;

                            if (InteractingChatRoom.UnbannedUsers.ContainsKey(TargetSession.GetHabbo().Id))
                                InteractingChatRoom.UnbannedUsers.TryRemove(TargetSession.GetHabbo().Id, out OutInt);

                            return;
                            #endregion

                        }
                        #endregion

                        #region Mute All

                        if (Command.ToLower().StartsWith("/mutartodos"))
                        {

                            #region Conditions

                            if (!InteractingChatRoom.CanBypassRestrictions(Client, true))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }

                            #endregion

                            #region Execute

                            if (InteractingChatRoom.ChatUsers == null)
                                return;

                            foreach(GameClient ChatUser in InteractingChatRoom.ChatUsers.Keys)
                            {
                                if (ChatUser == null)
                                    continue;

                                if (ChatUser.LoggingOut)
                                    continue;

                                if (ChatUser.GetRoleplay() == null)
                                    continue;

                                if (ChatUser.GetRoleplay().WebSocketConnection == null)
                                    continue;

                                InteractingChatRoom.SendGreyChatAlert(ChatUser, "<font color='red'><b>Todos neste grupo, exceto os administradores foram silenciado.</b></font>");
                            }

                            InteractingChatRoom.MutedRoom = true;

                            #endregion

                            return;
                        }
                        #endregion

                        #region Un-Mute All

                        if (Command.ToLower().StartsWith("/desmutartodos"))
                        {

                            #region Conditions

                            if (!InteractingChatRoom.CanBypassRestrictions(Client, true))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }

                            #endregion

                            #region Execute

                            if (InteractingChatRoom.ChatUsers == null)
                                return;

                            foreach (GameClient ChatUser in InteractingChatRoom.ChatUsers.Keys)
                            {
                                if (ChatUser == null)
                                    continue;

                                if (ChatUser.LoggingOut)
                                    continue;

                                if (ChatUser.GetRoleplay() == null)
                                    continue;

                                if (ChatUser.GetRoleplay().WebSocketConnection == null)
                                    continue;

                                InteractingChatRoom.SendGreyChatAlert(ChatUser, "<font color='red'><b>Todos neste grupo podem falar novamente.</b></font>");
                            }

                            InteractingChatRoom.MutedRoom = false;

                            #endregion

                            return;
                        }
                        #endregion

                        #region Mute
                        if (Command.ToLower().StartsWith("/mutar "))
                            {

                            #region Set Variables / Params
                            int ParamsLength = Regex.Split(Regex.Split(Command, "/mutar ")[1], " ").Length + 1;

                            if (ParamsLength < 3)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Comando inválido! Use :mutar [usuário] [minutos]</font></b>");
                                return;
                            }

                            string TargetUser = Regex.Split(Regex.Split(Command, "/mutar ")[1], " ")[0];
                            int ToMuteTime = 1;

                            if (!int.TryParse(Regex.Split(Regex.Split(Command, "/mutar ")[1], " ")[1], out ToMuteTime))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Tempo inválido!</font></b>");
                                return;
                            }

                            if (ToMuteTime <= 0)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Tempo inválido!</font></b>");
                                return;
                            }

                            Double Expire = (PlusEnvironment.GetUnixTimestamp() + (Convert.ToDouble(ToMuteTime) * 60));
                            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetUser);

                            #endregion

                            #region Conditions
                            if (TargetUser.Contains("["))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Usuário inválido! Remova o '[' </font></b>");
                                return;
                            }

                            if (TargetSession == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo() == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (!InteractingChatRoom.ChatUsers.ContainsKey(TargetSession))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (InteractingChatRoom.MutedUsers.ContainsKey(TargetSession.GetHabbo().Id))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário já está mutado!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo().Id == InteractingChatRoom.ChatOwner)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode mutar um administrador do grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo().Id == Client.GetHabbo().Id)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode mutar você mesmo!</font></b>");
                                return;
                            }
                            if (TargetSession.GetHabbo().Rank > 4)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode mutar um membro da Equipe!</font></b>");
                                return;
                            }
                            #endregion

                            #region Execute
                            InteractingChatRoom.MutedUsers.TryAdd(TargetSession.GetHabbo().Id, Expire);

                            string MuteTimeFormatted = "";
                            int MuteTimeSeconds = ToMuteTime * 60;

                            if (MuteTimeSeconds < 60) { MuteTimeFormatted = MuteTimeFormatted + " segundos."; }
                            else if (MuteTimeSeconds > 60 && MuteTimeSeconds < (60 * 60)) { MuteTimeFormatted = (MuteTimeSeconds / 60) + " minutos."; }
                            else { MuteTimeFormatted = ((((MuteTimeSeconds) / 60)) / 60) + " horas."; }

                            InteractingChatRoom.BroadCastChatWarning(Client, TargetSession.GetHabbo().Username + " foi mutado por " + Client.GetHabbo().Username + "!");
                            InteractingChatRoom.SendGreyChatAlert(TargetSession, "<font color='red'><b>Você foi mutado por " + MuteTimeFormatted + "</b></font>");

                            int OutInt = 0;

                            if (InteractingChatRoom.UnmutedUsers.ContainsKey(TargetSession.GetHabbo().Id))
                                InteractingChatRoom.UnmutedUsers.TryRemove(TargetSession.GetHabbo().Id, out OutInt);

                            return;
                            #endregion

                        }
                        #endregion

                        #region Unban
                        if (Command.ToLower().StartsWith("/desbanir "))
                        {

                            #region Set Variables / Params
                            int ParamsLength = Regex.Split(Regex.Split(Command, "/desbanir ")[1], " ").Length + 1;

                            string TargetUser = Regex.Split(Regex.Split(Command, "/desbanir ")[1], " ")[0];

                            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetUser);
                            #endregion

                            #region Conditions

                            if (TargetUser.Contains("["))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Usuário inválido! Remova o '[' </font></b>");
                                return;
                            }

                            if (TargetSession == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo() == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }
                            
                            if (!InteractingChatRoom.BannedUsers.ContainsKey(TargetSession.GetHabbo().Id))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não está banido!</font></b>");
                                return;
                            }
                            #endregion

                            #region Execute
                            double OutDouble = 0;
                            InteractingChatRoom.BannedUsers.TryRemove(TargetSession.GetHabbo().Id, out OutDouble);

                            InteractingChatRoom.BroadCastChatWarning(Client, TargetSession.GetHabbo().Username + " foi desbanido do grupo por " + Client.GetHabbo().Username + "!");

                            int OutInt = 0;

                            if (InteractingChatRoom.UnbannedUsers.ContainsKey(TargetSession.GetHabbo().Id))
                                InteractingChatRoom.UnbannedUsers.TryRemove(TargetSession.GetHabbo().Id, out OutInt);

                            InteractingChatRoom.UnbannedUsers.TryAdd(TargetSession.GetHabbo().Id, 1);

                            //send unmbanned msg to user
                            #endregion

                            return;
                        }
                        #endregion

                        #region Unmute
                        if (Command.ToLower().StartsWith("/desmutar "))
                        {

                            #region Set Variables / Params
                            int ParamsLength = Regex.Split(Regex.Split(Command, "/desmutar ")[1], " ").Length + 1;

                            string TargetUser = Regex.Split(Regex.Split(Command, "/desmutar ")[1], " ")[0];

                            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetUser);
                            #endregion

                            #region Conditions
                            if (TargetSession == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo() == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (!InteractingChatRoom.MutedUsers.ContainsKey(TargetSession.GetHabbo().Id))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não está mutado!</font></b>");
                                return;
                            }
                            #endregion

                            #region Execute

                            double OutDouble = 0;
                            int OutInt = 0;

                            InteractingChatRoom.MutedUsers.TryRemove(TargetSession.GetHabbo().Id, out OutDouble);

                            InteractingChatRoom.BroadCastChatWarning(Client, TargetSession.GetHabbo().Username + " foi desmutado do grupo por " + Client.GetHabbo().Username + "!");
                            InteractingChatRoom.SendGreyChatAlert(TargetSession, "<b>" + Client.GetHabbo().Username + " desmutou você!</b>");

                            if (InteractingChatRoom.UnmutedUsers.ContainsKey(TargetSession.GetHabbo().Id))
                                InteractingChatRoom.UnmutedUsers.TryRemove(TargetSession.GetHabbo().Id, out OutInt);

                            InteractingChatRoom.UnmutedUsers.TryAdd(TargetSession.GetHabbo().Id, 1);

                            return;
                            #endregion

                        }
                        #endregion

                        #region Banned users
                        if (Command.ToLower().StartsWith("/banidos"))
                        {

                            string BannedUsers = "";

                            foreach(int BannedUser in InteractingChatRoom.BannedUsers.Keys)
                            {
                                GameClient BannedUserClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(BannedUser);

                                if (BannedUserClient == null)
                                    continue;

                                if (BannedUserClient.GetHabbo() == null)
                                    continue;

                                BannedUsers += BannedUserClient.GetHabbo().Username + "<br/>";
                            }

                            string BannedUsersRet = "";
                            BannedUsersRet += "<br/><b><-----></b><br/>";
                            BannedUsersRet += "<b>Lista de usuários banidos do grupo</b><br/>";
                            BannedUsersRet += "<b><-----></b><br/>";
                            BannedUsersRet += BannedUsers;
                            BannedUsersRet += "<b><-----></b><br/>";
                            InteractingChatRoom.SendGreyChatAlert(Client, BannedUsersRet);

                            return;
                        }
                        #endregion

                        #region Muted users
                        if (Command.ToLower().StartsWith("/mutados"))
                        {

                            string retMutedUsers = "";

                            foreach (int MutedUser in InteractingChatRoom.MutedUsers.Keys)
                            {
                                GameClient MutedUserClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(MutedUser);

                                if (MutedUserClient == null)
                                    continue;

                                if (MutedUserClient.GetHabbo() == null)
                                    continue;

                                retMutedUsers += MutedUserClient.GetHabbo().Username + "<br/>";
                            }

                            string MutedUsers = "";
                            MutedUsers += "<br/><b><-----></b><br/>";
                            MutedUsers += "<b>Lista de usuários mutados do grupo</b><br/>";
                            MutedUsers += "<b><-----></b><br/>";
                            MutedUsers += retMutedUsers;
                            MutedUsers += "<b><-----></b><br/>";

                            InteractingChatRoom.SendGreyChatAlert(Client, MutedUsers);

                            return;
                        }
                        #endregion

                        #region Lock
                        if (Command.ToLower().StartsWith("/trancar"))
                        {

                            InteractingChatRoom.ChatValues["locked"] = true;
                            Client.SendWhisper("Você trancou o grupo '" + InteractingChatRoom.ChatName + "' com sucesso!", 1);

                            InteractingChatRoom.BroadCastChatWarning(Client, "<b><font color='red'>O grupo foi trancado por " + Client.GetHabbo().Username + "!</font></b>");

                            return;
                        }
                        #endregion

                        #region UnLock
                        if (Command.ToLower().StartsWith("/destrancar"))
                        {

                            InteractingChatRoom.ChatValues["locked"] = false;
                            Client.SendWhisper("Você destrancou o grupo '" + InteractingChatRoom.ChatName + "' com sucesso!", 1);

                            InteractingChatRoom.BroadCastChatWarning(Client, "<b><font color='red'>O grupo foi destrancado por " + Client.GetHabbo().Username + "!</font></b>");

                            return;
                        }
                        #endregion

                        #region Set Password
                        if (Command.ToLower().StartsWith("/senha "))
                        {

                            if (InteractingChatRoom.ChatOwner != Client.GetHabbo().Id  && Client.GetHabbo().VIPRank < 2)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }
                            int ParamsLength = Regex.Split(Regex.Split(Command, "/senha ")[1], " ").Length + 1;

                            if (ParamsLength < 2)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Comando inválido! Use: /senha [senha do grupo]</font></b>");
                                return;
                            }
                            
                            string DesiredPassword = Regex.Split(Regex.Split(Command, "/senha ")[1], " ")[0];

                            InteractingChatRoom.ChatValues["password"] = DesiredPassword;

                            InteractingChatRoom.BroadCastChatWarning(Client, "<b><font color='red'>A senha do grupo foi definida por " + Client.GetHabbo().Username + "!</font></b>");

                            return;
                        }
                        #endregion

                        #region Remove Password
                        if (Command.ToLower().StartsWith("/removersenha"))
                        {

                            if (InteractingChatRoom.ChatOwner != Client.GetHabbo().Id && Client.GetHabbo().VIPRank < 2)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }

                            InteractingChatRoom.ChatValues["password"] = null;

                            InteractingChatRoom.BroadCastChatWarning(Client, "<b><font color='red'>A senha do grupo foi removida por " + Client.GetHabbo().Username + "!</font></b>");

                            return;
                        }
                        #endregion

                        #region Make Admin
                        if (Command.ToLower().StartsWith("/daradmin "))
                        {

                            #region  Set Variables / Params
                            if (InteractingChatRoom.ChatOwner != Client.GetHabbo().Id && Client.GetHabbo().VIPRank < 2)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }
                            int ParamsLength = Regex.Split(Regex.Split(Command, "/daradmin ")[1], " ").Length + 1;

                            if (ParamsLength < 2)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Comando inválido! /daradmin [usuário]</font></b>");
                                return;
                            }
                            
                            string TargetUser = Regex.Split(Regex.Split(Command, "/daradmin ")[1], " ")[0];
                            #endregion

                            #region Conditions

                            if (TargetUser.Contains("["))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Usuário inválido! Remova o '[' </font></b>");
                                return;
                            }

                            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetUser);

                            if (TargetSession == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo() == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }
                            if (TargetSession == Client)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode dar administrador para você mesmo!</font></b>");
                                return;
                            }
                            if (TargetSession.GetHabbo().Id == InteractingChatRoom.ChatOwner)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode fazer isso com um administrador do grupo!</font></b>");
                                return;
                            }
                            if (InteractingChatRoom.ChatAdmins.Contains(TargetSession.GetHabbo().Id))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário já é um administrador.</font></b>");
                                return;
                            }
                            #endregion

                            #region Execute
                            InteractingChatRoom.ChatAdmins.Add(TargetSession.GetHabbo().Id);
                            InteractingChatRoom.SendGreyChatAlert(TargetSession, "<font color='green'><b>" + Client.GetHabbo().Username + " tornou você um administrador do grupo!</b></font>");
                            InteractingChatRoom.SendGreyChatAlert(Client, "<font color='green'><b>Tornou com sucesso " + TargetSession.GetHabbo().Username + " um administrador do grupo!</b></font>");
                            #endregion

                            return;

                        }
                        #endregion

                        #region Remove Admin
                        if (Command.ToLower().StartsWith("/removeradmin "))
                        {

                            #region  Set Variables / Params
                            if (InteractingChatRoom.ChatOwner != Client.GetHabbo().Id && Client.GetHabbo().VIPRank < 2)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }
                            int ParamsLength = Regex.Split(Regex.Split(Command, "/removeradmin ")[1], " ").Length + 1;

                            if (ParamsLength < 2)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Comando inválido! /removeradmin [usuário]</font></b>");
                                return;
                            }

                            string TargetUser = Regex.Split(Regex.Split(Command, "/removeradmin ")[1], " ")[0];
                            #endregion

                            #region Conditions

                            if (TargetUser.Contains("["))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Usuário inválido! Remova o '[' </font></b>");
                                return;
                            }

                            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(TargetUser);

                            if (TargetSession == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo() == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário não foi encontrado neste grupo!</font></b>");
                                return;
                            }

                            if (TargetSession == Client)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode se tornar um administrador!</font></b>");
                                return;
                            }

                            if (TargetSession.GetHabbo().Id == InteractingChatRoom.ChatOwner)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não pode fazer isso com um administrador do grupo!</font></b>");
                                return;
                            }

                            if (!InteractingChatRoom.ChatAdmins.Contains(TargetSession.GetHabbo().Id))
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Este usuário já <b>NÃO</b> é um administrador</font></b>");
                                return;
                            }
                            #endregion

                            #region Execute
                            InteractingChatRoom.ChatAdmins.Remove(TargetSession.GetHabbo().Id);
                            InteractingChatRoom.SendGreyChatAlert(TargetSession, "<font color='red'><b>" + Client.GetHabbo().Username + " removeu você como administrador do grupo!</b></font>");
                            InteractingChatRoom.SendGreyChatAlert(Client, "<font color='red'><b>Removeu com sucesso " + TargetSession.GetHabbo().Username + " como um administrador do grupo!</b></font>");
                            #endregion

                            return;
                        }
                        #endregion

                        #region Set Gang
                        if (Command.ToLower().StartsWith("/chatgangue"))
                        {

                            if (InteractingChatRoom.ChatOwner != Client.GetHabbo().Id)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }

                            Plus.HabboHotel.Groups.Group Gang = GroupManager.GetGang(Client.GetRoleplay().GangId);

                            if (Gang == null)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não tem uma gangue para definir esse grupo!</font></b>");
                                return;
                            }

                            if (Gang.Id <= 1000)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você não tem uma gangue para definir esse grupo!</font></b>");
                                return;
                            }

                            if (Gang.CreatorId != Client.GetHabbo().Id)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser o Chefe da gangue para fazer isso!</font></b>");
                                return;
                            }

                            if (WebSocketChatManager.RunningChatRooms.Values.Where(Runningchat => Runningchat != null).Where(Runningchat => Runningchat.ChatValues.ContainsKey("gang")).Where(Runningchat => Convert.ToInt32(Runningchat.ChatValues["gang"]) == Client.GetRoleplay().GangId).ToList().Count > 0)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Essa gangue já possui um grupo definido!</font></b>");
                                return;
                            }


                            InteractingChatRoom.ChatValues["gang"] = Client.GetRoleplay().GangId;
                            InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Definiu com sucesso o grupo como oficial da gangues para este grupo!</font></b>");

                            return;
                        }
                        #endregion

                        #region Unset Gang
                        if (Command.ToLower().StartsWith("/naogangue"))
                        {

                            if (InteractingChatRoom.ChatOwner != Client.GetHabbo().Id)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Você deve ser um administrador do grupo para fazer isso!</font></b>");
                                return;
                            }

                            Plus.HabboHotel.Groups.Group Gang = GroupManager.GetGang(Client.GetRoleplay().GangId);
                            
                            if (string.IsNullOrEmpty(Convert.ToString(InteractingChatRoom.ChatValues["gang"])) || Convert.ToInt32(InteractingChatRoom.ChatValues["gang"]) == 0)
                            {
                                InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='green'>Este grupo já não tem uma gangue definida a ele!</font></b>");
                                return;
                            }

                            InteractingChatRoom.ChatValues["gang"] = 0;
                            InteractingChatRoom.SendGreyChatAlert(Client, "<b><font color='red'>Retirou sua gangue com sucesso do grupo</font></b>");

                            return;
                        }
                        #endregion


                        #endregion

                        break;
                    }
                    #endregion
            }
        }
    }
}
