using System;
using System.Linq;
using log4net;
using Plus.HabboHotel;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboRoleplay.Misc;
using System.Data;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using System.Text;

namespace Plus.Core
{
    public class ConsoleCommandHandler
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.Core.ConsoleCommandHandler");

        public static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData))
                return;
            try
            {
                #region Command parsing
                string[] parameters = inputData.Split(' ');
                switch (parameters[0].ToLower())
                {
                    #region General

                    #region wha
                    case "wha":
                        {
                            string Notice = CommandManager.MergeParams(parameters, 1);

                            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                if (client == null)
                                    continue;

                                if (client.GetHabbo() == null)
                                    continue;

                                if (client.LoggingOut)
                                    continue;

                                client.SendWhisper("[Alerta do HOTEL] " + Notice, 33);
                            }

                            log.Info("Enviado Alerta: '" + Notice + "'");

                            break;
                        }
                    #endregion

                    #region ban
                    case "pban":
                    case "ban":
					case "banir":
                        {
                            string User = parameters[1].ToLower();
                            GameClient Session = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User);
                            Session.SendNotification("Você foi banido!");

                            if (Session != null)
                            {
                                #region Online Ban
                                if (Session.GetHabbo() == null)
                                    return;

                                PlusEnvironment.GetGame().GetModerationManager().BanUser("[SISTEMA]", ModerationBanType.USERNAME, Session.GetHabbo().Username, "Ban automático ", 1538641023.14615);
                                
                                if (Session != null)
                                {
                                    Session.Disconnect(true);
                                }
                                #endregion
                            }
                            else
                            {
                                #region Offline Ban
                                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                {
                                    double ExpireTimestamp = 1538641023.14615;
                                    dbClient.SetQuery("REPLACE INTO `bans` (`bantype`, `value`, `reason`, `expire`, `added_by`,`added_date`) VALUES ('user', '" + User + "','Automatic console ban', " + ExpireTimestamp + ", '[SYSTEM]', '" + PlusEnvironment.GetUnixTimestamp() + "');");
                                    dbClient.RunQuery();
                                }
                                #endregion
                            }


                            log.Info("Proibido permanentemente: '" + User + "'");

                            break;
                        }
                    #endregion

                    #region unban
                    case "unban":
					case "desbanir":
					case "desban":
                        {
                            string User = parameters[1].ToLower();

                            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT `ip_last`,`machine_id` FROM `users` where `username` = '" + User + "' LIMIT 1");
                                var Row = dbClient.getRow();

                                var IPLast = Convert.ToString(Row["ip_last"]);
                                var MachineID = Convert.ToString(Row["machine_id"]);

                                dbClient.RunQuery("DELETE FROM `bans` WHERE `value` = '" + User + "' OR `value` = '" + IPLast + "' OR `value` = '" + MachineID + "'");
                            }

                             log.Info("Desbanido com sucesso: '" + User + "'!");

                            break;
                        }
                    #endregion

                    #region dc
                    case "dc":
					case "desconectar":
                        {
                            #region Variables
                            string User = parameters[1].ToLower();
                            GameClient Session = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User);
                            #endregion

                            #region Conditions
                            if (Session == null)
                            {
                                log.Info("'" + User + "' está offline!");
                                return;
                            }
                            #endregion

                            Session.Disconnect(true);
                            log.Info("'" + User + "' desconectado com sucesso!");

                            break;
                        }
                    #endregion

                    #region senduser
                    case "senduser":
					case "uenviar":
					case "enviaru":
                        {
                            #region Variables
                            string User = parameters[1].ToLower();
                            GameClient Session = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User);

                            int RoomId = 1;
                            if (!int.TryParse(parameters[2], out RoomId))
                            {
                                log.Info("Quarto ID inválido!");
                                return;
                            }

                            RoomId = Convert.ToInt32(parameters[2]);
                            Room TargetRoom = HabboRoleplay.Misc.RoleplayManager.GenerateRoom(RoomId);
                            #endregion

                            #region Null checks
                            if (Session == null)
                            {
                                log.Info("'" + User + "' está offline!");
                                return;
                            }

                            if (Session.GetRoleplay() == null)
                            {
                                log.Info("'" + User + "' está offline!");
                                return;
                            }

                            if (Session.GetHabbo() == null)
                            {
                                log.Info("'" + User + "' está offline!");
                                return;
                            }

                            if (TargetRoom == null)
                            {
                                log.Info("'Quarto' está nulo!");
                            }
                            #endregion

                            #region Conditions
                            if (TargetRoom == Session.GetHabbo().CurrentRoom)
                            {
                                log.Info("Este usuário já está nesta sala!");
                                return;

                            }

                            if (Session.GetRoleplay().IsDead)
                            {
                                Session.GetRoleplay().IsDead = false;
                                Session.GetRoleplay().ReplenishStats();
                            }

                            if (Session.GetRoleplay().IsJailed)
                            {
                                Session.GetRoleplay().IsJailed = false;
                                Session.GetRoleplay().JailedTimeLeft = 0;
                            }
                            #endregion

                            RoleplayManager.SendUser(Session, RoomId, "Você foi enviado para o quarto " + TargetRoom.Name + " [Quarto ID: " + RoomId + "] por um administrador!");
                            log.Info("Enviou com sucesso: '" + Session.GetHabbo().Username + "' para o Quarto ID: '" + RoomId + "'");
                            break;
                        }
                    #endregion

                    #region kill
                    case "kill":
					case "matar":
                        {

                            #region Variables
                            string User = parameters[1].ToLower();
                            GameClient Session = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User);
                            #endregion

                            #region Null Checks
                            if (Session == null)
                            {
                                log.Info("'" + User + "' está offline!");
                                return;
                            }

                            if (Session.GetRoleplay() == null)
                            {
                                log.Info("'" + User + "' está offline!");
                                return;
                            }

                            if (Session.GetHabbo() == null)
                            {
                                log.Info("'" + User + "' está offline!");
                                return;
                            }
                            #endregion

                            Session.GetRoleplay().CurHealth = 0;
                            log.Info("Matou com sucesso: '" + Session.GetHabbo().Username + "'");

                            break;
                        }
                    #endregion


                    #endregion

                    #region Server Management

                    #region stop
                    case "deslig":
					case "shutdown":
					case "parar":
					case "stop":
                        {
                            Logging.DisablePrimaryWriting(true);
                            Logging.WriteLine("O servidor está salvando móveis de usuários, quartos, etc. ESPERE QUE O SERVIDOR FECHE, NÃO SALE DO PROCESSO NO GERADOR DE TAREFAS!!", ConsoleColor.Yellow);
                            PlusEnvironment.PerformShutDown(false);
                            break;
                        }
                    #endregion

                    #region refresh
                    case "update":
                    case "refresh":
					case "atualizar":
                        {

                            if (parameters.Length < 2)
                            {
                                Console.WriteLine("Comando inválido!: :atualizar <o que?>");
                                return;
                            }

                            string ToRefresh = parameters[1].ToLower();
                            
                            switch (ToRefresh)
                            {

                                #region p
                                case "ranks":
                                case "rights":
                                case "permissions":
								case "cargos":
								case "permissoes":
                                    {

                                        PlusEnvironment.GetGame().GetPermissionManager().Init();

                                        log.Info("Direitos atualizados com sucesso!");

                                        break;
                                    }
                                #endregion

                                #region rpbots
                                case "bots":
                                case "bot":
                                case "rpbots":
                                case "rpbot":
								case "robos":
                                    {

                                        log.Info("Successfully refreshed Roleplay bots!");
                                        RoleplayBotManager.Initialize(true);

                                        lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
                                        {
                                            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                                            {
                                                if (client == null || client.GetHabbo() == null)
                                                    continue;

                                                client.SendWhisper("[SISTEMA]Todos os BOTS foram atualizados!", 33);
                                            }
                                        }

                                    }
                                break;
                                #endregion

                                #region users
                                case "users":
								case "usuarios":
                                    {

                                        foreach(GameClient user in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                                        {
                                            if (user == null)
                                                continue;

                                            if (user.LoggingOut)
                                                continue;

                                            if (user.GetRoleplay() != null)
                                            {
                                                if (user.GetRoleplay().WebSocketConnection != null)
                                                    user.GetRoleplay().SendTopAlert("Todos os usuários foram atualizados, você será desconectado, entre novamente!");
                                            }

                                            user.Disconnect(false);
                                        }

                                        break;
                                    }
                                #endregion

                                #region chats
                                case "chats":
								case "wpp":
								case "whatsapp":
								case "grupos":
                                    {
                                        HabboRoleplay.Web.Util.ChatRoom.WebSocketChatManager.Initialiaze();
                                        break;
                                    }
                                #endregion

                                #region botwait
                                case "botwait":
								case "botesperando":
								case "espera":
								case "bespera":
                                    {

                                        foreach(RoomUser User in RoleplayBotManager.DeployedRoleplayBots.Values)
                                        {
                                            if (User == null)
                                                continue;

                                            if (User.GetBotRoleplay() == null)
                                                continue;

                                            log.Info(User.GetBotRoleplay().Name + " (Bot Esperando: " +
                                                User.GetBotRoleplay().RoomStayTime + "/" +
                                                User.GetBotRoleplay().RoomStayInterval + ") : (Contagem do Bot: " +
                                                User.GetBotRoleplay().RoamCooldown + ")" + " : (Bot Virtual ID: " + User.GetBotRoleplay().VirtualId + ")");
                                        }

                                    }
                                    break;
                                #endregion

                                #region commands
                                case "commands":
								case "comandos":
                                    {
                                        PlusEnvironment.GetGame().GetChatManager()._commands = new CommandManager(":");
                                        Console.WriteLine("Todos os comandos foram atualizados!", ConsoleColor.Yellow);
                                    }
                                    break;
                               #endregion
                            }
                        }
                        break;
                    #endregion

                    #region sockets
                    case "sockets":
					case "websockets":
					case "socket":
                        {
                            Logging.WriteLine("Contador Socket:" + PlusEnvironment.GetGame().GetWebEventManager()._webSockets.Count + "", ConsoleColor.Yellow);
                            string Append = "";

                            foreach (Fleck.IWebSocketConnection Connection in PlusEnvironment.GetGame().GetWebEventManager()._webSockets.Keys.ToList())
                            {
                                
                                Append += "WebSocket ID: " + PlusEnvironment.GetGame().GetWebEventManager().GetSocketsUserID(Connection) + "\n";
                                Append += "Socket disponível: " + Connection.IsAvailable.ToString() + "\n";
                                Append += "Socket caminho: " + Connection.ConnectionInfo.Path.Trim() + "\n";
                                Append += "Socket pronto: " + PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Connection).ToString() + "\n";
                                Append += "\n\n";

                            }


                            Logging.WriteLine(Append);
                        }
                        break;
                    #endregion
                    
                    #region sockets
                    case "chats":
                        {
                            Logging.WriteLine("TOTAL Contagem de grupos:" + HabboRoleplay.Web.Util.ChatRoom.WebSocketChatManager.RunningChatRooms.Count + "", ConsoleColor.Red);
                            string Append = "";

                            foreach (HabboRoleplay.Web.Util.ChatRoom.WebSocketChatRoom ChatRoom in HabboRoleplay.Web.Util.ChatRoom.WebSocketChatManager.RunningChatRooms.Values)
                            {
                                Append += "\n\nNome do Chat: " + ChatRoom.ChatName + "\n";
                                Append += "Dono do Chat: " + ChatRoom.ChatOwner + "\n";
                                Append += "Usuários: " + ChatRoom.ChatUsers.Count + "\n";
                                Append += "Chatlog: " + ChatRoom.ChatLogs.Count + "\n";
                                Append += "<--------Usuários-------->";
                                foreach(GameClient User in ChatRoom.ChatUsers.Keys)
                                {
                                    Append += "\nUsuário: " + User.GetHabbo().Username + "";
                                }
                                Append += "\n---------------------";
                            }


                            Logging.WriteLine(Append, ConsoleColor.Red);
                        }
                        break;
                    #endregion

                    #region clear
                    case "clear":
					case "limpar":
                        {
                            Console.Clear();
                            break;
                        }
                    #endregion

                    #region alert
                    case "alert":
					case "alerta":
					case "alertar":
                        {
                            string Notice = inputData.Substring(6);

                            PlusEnvironment.GetGame().GetClientManager().SendMessage(new BroadcastMessageAlertComposer(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("console.noticefromadmin") + "\n\n" + Notice));

                            log.Info("Alerta enviado com sucesso.");
                            break;
                        }
                    #endregion

                    #region furni and catalog commands

                    #region Catalog stuff
                    case "updatefurni":
					case "attmobis":
                        {
                            PlusEnvironment.GetGame().GetItemManager().UpdateFurniSpecial();
                            PlusEnvironment.GetGame().GetItemManager().ProductDataMaker();
                            PlusEnvironment.GetGame().GetItemManager().DownloadFurnis();
                            log.Info("Completamente atualizado todos os furni!");
                            break;
                        }
                    case "furnidata":
					case "attdata":
                        {
                            PlusEnvironment.GetGame().GetItemManager().UpdateFurniSpecial();
                            log.Info("Completamente atualizado todos os dados!.");
                            break;
                        }
                    case "furni":
					case "mobis":
                        {
                            PlusEnvironment.GetGame().GetItemManager().DownloadFurnis();
                            log.Info("Successfully updated database with furnidata.");
                            break;
                        }
                    case "productdata":
					case "cache":
                        {
                            PlusEnvironment.GetGame().GetItemManager().ProductDataMaker();
                            break;
                        }
                    #endregion

                    #region FurniFix2
                    case "furnifix2":
					case "fixmobis":
                        {
                            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT * FROM `furniture_old`");
                                DataTable Table = dbClient.getTable();

                                dbClient.SetQuery("SELECT * FROM `items`");
                                DataTable ToFix = dbClient.getTable();

                                int Count = 0;
                                int Count2 = 0;

                                if (Table != null && ToFix != null)
                                {
                                    Dictionary<int, int> Changes = new Dictionary<int, int>();

                                    int counter = 0;
                                    int counter2 = 0;

                                    foreach (DataRow Row in Table.Rows)
                                    {
                                        string Name = Convert.ToString(Row["item_name"]);
                                        int Id = Convert.ToInt32(Row["id"]);

                                        ItemData Data = null;
                                        if (!PlusEnvironment.GetGame().GetItemManager().GetItem(Name, out Data))
                                            continue;

                                        if (Changes.ContainsKey(Id))
                                            continue;

                                        counter++;
                                        Changes.Add(Id, Data.Id);

                                        if (counter > 100)
                                        {
                                            counter2++;
                                            counter = 0;
                                            Console.WriteLine("Anotação: " + (counter2 * 100) + " itens adicionados até agora");
                                        }
                                    }
                                    Console.WriteLine("Fez a anotaçãos");

                                    StringBuilder String = new StringBuilder();

                                    foreach (DataRow Row in ToFix.Rows)
                                    {
                                        int DataId = Convert.ToInt32(Row["id"]);
                                        int UserId = Convert.ToInt32(Row["user_id"]);
                                        int RoomId = Convert.ToInt32(Row["room_id"]);
                                        int BaseItem = Convert.ToInt32(Row["base_item"]);
                                        string ExtraData = Row["extra_data"].ToString();
                                        int X = Convert.ToInt32(Row["x"]);
                                        int Y = Convert.ToInt32(Row["y"]);
                                        double Z = Convert.ToDouble(Row["z"]);
                                        int Rot = Convert.ToInt32(Row["rot"]);
                                        string WallPos = Row["wall_pos"].ToString();
                                        int LimitedNum = Convert.ToInt32(Row["limited_number"]);
                                        int LimitedStack = Convert.ToInt32(Row["limited_stack"]);

                                        if (Changes.ContainsKey(BaseItem))
                                        {
                                            Count++;

                                            if (Count >= 100)
                                            {
                                                Count = 0;
                                                Count2++;
                                                Console.WriteLine("Atualizado " + (Count2 * 100) + " items até agora!");
                                            }

                                            String.Append("INSERT INTO `items_new` VALUES ('" + DataId + "','" + UserId + "','" + RoomId + "','" + Changes[BaseItem] + "','" + ExtraData + "','" + X + "','" + Y + "','" + Z + "','" + Rot + "','" + WallPos + "','" + LimitedNum + "','" + LimitedStack + "');\n");
                                        }
                                    }
                                    ConsoleWriter.Writer.WriteProductData(String.ToString());
                                }
                                Console.WriteLine("Terminado!");
                            }
                            break;
                        }
                    #endregion

                    #region FurniFix
                    case "furnifix":
					case "mobifix":
                        {
                            Dictionary<int, int> Changes = new Dictionary<int, int>();

                            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT * FROM `furniture_fix`");
                                DataTable Table = dbClient.getTable();

                                if (Table != null)
                                {
                                    foreach (DataRow Row in Table.Rows)
                                    {
                                        string Name = Convert.ToString(Row["item_name"]);
                                        int OldId = Convert.ToInt32(Row["id"]);
                                        int NewId = Convert.ToInt32(Row["new_id"]);

                                        if (NewId == 0 && !Changes.ContainsKey(OldId))
                                        {
                                            dbClient.SetQuery("SELECT * FROM `furniture` WHERE `item_name` = '" + Name.ToLower() + "' LIMIT 1");
                                            DataRow Data = dbClient.getRow();

                                            if (Data != null)
                                            {
                                                int RealNewId = Convert.ToInt32(Data["id"]);

                                                Changes.Add(OldId, RealNewId);
                                            }
                                        }
                                    }
                                    Console.WriteLine("terminado na anotação");

                                    foreach (var Pair in Changes)
                                    {
                                        dbClient.RunQuery("UPDATE `furniture_fix` SET `new_id` = '" + Pair.Value + "' WHERE `id` = '" + Pair.Key + "' LIMIT 1");
                                    }

                                    Console.WriteLine("TABELA DE FIXAÇÃO DE MOBILIÁRIO FINALIZADA");
                                }
                            }
                            break;
                        }
                    #endregion

                    #endregion

                    #endregion

                    #region Default
                    default:
                        {
                            log.Error(parameters[0].ToLower() + " é um comando desconhecido ou não suportado. Digite ajuda para obter mais informações");
                            break;
                        }
                    #endregion
                }
                #endregion
            }
            catch (Exception e)
            {
                log.Error("Erro no comando [" + inputData + "]: " + e);
            }
        }
    }
}