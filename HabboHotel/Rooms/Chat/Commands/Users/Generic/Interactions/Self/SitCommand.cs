using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class SitCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_sit"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite que você se sente em seu lugar atual."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            if (User.Statusses.ContainsKey("lay") || User.isLying || User.RidingHorse || User.IsWalking)
                return;

            var Items = Room.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
            bool HasChair = Items.ToList().Where(x => x != null && x.GetBaseItem().IsSeat).Count() > 0;

            if (User.ForceSit || User.ForceLay)
                return;

            if (HasChair)
                return;

            if (!User.Statusses.ContainsKey("sit"))
            {
                if ((User.RotBody % 2) == 0)
                {
                    if (User == null)
                        return;

                    try
                    {
                        User.Statusses.Add("sit", "1.0");
                        User.Z -= 0.35;
                        User.isSitting = true;
                        User.UpdateNeeded = true;
                    }
                    catch { }
                }
                else
                {
                    User.RotBody--;
                    User.Statusses.Add("sit", "1.0");
                    User.Z -= 0.35;
                    User.isSitting = true;
                    User.UpdateNeeded = true;
                }
            }
            else
            {
                User.Z += 0.35;
                User.RemoveStatus("sit");
                User.isSitting = false;
                User.UpdateNeeded = true;
            }
        }
    }
}
