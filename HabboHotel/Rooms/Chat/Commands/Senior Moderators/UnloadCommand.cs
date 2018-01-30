using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class UnloadCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_unload"; }
        }

        public string Parameters
        {
            get { return "%id%"; }
        }

        public string Description
        {
            get { return "Recarrega o quarto atual."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Room R = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Room.Id, out R))
                return;

            if (Room.Name.ToLower().Contains("gun"))
            {
                RoleplayManager.CalledDelivery = false;
                RoleplayManager.DeliveryWeapon = null;
            }

            List<RoomUser> UsersToReturn = Room.GetRoomUserManager().GetRoomUsers().ToList();

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(R, true);

            foreach (RoomUser User in UsersToReturn)
            {
                if (User == null || User.GetClient() == null)
                    continue;

                RoleplayManager.SendUser(User.GetClient(), Room.Id, "Este quarto foi recarregado para correções!");

            }
        }
    }
}
