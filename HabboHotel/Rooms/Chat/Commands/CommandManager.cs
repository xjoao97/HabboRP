using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;

using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.GameClients;

using Plus.HabboHotel.Rooms.Chat.Commands.Users.Events;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Apartment;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Hospital;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Clothing;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Restaurant;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Criminal;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Banking;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Police;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Timers;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Purchasing;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Toggles;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Marriage;

using Plus.HabboHotel.Rooms.Chat.Commands.VIP;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderators;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Trials;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors;
using Plus.HabboHotel.Rooms.Chat.Commands.Administrators;
using Plus.HabboHotel.Rooms.Chat.Commands.Managers;
using Plus.HabboHotel.Rooms.Chat.Commands.Developers;
using Plus.HabboHotel.Rooms.Chat.Commands.Owners;
using Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Basic;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Bounties;
using Plus.HabboHotel.Rooms.Chat.Commands.Ambassadors;

namespace Plus.HabboHotel.Rooms.Chat.Commands
{
    public class CommandManager
    {
        /// <summary>
        /// Command Prefix only applies to custom commands.
        /// </summary>
        private string _prefix = ":";

        /// <summary>
        /// Commands registered for use.
        /// </summary>
        private readonly Dictionary<string, IChatCommand> _commands;
        private readonly Dictionary<string, IChatCommand> _jobcommands;
        private readonly Dictionary<string, IChatCommand> _gangcommands;
        private readonly Dictionary<string, IChatCommand> _staffcommands;
        private readonly Dictionary<string, IChatCommand> _ambassadorcommands;
        private readonly Dictionary<string, IChatCommand> _loggedcommands;
        private readonly Dictionary<string, IChatCommand> _vipcommands;
        private readonly Dictionary<string, IChatCommand> _eventcommands;
        private List<string> _aliases;

        /// <summary>
        /// The default initializer for the CommandManager
        /// </summary>
        public CommandManager(string Prefix)
        {
            this._prefix = Prefix;
            this._commands = new Dictionary<string, IChatCommand>();
            this._jobcommands = new Dictionary<string, IChatCommand>();
            this._gangcommands = new Dictionary<string, IChatCommand>();
            this._ambassadorcommands = new Dictionary<string, IChatCommand>();
            this._staffcommands = new Dictionary<string, IChatCommand>();
            this._loggedcommands = new Dictionary<string, IChatCommand>();
            this._vipcommands = new Dictionary<string, IChatCommand>();
            this._eventcommands = new Dictionary<string, IChatCommand>();
            this._aliases = new List<string>();

            this.RegisterUsers();
            this.RegisterUsersGangs();
            this.RegisterUsersJobs();
            this.RegisterVIP();
            this.RegisterAmbassadors();
            this.RegisterTrialModerators();
            this.RegisterModerators();
            this.RegisterSeniorModerators();
            this.RegisterAdministrators();
            this.RegisterManagers();
            this.RegisterDevelopers();
            this.RegisterOwners();
            this.RegisterSpecialRights();
        }

        /// <summary>
        /// Request the text to parse and check for commands that need to be executed.
        /// </summary>
        /// <param name="Session">Session calling this method.</param>
        /// <param name="Message">The message to parse.</param>
        /// <returns>True if parsed or false if not.</returns>
        public bool Parse(GameClient Session, string Message)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return false;

            if (!Message.StartsWith(_prefix))
                return false;

            #region Commands List

            #region :commands
            if (Message.ToLower() == _prefix + "comandos")
            {
                StringBuilder List = new StringBuilder();
                List.Append("Esta é a lista de comandos que você tem disponível:\n\n");
                List.Append(":Tcomandos - Lista de todos os comandos de Trabalho.\n");
                List.Append(":Gcomandos - Lista de todos os comandos de Gangues.\n");
                List.Append(":Vcomandos - Lista de todos os comandos de VIP.\n");
                List.Append(":Scomandos - Lista de todos os comandos de STAFF.\n\n");

                foreach (var CmdList in _commands.ToList())
                {
                    if (_aliases.Contains(CmdList.Key.ToLower()))
                        continue;

                    if (!string.IsNullOrEmpty(CmdList.Value.PermissionRequired))
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand(CmdList.Value.PermissionRequired))
                            continue;
                    }

                    List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                }
                Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                return true;
            }
            #endregion
             
			 
			 
			#region :jobcommands
            if (Message.ToLower() == _prefix + "tcomandos")
            {
                StringBuilder List = new StringBuilder();
                List.Append("Estes são ALGUNS dos comandos de trabalho disponíveis.\n\n");
                foreach (var CmdList in _jobcommands.ToList())
                {
                    if (_aliases.Contains(CmdList.Key.ToLower()))
                        continue;

                    if (CmdList.Key == "trabalhar" || CmdList.Key == "ptrabalhar" || CmdList.Key == "corplista" || CmdList.Key == "corpinfo")
                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    else if (CmdList.Key == "promover" || CmdList.Key == "rebaixar")
                    {
                        if (Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                            List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                    else if (CmdList.Key == "radio" || CmdList.Key == "adradio")
                    {
                        if (Session.GetHabbo().GetPermissions().HasRight("corporation_rights") || Groups.GroupManager.HasJobCommand(Session, CmdList.Key.ToLower()))
                            List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                    else
                    {
                        if (Groups.GroupManager.HasJobCommand(Session, CmdList.Key.ToLower()))
                            List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                }
                Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                return true;
            }
            #endregion

            #region :gangcommands
            if (Message.ToLower() == _prefix + "gcomandos")
            {
                StringBuilder List = new StringBuilder();
                List.Append("Esta é a lista de comandos de gangues que você tem disponível:\n\n");
                foreach (var CmdList in _gangcommands.ToList())
                {
                    if (_aliases.Contains(CmdList.Key.ToLower()))
                        continue;

                    if (CmdList.Key == "ginfo" || CmdList.Key == "glista" || CmdList.Key == "territorios" || CmdList.Key == "gcriar" || CmdList.Key == "gsaiar")
                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    else if (CmdList.Key == "gcapturar" && Session.GetRoleplay().GangId > 1000)
                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    else if (Groups.GroupManager.HasGangCommand(Session, CmdList.Key))
                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                }
                Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                return true;
            }
            #endregion

            #region :staffcommands
            if (Message.ToLower() == _prefix + "scomandos")
            {
                if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Session.GetHabbo().VIPRank < 2)
                {
                    Session.SendWhisper("Você não é um membro da equipe!", 1);
                    return true;
                }
                else
                {
                    StringBuilder List = new StringBuilder();
                    List.Append("Esta é a lista de comandos Staffs que você tem disponível:\n\n");
                    foreach (var CmdList in _staffcommands.ToList())
                    {
                        if (_aliases.Contains(CmdList.Key.ToLower()))
                            continue;

                        if (!string.IsNullOrEmpty(CmdList.Value.PermissionRequired))
                        {
                            if (!Session.GetHabbo().GetPermissions().HasCommand(CmdList.Value.PermissionRequired) && Session.GetHabbo().VIPRank < 2)
                                continue;
                        }

                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                    Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                    return true;
                }
            }
            #endregion

            #region :vipcommands
            if (Message.ToLower() == _prefix + "vcomandos")
            {
                if (Session.GetHabbo().VIPRank <= 0 && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                {
                    Session.SendWhisper("Você não tem VIP!", 1);
                    return true;
                }
                else
                {
                    StringBuilder List = new StringBuilder();
                    List.Append("Esta é a lista de comandos VIP que você possui:\n\n");
                    foreach (var CmdList in _vipcommands.ToList())
                    {
                        if (_aliases.Contains(CmdList.Key.ToLower()))
                            continue;

                        List.Append(":" + CmdList.Key + " " + CmdList.Value.Parameters + " - " + CmdList.Value.Description + "\n");
                    }
                    Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                    return true;
                }
            }
            #endregion

            #endregion

            Message = Message.Substring(1);
            string[] Split = Message.Split(' ');

            if (Split.Length == 0)
                return false;

            IChatCommand Cmd = null;
            IChatCommand LogCmd = null;
            if (_commands.TryGetValue(Split[0].ToLower(), out Cmd))
            {
                _loggedcommands.TryGetValue(Split[0].ToLower(), out LogCmd);

                if (Cmd == LogCmd)
                {
                    if (_staffcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "staff");
                    else if (_ambassadorcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "embaixador");
                    else if (_jobcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "trabalho");
                    else if (_vipcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "vip");
                    else if (_eventcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "evento");
                    else
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "usuario");
                }
                if (!string.IsNullOrEmpty(Cmd.PermissionRequired))
                {
                    if (Split[0].ToLower() == "empurrar")
                    {
                        if (Session.GetRoleplay().Game == null || Session.GetRoleplay().Team == null)
                        {
                            if (!Session.GetHabbo().GetPermissions().HasCommand(Cmd.PermissionRequired))
                                return false;
                        }
                    }
                    else
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand(Cmd.PermissionRequired))
                            return false;
                    }
                }

                Session.GetHabbo().IChatCommand = Cmd;
                Session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, Session.GetHabbo(), this);

                Cmd.Execute(Session, Session.GetHabbo().CurrentRoom, Split);
                return true;
            }
            return false;
        }

        #region Commands
        /// <summary>
        /// User set of commands
        /// </summary>
        private void RegisterUsers()
        {
            // Geral
			this.Register("rpstatus", new RPStatsCommand());
            this.Register("ialerta", new ChangeLogCommand());
            this.Register("gamemapa", new GameMapCommand());
            this.Register("mapa", new GameMapCommand(), "", true);
            this.Register("status", new StatsCommand());
            this.Register("sagricultura", new FarmingStatsCommand());
            this.Register("agricultura", new FarmingStatsCommand(), "", true);
            this.Register("armas", new WeaponsCommand());
            this.Register("whosonline", new OnlineCommand());
            this.Register("onlines", new OnlineCommand(), "", true);
            this.Register("populares", new HotRoomsCommand());
            this.Register("cheios", new HotRoomsCommand(), "", true);
            this.Register("taxi", new TaxiCommand());
            this.Register("ptaxi", new StopTaxiCommand());
            this.Register("quartoid", new RoomIDCommand());
            this.Register("quartoinfo", new RoomInfoCommand());
            this.Register("attmapa", new RegenMapsCommand());
            this.Register("limparinventario", new EmptyItemsCommand());
            this.Register("attroupa", new PoofCommand());
            this.Register("quit", new LogOutCommand());
            this.Register("mudarclasse", new ChangeClassCommand());
            this.Register("setas", new ArrowCommand());
            this.Register("andar", new ArrowCommand());

            // Tempos e Contagens
            this.Register("temporestante", new TimeLeftCommand());
            this.Register("contagens", new CooldownsCommand());
            this.Register("cds", new CooldownsCommand(), "", true);
            this.Register("cd", new CooldownsCommand(), "", true);

            // Banco
            this.Register("saldo", new BalanceCommand());
            this.Register("depositar", new DepositCommand());
            this.Register("retirar", new WithdrawCommand());

            // Atividade Criminosa
            this.Register("leis", new LawsCommand());
            this.Register("roubar", new RobCommand());
            this.Register("cheirar", new SnortCommand());
            this.Register("dispor", new DisposeCommand());
            this.Register("fumar", new SmokeCommand());

            // Compras de mercadorias
            this.Register("comprarbalas", new BuyBulletsCommand());
            this.Register("balas", new BuyBulletsCommand(), "", true);
            this.Register("comprarcreditos", new BuyCreditCommand());
            this.Register("creditos", new BuyCreditCommand(), "", true);
            this.Register("comprargasolina", new BuyFuelCommand());
            this.Register("gasolina", new BuyFuelCommand(), "", true);
            this.Register("comprarticket", new BuyTicketCommand(), "userlog");
            this.Register("ticket", new BuyTicketCommand(), "userlog", true);

            // Combate
            this.Register("modocombate", new CombatModeCommand());
            this.Register("modoc", new CombatModeCommand(), "", true);
            this.Register("soco", new HitCommand());
            this.Register("equipar", new EquipCommand());
            this.Register("usar", new EquipCommand(), "", true);
            this.Register("desequipar", new UnEquipCommand());
            this.Register("tirar", new UnEquipCommand(), "", true);
            this.Register("atirar", new ShootCommand());
            this.Register("recarregar", new ReloadGunCommand());

            // Ofertas
            this.Register("dar", new GiveCommand(), "userlog");
            this.Register("ofertas", new OffersCommand());
            this.Register("oferecer", new OfferCommand());
            this.Register("aceitar", new AcceptCommand());
            this.Register("recusar", new DeclineCommand());

            // Relacionado com a polícia
            this.Register("policia", new CallPoliceCommand());
            this.Register("190", new CallPoliceCommand(), "", true);
            this.Register("fianca", new BailCommand());
            this.Register("serender", new SurrenderCommand());
            this.Register("procurados", new WantedListCommand());
            this.Register("lp", new WantedListCommand(), "", true);
            this.Register("escoltar", new EscortCommand(), "");

            // Tribunal
            this.Register("julgamento", new TrialCommand());
            this.Register("votar", new VoteCommand());

            // Ativar/Desativar coisas
            this.Register("adtextos", new ToggleTextsCommand());
            this.Register("dsussurros", new DisableWhispersCommand());
            this.Register("dcopiar", new DisableMimicCommand());

            // Suas interações
            this.Register("dirigir", new DriveCommand());
            this.Register("sentar", new SitCommand());
            this.Register("levantar", new StandCommand());
            this.Register("deitar", new LayCommand());
            this.Register("danca", new DanceCommand());
            this.Register("atencao", new MeCommand());

            // Interação de itens
            this.Register("pedirentrega", new CallDeliveryCommand());
            this.Register("comer", new EatCommand());
            this.Register("beber", new DrinkCommand());
            this.Register("malhar", new WorkoutCommand());
            this.Register("plantar", new PlaceCommand());
            this.Register("colocar", new PlaceCommand());
            this.Register("explodir", new PlaceCommand());
            this.Register("reparar", new PlaceCommand());

            // Interação de casamento
            this.Register("casar", new MarryCommand());
            this.Register("propor", new MarryCommand(), "", true);
            this.Register("divorciar", new DivorceCommand());
            this.Register("sexo", new SexCommand());

            // Interação do usuário
            this.Register("tapa", new SlapCommand());
            this.Register("beijar", new KissCommand());
            this.Register("abracar", new HugCommand());
            this.Register("estuprar", new RapeCommand());

            // Apartmento
            this.Register("expulsar", new KickCommand());
            this.Register("qexpulsar", new RoomKickCommand());
            this.Register("pegartudo", new PickAllCommand(), "userlog");
            this.Register("vender", new SetPriceommand(), "userlog");

            // Eventos
            this.Register("loja", new PurchaseEventCommand(), "eventlog");
            this.Register("lojape", new PurchaseEventCommand(), "eventlog", true);
            this.Register("comprar", new PurchaseEventCommand(), "eventlog", true);
            this.Register("sairjogo", new LeaveGameCommand(), "eventlog");
            this.Register("jsair", new LeaveGameCommand(), "eventlog", true);
            this.Register("alertatime", new TeamAlertCommand(), "eventlog");
            this.Register("timea", new TeamAlertCommand(), "eventlog", true);
            this.Register("alertat", new TeamAlertCommand(), "eventlog", true);
            this.Register("capturar", new CaptureCommand(), "eventlog");

            // Jogos de Aposta
            this.Register("apostar", new GamblingCommand(), "eventlog");
            this.Register("passar", new GamblingCommand(), "eventlog", true);

            // Evento Soloqueue
            this.Register("soloqueue", new SoloQueueCommand(), "eventlog");
            this.Register("joinqueue", new SoloQueueCommand(), "eventlog", true);
            this.Register("solo", new SoloQueueCommand(), "eventlog", true);

            // Recompensas
            this.Register("defrecompensa", new AddBountyCommand(), "userlog");
            this.Register("recompensa", new AddBountyCommand(), "userlog", true);
            this.Register("addrecompensa", new AddBountyCommand(), "userlog", true);
            this.Register("addr", new AddBountyCommand(), "userlog", true);
            this.Register("removerrecompensa", new RemoveBountyCommand(), "userlog");
            this.Register("rrecomepensa", new RemoveBountyCommand(), "userlog", true);
            this.Register("listar", new BountyListCommand(), "");
            this.Register("lrecompensa", new BountyListCommand(), "", true);
            this.Register("listarecompensa", new BountyListCommand(), "", true);

            // Traduções
            this.Register("traduzir", new TranslateCommand());
            this.Register("traduz", new TranslateCommand(), "", true);
            this.Register("ptraduzir", new StopTranslateCommand());
            this.Register("ptraduz", new StopTranslateCommand(), "", true);
            this.Register("ptd", new StopTranslateCommand(), "", true);

            // Diversos
            this.Register("fazerchat", new MakeChatCommand(), "userlog", true);
            this.Register("entrarchat", new JoinChatCommand(), "userlog", true);
            this.Register("chats", new ChatsCommand(), "userlog", true);
            this.Register("baixar", new DownloadAppCommand(), "userlog", true);
            this.Register("ajuda", new HelpCommand());
            this.Register("ajudaemb", new AmbassadorHelpCommand());
            this.Register("montar", new RideCommand());

        }

        /// <summary>
        /// User set of gang commands
        /// </summary>
        private void RegisterUsersGangs()
        {
            this.Register("ginfo", new GangInfoCommand(), "gang");
            this.Register("glista", new GangListCommand(), "gang");
            this.Register("territorios", new GangTurfsCommand(), "gang");
            this.Register("gcriar", new GangCreateCommand(), "gang");
            this.Register("gconvidar", new GangInviteCommand(), "gang");
            this.Register("gsair", new GangLeaveCommand(), "gang");
            this.Register("gcapturar", new GangCaptureCommand(), "gang");
            this.Register("gajuda", new GangBackupCommand(), "gang");
            this.Register("gcargo", new GangRankCommand(), "gang");
            this.Register("gtransferir", new GangTransferCommand(), "gang");
            this.Register("gcurar", new GangHealCommand(), "gang");
        }

        /// <summary>
        /// User set of job commands
        /// </summary>
        private void RegisterUsersJobs()
        {
            // General
            this.Register("trabalhar", new StartWorkCommand(), "joblog");
            this.Register("ptrabalhar", new StopWorkCommand(), "joblog");
            this.Register("corplista", new CorpListCommand(), "joblog");
            this.Register("elista", new CorpListCommand(), "joblog", true);
            this.Register("corpinfo", new CorpInfoCommand(), "joblog");
            this.Register("einfo", new CorpInfoCommand(), "joblog", true);
            this.Register("promover", new PromoteCommand(), "joblog");
            this.Register("rebaixar", new DemoteCommand(), "joblog");
            this.Register("ecasa", new SendhomeCommand(), "joblog");
            this.Register("contratar", new HireCommand(), "joblog");
            this.Register("demitir", new FireCommand(), "joblog");
            this.Register("checarminutos", new CheckMinutesCommand(), "joblog");
            this.Register("checarmins", new CheckMinutesCommand(), "joblog", true);

            // Hospital
            this.Register("reviver", new DischargeCommand(), "joblog");
            this.Register("curar", new HealCommand(), "joblog");

            // Policia
            this.Register("radio", new RadioAlertCommand(), "joblog");
            this.Register("ra", new RadioAlertCommand(), "joblog", true);
            this.Register("r", new RadioAlertCommand(), "joblog", true);
            this.Register("adradio", new ToggleRadioAlertCommand(), "joblog");
            this.Register("desativarradio", new ToggleRadioAlertCommand(), "joblog", true);
            this.Register("rdesligar", new ToggleRadioAlertCommand(), "joblog", true);
            this.Register("estrelas", new LawCommand(), "joblog");
            this.Register("restrelas", new UnLawCommand(), "joblog");
            this.Register("atordoar", new StunCommand(), "joblog");
            this.Register("desatordoar", new UnStunCommand(), "joblog");
            this.Register("pulverizar", new StunCommand(), "joblog");
            this.Register("despulverizar", new UnStunCommand(), "joblog");
            this.Register("algemar", new CuffCommand(), "joblog");
            this.Register("desalgemar", new UnCuffCommand(), "joblog");
            this.Register("procurar", new SearchCommand(), "joblog");
            this.Register("prender", new ArrestCommand(), "joblog");
            this.Register("libertar", new ReleaseCommand(), "joblog");
            this.Register("ptreinar", new PoliceTrialCommand(), "joblog");
            this.Register("ptreino", new PoliceTrialCommand(), "joblog", true);
            this.Register("limprocurados", new ClearWantedCommand(), "joblog");
            this.Register("lpp", new ClearWantedCommand(), "joblog", true);
            this.Register("flashbang", new FlashBangCommand(), "joblog");
            this.Register("pajuda", new BackupCommand(), "joblog");

            // Restaurante & Cafe
            this.Register("servir", new ServeCommand(), "joblog");

            // Banco
            this.Register("abrirconta", new OpenAccountCommand(), "joblog");
            this.Register("conta", new OpenAccountCommand(), "joblog", true);
            this.Register("checarsaldo", new CheckBalanceCommand(), "joblog");

            // Roupas
            this.Register("desconto", new DiscountCommand(), "joblog");
        }

        /// <summary>
        /// VIP set of commands
        /// </summary>
        private void RegisterVIP()
        {
            this.Register("empurrar", new PushCommand(), "vip");
            this.Register("puxar", new PullCommand(), "vip");
            this.Register("mudarnick", new FlagMeCommand(), "vip");
            this.Register("definirsh", new SetSHCommand(), "vip");
            this.Register("pararsh", new StopSHCommand(), "vip");
            this.Register("abrirdimmer", new OpenDimmerCommand(), "vip");
            this.Register("odimmer", new OpenDimmerCommand(), "vip", true);
            this.Register("vipa", new VIPAlertCommand(), "vip");
            this.Register("av", new VIPAlertCommand(), "vip", true);
            this.Register("vipalerta", new VIPAlertCommand(), "vip", true);
            this.Register("dvipa", new ToggleVIPAlertCommand(), "vip");
            this.Register("desativarvipa", new ToggleVIPAlertCommand(), "vip", true);
            this.Register("avipa", new ToggleVIPAlertCommand(), "vip", true);
            this.Register("moonwalk", new MoonwalkCommand(), "vip", true);
        }

        /// <summary>
        /// Ambassador set of commands
        /// </summary>
        private void RegisterAmbassadors()
        {
            this.Register("ealerta", new AmbassadorAlertCommand(), "ambassadorlog");
            this.Register("eplantao", new AmbassadorOnDutyCommand(), "ambassadorlog");
            this.Register("splantao", new AmbassadorOffDutyCommand(), "ambassadorlog");
        }

        /// <summary>
        /// Trial Moderator set of commands
        /// </summary>
        private void RegisterTrialModerators()
        {
            this.Register("sa", new StaffAlertCommand(), "stafflog");
            this.Register("onplantao", new OnDutyCommand(), "stafflog");
            this.Register("offplantao", new OffDutyCommand(), "stafflog");
        }

        /// <summary>
        /// Moderator set of commands
        /// </summary>
        private void RegisterModerators()
        {
            this.Register("alertar", new AlertCommand(), "stafflog");
            this.Register("banir", new BanCommand(), "stafflog");
            this.Register("mutar", new MuteCommand(), "stafflog");
            this.Register("desmutar", new UnmuteCommand(), "stafflog");
            this.Register("userinfo", new UserInfoCommand(), "stafflog");
            this.Register("atualizar", new UpdateCommand(), "stafflog");
        }

        /// <summary>
        /// Senior Moderator set of commands
        /// </summary>
        private void RegisterSeniorModerators()
        {
            this.Register("ha", new HotelAlertCommand(), "stafflog");
            this.Register("wha", new WhisperHotelAlertCommand(), "stafflog");
            this.Register("nha", new NoticeHotelAlertCommand(), "stafflog");
            this.Register("banip", new IPBanCommand(), "stafflog");
            this.Register("ralerta", new RoomAlertCommand(), "stafflog");
            this.Register("rmutar", new RoomMuteCommand(), "stafflog");
            this.Register("rdesmutar", new RoomUnmuteCommand(), "stafflog");
            this.Register("trazer", new SummonCommand(), "stafflog");
            this.Register("seguir", new FollowCommand(), "stafflog");
            this.Register("reload", new UnloadCommand(), "stafflog");
            this.Register("uenviar", new SendUserCommand(), "stafflog");
            this.Register("scontratar", new SuperHireCommand(), "stafflog");
        }

        /// <summary>
        /// Administrator set of commands
        /// </summary>
        private void RegisterAdministrators()
        {
            this.Register("ir", new AdminTaxiCommand(), "stafflog");
            this.Register("hal", new HALCommand(), "stafflog");
            this.Register("banpc", new MIPCommand(), "stafflog");
            this.Register("ustatus", new RPStatsCommand(), "stafflog");
            this.Register("uarmas", new RPWeaponsCommand(), "stafflog");
            this.Register("uagricultura", new RPFarmingStatsCommand(), "stafflog");
            this.Register("override", new OverrideCommand(), "stafflog");
            this.Register("tele", new TeleportCommand(), "stafflog");
            this.Register("spuxar", new SuperPullCommand(), "stafflog");
            this.Register("sempurrar", new SuperPushCommand(), "stafflog");
            this.Register("eha", new EventAlertCommand(), "stafflog");
            this.Register("restaurar", new RestoreCommand(), "stafflog");
            this.Register("admliberar", new AdminReleaseCommand(), "stafflog");
            this.Register("admprender", new AdminJailCommand(), "stafflog");
            this.Register("qrestaurar", new RoomRestoreCommand(), "stafflog");
            this.Register("roomrelease", new RoomReleaseCommand(), "stafflog");
            this.Register("qvida", new RoomHealCommand(), "stafflog");
            this.Register("amarrarme", new WarpToMeCommand(), "stafflog");
            this.Register("amarrarem", new WarpMeToCommand(), "stafflog");
            this.Register("listanegra", new BlackListCommand(), "stafflog");
            this.Register("rlistanegra", new UnBlackListCommand(), "stafflog");
        }

        /// <summary>
        /// Manager set of commands
        /// </summary>
        private void RegisterManagers()
        {

            this.Register("daremblema", new GiveBadgeCommand(), "stafflog");
            this.Register("qemblema", new RoomBadgeCommand(), "stafflog");
            this.Register("temblema", new MassBadgeCommand(), "stafflog");
            this.Register("congelar", new FreezeCommand(), "stafflog");
            this.Register("descongelar", new UnFreezeCommand(), "stafflog");
            this.Register("mudarnome", new FlagOtherCommand(), "stafflog");
            this.Register("umudarnome", new FlagOtherCommand(), "stafflog", true);
            this.Register("imitar", new MimicCommand(), "staff");
            this.Register("dsussurro", new ToggleWhispersCommand(), "staff");
            this.Register("desconectar", new DisconnectCommand(), "stafflog");
            this.Register("dc", new DisconnectCommand(), "stafflog", true);
            this.Register("ievento", new StartEventCommand(), "stafflog");
            this.Register("finiciar", new ForceStartCommand(), "stafflog");
            this.Register("pevento", new StopEventCommand(), "stafflog");
            this.Register("checarloteria", new StopEventCommand(), "stafflog", true);
            this.Register("evento", new StartEventCommand(), "stafflog", true);
            this.Register("checarconta", new AccountCheckCommand(), "stafflog");
            this.Register("uchecar", new AccountCheckCommand(), "stafflog", true);
            this.Register("checarnome", new NameCheckCommand(), "stafflog");
            this.Register("unome", new NameCheckCommand(), "stafflog", true);
            this.Register("puxarstaff", new SummonStaffCommand(), "stafflog");
            this.Register("checarpergunta", new CheckPollCommand(), "stafflog");
            this.Register("pchecar", new CheckPollCommand(), "stafflog", true);
            this.Register("teletodos", new WarpAllToMeCommand(), "stafflog");
            this.Register("qenviar", new SendRoomCommand(), "stafflog");
            this.Register("qcongelar", new FreezeRoomCommand(), "stafflog");
            this.Register("qdescongelar", new UnFreezeRoomCommand(), "stafflog");
            this.Register("wonline", new WOnlineCommand(), "stafflog");
            this.Register("botacao", new MakeBotActionCommand(), "stafflog");
            this.Register("banwpp", new BanChatterCommand(), "stafflog");
            this.Register("desbanwpp", new UnBanChatterCommand(), "stafflog");
            this.Register("deletarchat", new DeleteChatCommand(), "stafflog");
            this.Register("ctrancar", new TLockCommand(), "stafflog");
            this.Register("nomear", new EventAssignCommand(), "stafflog");
        }

        /// <summary>
        /// Developer set of commands
        /// </summary>
        private void RegisterDevelopers()
        {
            this.Register("bolha", new BubbleCommand(), "staff");
            this.Register("item", new HandItemCommand(), "staff");
            this.Register("efeito", new EnableCommand(), "staff");
            this.Register("coordenada", new CoordsCommand(), "staff");
            this.Register("velocidade", new SetSpeedCommand(), "stafflog");
            this.Register("ipergunta", new StartQuestionCommand(), "staff");
            this.Register("kbots", new KickBotsCommand(), "stafflog");
            this.Register("kpets", new KickPetsCommand(), "stafflog");
            this.Register("ddiagonal", new DisableDiagonalCommand(), "stafflog");
            this.Register("quarto", new RoomCommand(), "stafflog");
            this.Register("bot", new BotCommand(), "stafflog");
            this.Register("onbots", new ActiveBotsCommand(), "stafflog");
            this.Register("farmas", new FixWeaponsCommand(), "stafflog");
            this.Register("wpiso", new SetWhisperTileCommand(), "stafflog");
            this.Register("pagina", new HtmlPageCommand(), "stafflog");
            this.Register("upagina", new HtmlUPageCommand(), "stafflog");
            this.Register("uipagina", new HtmlUIPageCommand(), "stafflog");
            this.Register("rpagina", new HtmlRPageCommand(), "stafflog");
            this.Register("manutencao", new MaintenanceCommand(), "stafflog");
            this.Register("manu", new MaintenanceCommand(), "stafflog", true);

            this.Register("tarefa", new ToDoCommand());
            this.Register("addtarefa", new ToDoCommand());
            this.Register("addtaref", new ToDoCommand(), "", true);
            this.Register("atf", new ToDoCommand(), "", true);
            this.Register("deltarefa", new ToDoCommand());
            this.Register("deletartarefa", new ToDoCommand(), "", true);
            this.Register("remtarefa", new ToDoCommand(), "", true);
            this.Register("removertarefa", new ToDoCommand(), "", true);
            this.Register("texc", new ToDoCommand(), "", true);

        }

        /// <summary>
        /// Owner set of commands
        /// </summary>
        private void RegisterOwners()
        {
            this.Register("correr", new FastwalkCommand(), "stafflog");
            this.Register("fsentar", new ForceSitCommand(), "stafflog");
            this.Register("fdeitar", new ForceLayCommand(), "stafflog");
            this.Register("puxartodos", new AllAroundMeCommand(), "stafflog");
            this.Register("tolhar", new AllEyesOnMeCommand(), "stafflog");
            this.Register("tdancar", new MassDanceCommand(), "stafflog");
            this.Register("tefeito", new MassEnableCommand(), "stafflog");
            this.Register("trazertodos", new SummonAllCommand(), "stafflog");
            this.Register("libertartodos", new ReleaseAllCommand(), "stafflog");
            this.Register("revivertodos", new RestoreAllCommand(), "stafflog");
            this.Register("invisivel", new InvisibleCommand(), "stafflog");
            this.Register("visivel", new VisibleCommand(), "stafflog");
            this.Register("tacao", new MassActionCommand(), "stafflog");
            this.Register("acordar", new UnIdleCommand(), "stafflog");
            this.Register("desbanir", new UnBanCommand(), "stafflog");
        }

        /// <summary>
        /// Special Right set of commands
        /// </summary>
        private void RegisterSpecialRights()
        {
            this.Register("tpet", new MakePetCommand(), "stafflog");
            this.Register("todospet", new TransformAllCommand(), "stafflog");
            this.Register("qpet", new RoomMakePetCommand(), "stafflog");
            this.Register("trazerpets", new SummonPetsCommand(), "stafflog");
            this.Register("virarpet", new PetTransformCommand(), "stafflog");
            this.Register("cor", new ColourChangeCommand(), "stafflog");
            this.Register("cores", new ColourChangeCommand(), "stafflog", true);
            this.Register("umudarclasse", new ChangeUClassCommand(), "stafflog");
            this.Register("supercorrer", new SuperFastwalkCommand(), "stafflog");
            this.Register("ufalar", new MakeSayCommand(), "stafflog");
            this.Register("tfalar", new SayAllCommand(), "stafflog");
            this.Register("grana", new GiveCoinsCommand(), "stafflog");
            this.Register("creditoscel", new GiveDucketsCommand(), "stafflog");
            this.Register("diamantes", new GiveDiamondsCommand(), "stafflog");
            this.Register("pontosev", new GiveEventPointsCommand(), "stafflog");
            this.Register("cargo", new GiveRankCommand(), "stafflog");
            this.Register("matar", new KillCommand(), "stafflog");
            this.Register("definir", new SetStatCommand(), "stafflog");
            this.Register("sangue", new SetStatCommand(), "stafflog", true);
            this.Register("snap", new KillCommand(), "stafflog");
            this.Register("energia", new SetStatCommand(), "stafflog", true);
            this.Register("fome", new SetStatCommand(), "stafflog", true);
            this.Register("higiene", new SetStatCommand(), "stafflog", true);
            this.Register("darvip", new GiveVIPCommand(), "stafflog");
            this.Register("retirarvip", new TakeVIPCommand(), "stafflog");
            this.Register("banvip", new BanVIPCommand(), "stafflog");
            this.Register("desbanvip", new UnBanVIPCommand(), "stafflog");
        }
        #endregion

        /// <summary>
        /// Registers a Chat Command.
        /// </summary>
        /// <param name="CommandText">Text to type for this command.</param>
        /// <param name="Command">The command to execute.</param>
        public void Register(string CommandText, IChatCommand Command, string Type = "", bool IsAlias = false)
        {
            if (IsAlias && !this._aliases.Contains(CommandText))
                this._aliases.Add(CommandText);

            switch (Type.ToLower())
            {
                case "job":
				case "trabalho":
				case "emprego":
				case "empregos":
				case "trabalhos":
                    {
                        this._commands.Add(CommandText, Command);
                        this._jobcommands.Add(CommandText, Command);
                        break;
                    }
                case "gang":
				case "gangue":
                    {
                        this._commands.Add(CommandText, Command);
                        this._gangcommands.Add(CommandText, Command);
                        break;
                    }
                case "vip":
                    {
                        this._commands.Add(CommandText, Command);
                        this._vipcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "staff":
                    {
                        this._commands.Add(CommandText, Command);
                        this._staffcommands.Add(CommandText, Command);
                        break;
                    }
                case "stafflog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._staffcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "ambassadorlog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._ambassadorcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "userlog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "joblog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._jobcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                case "eventlog":
                    {
                        this._commands.Add(CommandText, Command);
                        this._eventcommands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                default:
                    {
                        this._commands.Add(CommandText, Command);
                        break;
                    }
            }
        }

        public static string MergeParams(string[] Params, int Start)
        {
            var Merged = new StringBuilder();
            for (int i = Start; i < Params.Length; i++)
            {
                if (i > Start)
                    Merged.Append(" ");
                Merged.Append(Params[i]);
            }

            return Merged.ToString();
        }

        public static string GenerateRainbowText(string Name)
        {
            StringBuilder NewName = new StringBuilder();

            string[] Colours = { "FF0000", "FFA500", "FFFF00", "008000", "0000FF", "800080" };

            int Count = 0;
            int Count2 = 0;
            while (Count < Name.Length)
            {
                NewName.Append("<font color='#" + Colours[Count2] + "'>" + Name[Count] + "</font>");

                Count++;
                Count2++;

                if (Count2 >= 6)
                    Count2 = 0;
            }

            return NewName.ToString();
        }

        public void LogCommand(int UserId, string Data, string MachineId, string Type)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Type.ToLower() == "staff")
                    dbClient.SetQuery("INSERT INTO `command_logs_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "ambassador")
                    dbClient.SetQuery("INSERT INTO `command_logs_ambassador` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "job")
                    dbClient.SetQuery("INSERT INTO `command_logs_jobs` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "vip")
                    dbClient.SetQuery("INSERT INTO `command_logs_vip` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "event")
                    dbClient.SetQuery("INSERT INTO `command_logs_events` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else
                    dbClient.SetQuery("INSERT INTO `command_logs_users` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                dbClient.AddParameter("UserId", UserId);
                dbClient.AddParameter("Data", Data);
                dbClient.AddParameter("MachineId", MachineId);
                dbClient.AddParameter("Timestamp", PlusEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public bool TryGetCommand(string Command, out IChatCommand IChatCommand)
        {
            return this._commands.TryGetValue(Command, out IChatCommand);
        }
    }
}