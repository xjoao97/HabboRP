using System;
using System.Linq;
using System.Text;
using System.Drawing;
using Plus.Utilities;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System.Threading;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Criminal
{
    class DisposeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_criminal_activity_dispose"; }
        }

        public string Parameters
        {
            get { return "%droga%"; }
        }

        public string Description
        {
            get { return "Joga fora suas drogas. (maconha/cocaina/todas)."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            string Type = "all";
            if (Params.Length > 1)
                Type = Params[1].ToLower();

            switch (Type)
            {
                #region Weed
                case "weed":
				case "maconha":
                    {
                        if (Session.GetRoleplay().Weed <= 0)
                        {
                            Session.SendWhisper("Você não tem maconha para jogar fora!", 1);
                            break;
                        }

                        Session.GetRoleplay().Weed = 0;
                        Session.Shout("*Retira todas as ervas do bolso e joga fora.*", 4);
                        break;
                    }
                #endregion

                #region Cocaine
                case "cocaine":
				case "cocaina":
                    {
                        if (Session.GetRoleplay().Cocaine <= 0)
                        {
                            Session.SendWhisper("Você não tem cocaína para jogar fora!", 1);
                            break;
                        }

                        Session.GetRoleplay().Cocaine = 0;
                        Session.Shout("*Retira toda a cocaína do bolso e joga fora.*", 4);
                        break;
                    }
                #endregion

                #region All Drugs
                case "all":
				case "tudo":
                    {
                        if (Session.GetRoleplay().Weed <= 0 && Session.GetRoleplay().Cocaine <= 0)
                        {
                            Session.SendWhisper("Você não tem drogas para jogar fora", 1);
                            break;
                        }

                        Session.GetRoleplay().Weed = 0;
                        Session.GetRoleplay().Cocaine = 0;
                        Session.Shout("*Retira todas as suas drogas do bolso e joga fora*", 4);
                        break;
                    }
                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("Este tipo de droga não existe!", 1);
                        break;
                    }
                #endregion
            }
        }
    }
}