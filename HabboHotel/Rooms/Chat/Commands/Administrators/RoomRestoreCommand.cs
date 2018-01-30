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
    class RoomRestoreCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_room_restore"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "O administrador recupera qualquer pessoa na sala do hospital se estiverem mortos."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int DeadUsers = 0;
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

                if (!User.GetClient().GetRoleplay().IsDead)
                    continue;

                DeadUsers++;
            }

            if (DeadUsers <= 0)
            {
                Session.SendWhisper("Não há usuários mortos nesta sala!", 1);
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

                    if (!User.GetClient().GetRoleplay().IsDead)
                        continue;

                    GameClient TargetClient = User.GetClient();

                    TargetClient.GetRoleplay().IsDead = false;
                    TargetClient.GetRoleplay().DeadTimeLeft = 0;
                    TargetClient.GetRoleplay().ReplenishStats(true);
                    TargetClient.SendWhisper("Um administrador reviveu você!", 1);
                }

                Session.Shout("*Restaura imediatamente qualquer pessoa morta na sala*", 23);
                return;
            }
            #endregion
        }
    }
}