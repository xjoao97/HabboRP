using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using System;
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Misc;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    internal class EventAssignCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "command_event_assign";
            }
        }
        public string Parameters
        {
            get
            {
                return "%usuário% %evento% %time%";
            }
        }
        public string Description
        {
            get
            {
                return "Atribui um cidadão a um evento desejado!";
            }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 4 && Params[2].ToLower() != "briga" && Params[2].ToLower() != "bg")
            {
                Session.SendWhisper("Digite o usuário, jogo e equipe (se necessário) que deseja atribuir.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetRoleplay() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Game != null || TargetClient.GetRoleplay().Team != null)
            {
                Session.SendWhisper("Este usuário já está dentro de um evento!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode colocar esse usuário em um evento enquanto ele está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode colocar esse usuário em um evento enquanto ele está preso!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsNoob)
            {
                Session.SendWhisper("Você não pode atribuir esse usuário a um evento enquanto ele é novato!", 1);
                return;
            }

            string Game = Params[2].ToString().ToLower();

            List<string> ValidTeams = new List<string>();
            ValidTeams.Add("blue");
            ValidTeams.Add("green");
            ValidTeams.Add("pink");
            ValidTeams.Add("yellow");

            switch (Game)
            {
                #region Brawl

                case "brawl":
                case "br":
				case "briga":
                    {
                        if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.Brawl))
                        {
                            Session.SendWhisper("Não há nenhum evento de Briga atualmente ativo!", 1);
                            break;
                        }

                        if (TargetClient.GetRoleplay().IsWorking)
                            TargetClient.GetRoleplay().IsWorking = false;

                        if (TargetClient.GetRoleplay().EquippedWeapon != null)
                            TargetClient.GetRoleplay().EquippedWeapon = null;

                        RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.Brawl), TargetClient, "", true);
                        Session.SendWhisper("Sucesso, você colocou " + TargetClient.GetHabbo().Username + " no evento de Briga!", 1);
                        break;
                    }

                #endregion

                #region Colour Wars

                case "cw":
                case "color":
                case "colourwars":
                case "colorwars":
				case "gc":
				case "guerradecores":
				case "guerracores":
                    {
                        if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.ColourWars))
                        {
                            Session.SendWhisper("Não há nenhum evento de Guerra de Cores atualmente ativo!", 1);
                            break;
                        }

                        string Team = Params[3].ToString().ToLower();
                        if (Params.Length != 4 || !ValidTeams.Contains(Team))
                        {
                            Session.SendWhisper("Você escolheu uma equipe inválida. Aqui estão as equipes disponíveis: " + string.Join(", ", ValidTeams.ToArray()), 1);
                            break;
                        }

                        if (TargetClient.GetRoleplay().IsWorking)
                            TargetClient.GetRoleplay().IsWorking = false;

                        if (TargetClient.GetRoleplay().EquippedWeapon != null)
                            TargetClient.GetRoleplay().EquippedWeapon = null;

                        RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.ColourWars), TargetClient, Team, true);
                        Session.SendWhisper("Sucesso, você colocou " + TargetClient.GetHabbo().Username + " no evento Guerra de Cores!", 1);
                        break;
                    }

                #endregion

                #region Team Brawl

                case "tb":
                case "teambrawl":
                case "tbrawl":
                case "teamb":
				case "brigatimes":
				case "bt":
				case "brigatime":
                    {
                        if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.TeamBrawl))
                        {
                            Session.SendWhisper("Não há um evento Briga de Times atualmente ativo!", 1);
                            break;
                        }

                        string Team = Params[3].ToString().ToLower();
                        if (Params.Length != 4 || !ValidTeams.Contains(Team))
                        {
                            Session.SendWhisper("Você escolheu uma equipe inválida. Aqui estão as equipes disponíveis: " + string.Join(", ", ValidTeams.ToArray()), 1);
                            break;
                        }

                        if (TargetClient.GetRoleplay().IsWorking)
                            TargetClient.GetRoleplay().IsWorking = false;

                        if (TargetClient.GetRoleplay().EquippedWeapon != null)
                            TargetClient.GetRoleplay().EquippedWeapon = null;

                        RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.TeamBrawl), TargetClient, Team, true);
                        Session.SendWhisper("Sucesso, você colocou " + TargetClient.GetHabbo().Username + " no evento Briga de Times!", 1);
                        break;
                    }

                #endregion

                #region Mafia Wars

                case "mw":
                case "mafia":
                case "mariawars":
				case "gm":
				case "guerramafia":
				case "guerrademafias":
				case "guerramafias":
                    {
                        if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.MafiaWars))
                        {
                            Session.SendWhisper("Não há um evento Guerra de Máfias atualmente ativo!", 1);
                            break;
                        }

                        string Team = Params[3].ToString().ToLower();
                        if (Params.Length != 4 || !ValidTeams.Contains(Team))
                        {
                            Session.SendWhisper("Você escolheu uma equipe inválida. Aqui estão as equipes disponíveis: " + string.Join(", ", ValidTeams.ToArray()), 1);
                            break;
                        }

                        if (TargetClient.GetRoleplay().IsWorking)
                            TargetClient.GetRoleplay().IsWorking = false;

                        if (TargetClient.GetRoleplay().EquippedWeapon != null)
                            TargetClient.GetRoleplay().EquippedWeapon = null;

                        RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.MafiaWars), TargetClient, Team, true);
                        Session.SendWhisper("Sucesso, você colocou " + TargetClient.GetHabbo().Username + " no evento Guerra de Máfias!", 1);
                        break;
                    }

                #endregion

                default:
                    {
                        Session.SendWhisper("O jogo que você escolheu não existe. Aqui estão os eventos disponíveis: briga, guerradecores/gc, brigadetimes/bt, guerrademafias/gm.", 1);
                        break;
                    }
            }
        }
    }
}
