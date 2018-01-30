using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Plus.Core;

using Plus.HabboHotel;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.UserDataManagement;
using Plus.Communication.Packets.Incoming;

using Plus.Net;
using Plus.Utilities;
using log4net;

using System.Data;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Encryption.Keys;
using Plus.Communication.Encryption;

using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache;
using Plus.Database;

using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Farming;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using System.Net;
using System.Text.RegularExpressions;
using Plus.HabboHotel.Rooms.Chat;
using Plus.HabboRoleplay.Web.Util.ChatRoom;

namespace Plus
{
    public static class PlusEnvironment
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.PlusEnvironment");

        public const string PrettyVersion = "Plus Emulador";
        public const string PrettyBuild = "3.4.3.0 [HabboRPG]";

        private static ConfigurationData _configuration;
        private static Encoding _defaultEncoding;
        private static ConnectionHandling _connectionManager;
        private static Game _game;
        private static DatabaseManager _manager;
        public static ConfigData ConfigData;
        public static MusSocket MusSystem;
        public static CultureInfo CultureInfo;

        public static bool Event = false;
        public static DateTime lastEvent;
        public static DateTime ServerStarted;

        private static readonly List<char> Allowedchars = new List<char>(new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
                'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
                'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.'
            });

        private static ConcurrentDictionary<int, Habbo> _usersCached = new ConcurrentDictionary<int, Habbo>();

        public static string SWFRevision = "";

        public static void Initialize()
        {
            ServerStarted = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine();
            Console.WriteLine("                     ____  __           ________  _____  __");
            Console.WriteLine(@"                    / __ \/ /_  _______/ ____/  |/  / / / /");
            Console.WriteLine("                   / /_/ / / / / / ___/ __/ / /|_/ / / / / ");
            Console.WriteLine("                  / ____/ / /_/ (__  ) /___/ /  / / /_/ /  ");
            Console.WriteLine(@"                 /_/   /_/\__,_/____/_____/_/  /_/\____/ ");

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("                                " + PrettyVersion + " <Feito por " + PrettyBuild + ">");
            Console.WriteLine("                                suamae");

            Console.WriteLine("");
            Console.Title = "Plus Emulador [Carregando...]";
            _defaultEncoding = Encoding.Default;

            Console.WriteLine("");
            Console.WriteLine("");

            CultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");


            try
            {

                _configuration = new ConfigurationData(Path.Combine(Application.StartupPath, @"config.ini"));

                var connectionString = new MySqlConnectionStringBuilder
                {
                    ConnectionTimeout = 60,
                    Database = GetConfig().data["db.name"],
                    DefaultCommandTimeout = 120,
                    Logging = false,
                    MaximumPoolSize = uint.Parse(GetConfig().data["db.pool.maxsize"]),
                    MinimumPoolSize = uint.Parse(GetConfig().data["db.pool.minsize"]),
                    Password = GetConfig().data["db.password"],
                    Pooling = true,
                    Port = uint.Parse(GetConfig().data["db.port"]),
                    Server = GetConfig().data["db.hostname"],
                    UserID = GetConfig().data["db.username"],
                    AllowZeroDateTime = true,
                    ConvertZeroDateTime = true,
                };

                _manager = new DatabaseManager(connectionString.ToString());

                if (!_manager.IsConnected())
                {
                    log.Error("Falha ao conectar ao específico servidor MySQL.");
                    Console.ReadKey(true);
                    Environment.Exit(1);
                    return;
                }


                log.Info("Conectado a database!");

                //Reset our statistics first.
                int randomInt = ((System.Diagnostics.Debugger.IsAttached) ? 0 : 1);
                using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                    dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0';");
                    dbClient.RunQuery("UPDATE `users` SET `online` = '0' WHERE `online` = '1'");
                    dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0', `environment_status` = '" + randomInt + "'");
                }

                //Get the configuration & Game set.
                ConfigData = new ConfigData();
                _game = new Game();

                //Have our encryption ready.
                HabboEncryptionV2.Initialize(new RSAKeys());

                //Make sure MUS is working.
                MusSystem = new MusSocket(GetConfig().data["mus.tcp.bindip"], int.Parse(GetConfig().data["mus.tcp.port"]), GetConfig().data["mus.tcp.allowedaddr"].Split(Convert.ToChar(";")), 0);

                //Accept connections.
                _connectionManager = new ConnectionHandling(int.Parse(GetConfig().data["game.tcp.port"]), int.Parse(GetConfig().data["game.tcp.conlimit"]), int.Parse(GetConfig().data["game.tcp.conperip"]), GetConfig().data["game.tcp.enablenagles"].ToLower() == "true");
                _connectionManager.init();

                _game.StartGameLoop();
            
                TimeSpan TimeUsed = DateTime.Now - ServerStarted;

                Console.WriteLine();

                log.Info("EMULATOR -> READY! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");
            }
            catch (KeyNotFoundException e)
            {
                Logging.WriteLine("Verifique o seu arquivo de configuração - alguns valores parecem estar faltando.", ConsoleColor.Red);
                Logging.WriteLine("Pressione qualquer tecla para desligar...");
                Logging.WriteLine(e.ToString());
                Console.ReadKey(true);
                Environment.Exit(1);
                return;
            }
            catch (InvalidOperationException e)
            {
                Logging.WriteLine("Falha ao inicializar Plus Emulador: " + e.Message, ConsoleColor.Red);
                Logging.WriteLine("Pressione qualquer tecla para desligar...");
                Console.ReadKey(true);
                Environment.Exit(1);
                return;
            }
            catch (Exception e)
            {
                Logging.WriteLine("Erro fatal para iniciar: " + e, ConsoleColor.Red);
                Logging.WriteLine("Pressione qualquer tecla para fechar.");

                Console.ReadKey();
                Environment.Exit(1);
            }


            RoleplayBotManager.Initialize(false);

        }

        public static bool EnumToBool(string Enum)
        {
            return (Enum == "1");
        }

        public static string BoolToEnum(bool Bool)
        {
            return (Bool == true ? "1" : "0");
        }

        public static int GetRandomNumber(int Min, int Max)
        {
            return RandomNumber.GenerateNewRandom(Min, Max);
        }

        public static double GetUnixTimestamp()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return ts.TotalSeconds;
        }

        public static long Now()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            double unixTime = ts.TotalMilliseconds;
            return (long)unixTime;
        }

        public static string FilterFigure(string figure)
        {
            foreach (char character in figure)
            {
                if (!isValid(character))
                    return "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62";
            }

            return figure;
        }

        private static bool isValid(char character)
        {
            return Allowedchars.Contains(character);
        }

        public static bool IsValidAlphaNumeric(string inputStr)
        {
            inputStr = inputStr.ToLower();
            if (string.IsNullOrEmpty(inputStr))
            {
                return false;
            }

            for (int i = 0; i < inputStr.Length; i++)
            {
                if (!isValid(inputStr[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetUsernameById(int UserId)
        {
            string Name = "Usuário desconhecido";

            GameClient Client = GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client != null && Client.GetHabbo() != null)
                return Client.GetHabbo().Username;

            using (UserCache User = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(UserId))
            {
                if (User != null)
                    return User.Username;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", UserId);
                Name = dbClient.getString();
            }

            if (string.IsNullOrEmpty(Name))
                Name = "Usuário desconhecido";

            return Name;
        }

        public static Habbo GetHabboById(int UserId)
        {
            try
            {
                if (GetGame() == null)
                    return null;

                if (GetGame().GetClientManager() == null)
                    return null;

                GameClient Client = GetGame().GetClientManager().GetClientByUserID(UserId);

                if (Client != null)
                {
                    Habbo User = Client.GetHabbo();
                    if (User != null && User.Id > 0)
                    {
                        if (_usersCached.ContainsKey(UserId))
                            _usersCached.TryRemove(UserId, out User);
                        return User;
                    }
                }
                else
                {
                    try
                    {
                        if (_usersCached.ContainsKey(UserId))
                            return _usersCached[UserId];
                        else
                        {
                            UserData data = UserDataFactory.GetUserData(UserId);
                            if (data != null)
                            {
                                Habbo Generated = data.user;
                                if (Generated != null)
                                {
                                    Generated.InitInformation(data);
                                    _usersCached.TryAdd(UserId, Generated);
                                    return Generated;
                                }
                            }
                        }
                    }
                    catch { return null; }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static Habbo GetHabboByUsername(String UserName)
        {
            try
            {
                using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @user LIMIT 1");
                    dbClient.AddParameter("user", UserName);
                    int id = dbClient.getInteger();
                    if (id > 0)
                        return GetHabboById(Convert.ToInt32(id));
                }
                return null;
            }
            catch { return null; }
        }



        public static void PerformShutDown(bool Crashed = true)
        {
            Console.Clear();
            log.Info("Servidor desligando...");
            Console.Title = "Plus Emulador: Desligando!";

            if (!Crashed)
                PlusEnvironment.GetGame().GetClientManager().SendMessage(new Communication.Packets.Outgoing.Rooms.Notifications.RoomNotificationComposer("[Alerta de Manutenção]", PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("shutdown_alert"), "disconnection", "ok", "event:"));
            else
                PlusEnvironment.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer("[Alerta de Manutenção]", PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("shutdown_alert_crash"), "disconnection", "ok", "event:"));
            GetGame().StopGameLoop();

            Thread.Sleep(Crashed ? 6500 : 2500);

            GetConnectionManager().Destroy();//Stop listening.

            GetGame().GetPacketManager().UnregisterAll();//Unregister the packets.
            GetGame().GetPacketManager().WaitForAllToComplete();

            FarmingManager.UpdateAllFarmingSpaces();

            RoleplayManager.TimerManager.EndAllTimers();
            RoleplayManager.TimerManager = null;

            GetGame().GetClientManager().CloseAll();//Close all connections
            GetGame().GetRoomManager().Dispose();//Stop the game loop.

            using (IQueryAdapter dbClient = _manager.GetQueryReactor())
            {
                dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                dbClient.RunQuery("UPDATE `users` SET online = '0', `auth_ticket` = NULL");
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0'");
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0', `environment_status` = '" + (Crashed ? 3 : 0) + "'");
            }

            WebSocketChatManager.StopAllChats();

            log.Info("Plus Emulador desligado com sucesso.");

            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        public static ConfigurationData GetConfig()
        {
            return _configuration;
        }

        public static ConfigData GetDBConfig()
        {
            return ConfigData;
        }

        public static Encoding GetDefaultEncoding()
        {
            return _defaultEncoding;
        }

        public static ConnectionHandling GetConnectionManager()
        {
            return _connectionManager;
        }

        public static Game GetGame()
        {
            return _game;
        }

        public static DatabaseManager GetDatabaseManager()
        {
            return _manager;
        }

        public static ICollection<Habbo> GetUsersCached()
        {
            return _usersCached.Values;
        }

        public static bool RemoveFromCache(int Id, out Habbo Data)
        {
            return _usersCached.TryRemove(Id, out Data);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static Int64 DateTimeToUnixTimeStamp(DateTime target)
        {
            var d = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((target - d).TotalSeconds);
        }

        /// <summary>
        /// Translate Text using Google Translate API’s
        /// Google URL – http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}
        /// </summary>
        /// <param name=”input”>Input string</param>
        /// <param name=”languagePair”>2 letter Language Pair, delimited by “|”.
        /// E.g. “ar|en” language pair means to translate from Arabic to English</param>
        /// <returns>Translated to String</returns>
        public static string TranslateText(string input, string languagePair)
        {
            try
            {
                input = input.Replace(".", ",").Replace("!", ",") /*.Replace("/", ",").Replace("\\", ",").Replace("<", ",").Replace(">", ",").Replace(")", ",").Replace("(", ",").Replace("*", ",")*/;
                
                // Decode from UTF-8
                byte[] bytes = Encoding.Default.GetBytes(input);
                input = Encoding.GetEncoding(1252).GetString(bytes);

                string URL = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", input, languagePair);
                string Result;

                using (WebClient webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.GetEncoding(1252);

                    Result = webClient.DownloadString(URL);
                    Result = Regex.Split(Result, "<span id=result_box")[1];
                    Result = Regex.Split(Result, "</span>")[0];
                    Result = Regex.Split(Result, "#fff'\">")[1];
                }

                return Result.Trim();
            }
            catch
            {
                return input;
            }
        }
    }
}