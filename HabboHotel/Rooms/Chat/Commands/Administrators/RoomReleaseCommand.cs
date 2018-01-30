using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class RoomReleaseCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_room_release"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "O administrador libera qualquer pessoa da sala da prisão se estiverem preso."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int JailedUsers = 0;
            #endregion

            #region Conditions
            foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
            {
                if (User == null)
                    continue;

                if (User.IsBot)
                    continue;

                if (User.GetClient() == null)
                    continue;

                if (User.GetClient().GetRoleplay() == null)
                    continue;

                if (!User.GetClient().GetRoleplay().IsJailed)
                    continue;

                JailedUsers++;
            }

            if (JailedUsers <= 0)
            {
                Session.SendWhisper("Não há usuários presos nesta sala!", 1);
                return;
            }
            #endregion

            #region Execute
            else
            {
                foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                {
                    if (User == null)
                        continue;

                    if (User.IsBot)
                        continue;

                    if (User.GetClient() == null)
                        continue;

                    if (User.GetClient().GetRoleplay() == null)
                        continue;

                    if (!User.GetClient().GetRoleplay().IsJailed)
                        continue;

                    GameClient TargetClient = User.GetClient();

                    TargetClient.GetRoleplay().IsJailed = false;
                    TargetClient.GetRoleplay().JailedTimeLeft = -5;
                    TargetClient.SendWhisper("Um administrador libertou você da prisão!", 1);
                }

                Session.Shout("*Retira imediatamente todos usuários presos da cadeia", 23);
                return;
            }
            #endregion
        }
    }
}