using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using System;
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    internal class StartEventCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "command_start_event";
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
                return "Inicia o evento desejado!";
            }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite um tipo de evento!", 1);
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
                            Session.SendWhisper("Já existe um evento de Briga!", 1);
                        else
                        {
                            RoleplayGameManager.CreateGame("brawl");
                            Session.SendWhisper("Você começou um evento de Briga!", 1);
                        }
                        break;
                    }
                #endregion

                #region Team Brawl
                case "teambrawl":
                case "tbrawl":
                case "tb":
				case "bt":
				case "brigadetimes":
				case "brigatimes":
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.TeamBrawl))
                            Session.SendWhisper("Já existe um evento de Briga de Times!", 1);
                        else
                        {
                            RoleplayGameManager.CreateGame("teambrawl");
                            Session.SendWhisper("Você começou um evento de Briga de Times!", 1);
                        }
                        break;
                    }
                #endregion

                #region Colour Wars
                case "colorwars":
                case "colourwars":
                case "cw":
				case "gc":
				case "guerradecores":
				case "guerracores":
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.ColourWars))
                            Session.SendWhisper("Já existe um evento de Guerra de Cores!", 1);
                        else
                        {
                            RoleplayGameManager.CreateGame("colourwars");
                            Session.SendWhisper("Você começou um evento de Guerra de Cores!", 1);
                        }
                        break;
                    }
                #endregion

                #region Mafia Wars
                case "mafiawars":
                case "mwars":
                case "mw":
				case "gm":
				case "guerrademafias":
				case "guerrademafia":
				case "guerramafia":
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.MafiaWars))
                            Session.SendWhisper("Já existe um evento de Guerra de Máfias!", 1);
                        else
                        {
                            RoleplayGameManager.CreateGame("mafiawars");
                            Session.SendWhisper("Você começou um evento de Guerra de Máfias!", 1);
                        }
                        break;
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
