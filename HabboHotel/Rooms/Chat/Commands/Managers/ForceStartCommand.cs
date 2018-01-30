using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Misc;
using System.Threading;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Guides;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    internal class ForceStartCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "command_force_start";
            }
        }
        public string Parameters
        {
            get
            {
                return "%evento%";
            }
        }
        public string Description
        {
            get
            {
                return "Force iniciar o evento desejado!";
            }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite um tipo de evento: [briga] - [brigadetimes/bt] - [guerradecores/gc] - [guerrademafias/gm] - [purga]!", 1);
                return;
            }

            string Message = Params[1].ToString().ToLower();

            switch (Message)
            {
                #region Brawl
                case "brawl":
				case "briga":
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.Brawl))
                        {
                            IGame Game = RoleplayGameManager.GetGame(GameMode.Brawl);
                            Game.Start();
                            Session.Shout("*Inicia um evento de brigas*", 23);
                        }
                        else
                            Session.SendWhisper("Não há nenhum evento de briga ativo! Use ':ievento briga' para começar um.", 1);
                        break;
                    }
                #endregion

                #region Team Brawl
                case "teambrawl":
                case "tbrawl":
                case "tb":
				case "brigatimes":
				case "brigadetimes":
				case "bt":
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.TeamBrawl))
                        {
                            IGame Game = RoleplayGameManager.GetGame(GameMode.TeamBrawl);
                            Game.Start();
                            Session.Shout("*Inicia um evento de " + Game.GetName() + "*", 23);
                        }
                        else
                            Session.SendWhisper("Não há nenhum evento Briga de Times ativo! Use ':ievento bt' para começar um.", 1);
                        break;
                    }
                #endregion

                #region Colour Wars
                case "colorwars":
                case "colourwars":
                case "cw":
				case "guerradecores":
				case "gc":
				case "guerracores":
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.ColourWars))
                        {
                            IGame Game = RoleplayGameManager.GetGame(GameMode.ColourWars);
                            Game.Start();
                            Session.Shout("*Inicia um evento de " + Game.GetName() + "*", 23);
                        }
                        else
                            Session.SendWhisper("Não há nenhum evento Guerra de Cores ativo! Use ':ievento gc' para começar um.", 1);
                        break;
                    }
                #endregion

                #region Mafia Wars
                case "mafiawars":
                case "mwars":
                case "mw":
				case "gm":
				case "guerramafias":
				case "guerrademafias":
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.MafiaWars))
                        {
                            IGame Game = RoleplayGameManager.GetGame(GameMode.MafiaWars);
                            Game.Start();
                            Session.Shout("*Force inicia um jogo de " + Game.GetName() + "*", 23);
                        }
                        else
                            Session.SendWhisper("Não há nenhum evento Guerra de Máfias ativo! Use ':ievento gm' para começar um.", 1);
                        break;
                    }
                #endregion

                #region Purge

                case "purge":
				case "purga":
                    {
                        if (RoleplayManager.PurgeStarted)
                        {
                            Session.SendWhisper("Evento de Purga já começou!");
                            break;
                        }

                        try
                        {
                            lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                                {
                                    if (client == null)
                                        continue;

                                    if (client.GetHabbo() == null)
                                        continue;

                                    if (client.GetHabbo().CurrentRoom == null)
                                        continue;

                                    if (client.GetRoomUser() == null)
                                        continue;                                    

                                    if (client.GetRoleplay() == null)
                                        continue;

                                    int Counter = 11;

                                    new Thread(() =>
                                    {
                                        while (Counter > 0)
                                        {
                                            if (client != null)
                                            {
                                                if (Counter == 11)
                                                    client.SendWhisper("O evento PURGA começará em alguns segundos!", 1);
                                                else
                                                    client.SendWhisper("A Purga começará em " + Counter + " segundos!", 1);
                                            }

                                            Counter--;
                                            Thread.Sleep(1000);

                                            if (Counter == 0)
                                            {
                                                client.SendWhisper("O tempo de Purga foi ativado! [TODOS CRIMES SÃO LEGAIS].", 34);
                                                RoleplayManager.WantedList.Clear();

                                                if (client.GetRoleplay().IsJailed)
                                                {
                                                    client.GetRoleplay().IsJailed = false;
                                                    client.GetRoleplay().JailedTimeLeft = 0;
                                                }

                                                if (GroupManager.HasJobCommand(client, "guide"))
                                                {
                                                    WorkManager.RemoveWorkerFromList(client);
                                                    client.GetRoleplay().IsWorking = false;
                                                    client.GetHabbo().Poof();

                                                    PlusEnvironment.GetGame().GetGuideManager().RemoveGuide(client);
                                                    client.SendMessage(new HelperToolConfigurationComposer(client));

                                                    #region End Existing Calls
                                                    if (client.GetRoleplay().GuideOtherUser != null)
                                                    {
                                                        client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                                                        client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                                                        if (client.GetRoleplay().GuideOtherUser.GetRoleplay() != null)
                                                        {
                                                            client.GetRoleplay().GuideOtherUser.GetRoleplay().Sent911Call = false;
                                                            client.GetRoleplay().GuideOtherUser.GetRoleplay().GuideOtherUser = null;
                                                        }

                                                        client.GetRoleplay().GuideOtherUser = null;
                                                        client.SendMessage(new OnGuideSessionDetachedComposer(0));
                                                        client.SendMessage(new OnGuideSessionDetachedComposer(1));
                                                    }
                                                    #endregion

                                                }
                                                RoleplayManager.PurgeStarted = true; // let the fun begin
                                            }
                                        }

                                    }).Start();
                                }
                            }
                            break;
                        }
                        catch(Exception e)
                        {
                            lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                                {
                                    if (client == null)
                                        continue;

                                    if (client.GetHabbo() == null)
                                        continue;

                                    if (client.GetHabbo().CurrentRoom == null)
                                        continue;

                                    if (client.GetRoomUser() == null)
                                        continue;

                                    client.SendWhisper("Desculpe, ocorreu um erro ao iniciar 'Tempo de Purga' - Será investigado pela equipe de técnicos do HabboRPG!");
                                }
                            }

                            Logging.LogRPGamesError("Erro em iniciar a Purga: " + e);
                            break;
                        }
                    }

                #endregion

                default:
                    {
                        Session.SendWhisper("Esse tipo de evento não existe ou está desativado!", 1);
                        break;
                    }
            }
        }
    }
}
