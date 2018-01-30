using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using System;
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Misc;
using Plus.Core;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    internal class StopEventCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "command_stop_event";
            }
        }
        public string Parameters
        {
            get
            {
                return "%nome%";
            }
        }
        public string Description
        {
            get
            {
                return "Para o evento desejado!";
            }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1 && Params[0] != "checarloteria")
            {
                Session.SendWhisper("Digite um tipo de evento!", 1);
                return;
            }

            string Message;

            if (Params[0] == "checarloteria")
                Message = "lottery";
            else
                Message = Params[1].ToString().ToLower();

            switch (Message)
            {
                #region Brawl
                case "brawl":
				case "briga":
                    {
                        if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.Brawl))
                            Session.SendWhisper("Não há evento de Briga acontecendo!", 1);
                        else
                        {
                            var Game = RoleplayGameManager.GetGame(GameMode.Brawl);
                            if (Game.GetPlayers().Count > 0)
                            {
                                while (Game.GetPlayers().Count > 0)
                                {
                                    foreach (var player in Game.GetPlayers())
                                    {
                                        var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(player);

                                        if (Client != null)
                                        {
                                            if (Client.GetRoomUser() != null)
                                                Client.GetRoomUser().ClearMovement(true);
                                            RoleplayManager.SpawnChairs(Client, "es_bench");
                                            Game.RemovePlayerFromGame(Client);
                                        }

                                        if (Game.GetPlayers().Contains(player))
                                            Game.GetPlayers().Remove(player);
                                        break;
                                    }

                                    if (Game.GetPlayers().Count <= 0)
                                        break;
                                }
                            }
                            RoleplayGameManager.StopGame(GameMode.Brawl);
                            Session.SendWhisper("Você parou o Evento de Briga!", 1);
                        }
                        break;
                    }
                #endregion

                #region Team Brawl
                case "teambrawl":
                case "tbrawl":
                case "tb":
				case "brigatimes":
				case "bt":
				case "brigadetimes":				
                    {
                        if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.TeamBrawl))
                            Session.SendWhisper("Não há um evento de Briga de Times acontecendo!", 1);
                        else
                        {
                            var Game = RoleplayGameManager.GetGame(GameMode.TeamBrawl);
                            if (Game.GetPlayers().Count > 0)
                            {
                                while (Game.GetPlayers().Count > 0)
                                {
                                    foreach (var Player in Game.GetPlayers())
                                    {
                                        var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Player);

                                        if (Client != null)
                                        {
                                            Game.RemovePlayerFromGame(Client);
                                            RoleplayManager.SpawnChairs(Client, "es_bench");
                                        }
                                        if (Game.GetPlayers().Contains(Player))
                                            Game.GetPlayers().Remove(Player);
                                        break;
                                    }

                                    if (Game.GetPlayers().Count <= 0)
                                        break;
                                }
                            }
                            RoleplayGameManager.StopGame(GameMode.TeamBrawl);
                            Session.SendWhisper("Você parou o Evento Briga de Times!", 1);
                        }
                        break;
                    }
                #endregion

                #region Colour Wars
                case "colorwars":
                case "colourwars":
                case "cw":
				case "guerradecores":
				case "guerracores":
				case "gc":
                    {
                        if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.ColourWars))
                            Session.SendWhisper("Não há nenhum evento de Guerra de Cores acontecendo!", 1);
                        else
                        {
                            var Game = RoleplayGameManager.GetGame(GameMode.ColourWars);
                            if (Game.GetPlayers().Count > 0)
                            {
                                while (Game.GetPlayers().Count > 0)
                                {
                                    foreach (var Player in Game.GetPlayers())
                                    {
                                        var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Player);

                                        if (Client != null)
                                            Game.RemovePlayerFromGame(Client);

                                        if (Game.GetPlayers().Contains(Player))
                                            Game.GetPlayers().Remove(Player);
                                        break;
                                    }

                                    if (Game.GetPlayers().Count <= 0)
                                        break;
                                }
                            }
                            RoleplayGameManager.StopGame(GameMode.ColourWars);
                            Session.SendWhisper("Você parou o Evento Guerra de Cores!", 1);
                        }
                        break;
                    }
                #endregion

                #region Mafia Wars
                case "mafiawars":
                case "mwars":
                case "mw":
				case "gm":
				case "guerramafia":
				case "guerrademafias":
				case "guerrademafia":
                    {
                        if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.MafiaWars))
                            Session.SendWhisper("Não há um evento de Guerra de Máfias acontecendo!", 1);
                        else
                        {
                            var Game = RoleplayGameManager.GetGame(GameMode.MafiaWars);
                            if (Game.GetPlayers().Count > 0)
                            {
                                while (Game.GetPlayers().Count > 0)
                                {
                                    foreach (var Player in Game.GetPlayers())
                                    {
                                        var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Player);

                                        if (Client != null)
                                            Game.RemovePlayerFromGame(Client);

                                        if (Game.GetPlayers().Contains(Player))
                                            Game.GetPlayers().Remove(Player);
                                        break;
                                    }

                                    if (Game.GetPlayers().Count <= 0)
                                        break;
                                }
                            }
                            RoleplayGameManager.StopGame(GameMode.MafiaWars);
                            Session.SendWhisper("Você parou o Evento Guerra de Máfias!", 1);
                        }
                        break;
                    }
                #endregion

                #region Purge

                case "purge":
				case "purga":
				case "tempodepurga":
				case "purg":
                   {
                       if (!RoleplayManager.PurgeStarted)
                       {
                           Session.SendWhisper("Este não há um evento de Purga acontecendo agora!");
                           break;
                       }

                       try
                       {
                           lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
                           {
                               foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                               {
                                   if (client == null)
                                       continue;

                                   if (client.GetHabbo() == null)
                                       continue;

                                   client.SendWhisper("Um membro da Equipe terminou o Evento de Purga. Nós esperamos que você tenha aproveitado! ;)", 34);
                               }
                           }

                           RoleplayManager.PurgeStarted = false;
                       }
                       catch(Exception e)
                       {
                           Logging.LogCriticalException("Erro ao parar a purga: " + e);
                       }
                       break;
                   }

                #endregion

                #region Lottery
                case "lottery":
				case "loteria":
                    {
                        if (!LotteryManager.LotteryFull())
                        {
                            Session.SendWhisper("Nem todos os bilhetes foram vendidos! [" + LotteryManager.LotteryTickets.Count + "/" + LotteryManager.TicketLimit + "]", 1);
                            break;
                        }
                        else
                        {
                            int Winner = LotteryManager.GetWinner();
                            LotteryManager.GivePrize(Winner);
                            LotteryManager.ClearLottery();
                            Session.SendWhisper("A loteria foi limpa com sucesso!", 1);
                        }

                        break;
                    }
                #endregion

                default:
                    {
                        Session.SendWhisper("Este tipo de evento não existe ou está desabilitado!", 1);
                        break;
                    }
            }
        }
    }
}
