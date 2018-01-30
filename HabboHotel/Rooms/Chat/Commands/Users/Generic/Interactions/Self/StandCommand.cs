using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class StandCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_stand"; }
        }

        public string Parameters
        {
            get { return ""; ; }
        }

        public string Description
        {
            get { return "Permite que você se levante."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            if (User == null)
                return;

            var Items = Room.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
            bool HasChair = Items.ToList().Where(x => x != null && x.GetBaseItem().IsSeat).Count() > 0;
            bool HasBed = Items.ToList().Where(x => x != null && x.GetBaseItem().IsBed()).Count() > 0;

            if (HasChair || HasBed)
                return;

            if (User.isSitting)
            {
                User.Z += 0.35;
                User.RemoveStatus("sit");
                User.isSitting = false;
                User.UpdateNeeded = true;
            }
            else if (User.isLying)
            {
                User.Z += 0.35;
                User.RemoveStatus("lay");
                User.isLying = false;
                User.UpdateNeeded = true;
            }
        }
    }
}
