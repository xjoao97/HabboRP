using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class LayCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_lay"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite que você se deite na sala, sem precisar de uma cama."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            if (!Room.GetGameMap().ValidTile(User.X + 2, User.Y + 2) && !Room.GetGameMap().ValidTile(User.X + 1, User.Y + 1))
            {
                Session.SendWhisper("Opa, não pode deitar aqui - tente em outro lugar!", 1);
                return;
            }

            if (User.ForceSit || User.ForceLay)
                return;

            if (User.Statusses.ContainsKey("sit") || User.isSitting || User.RidingHorse || User.IsWalking)
                return;

            var Items = Room.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
            bool HasBed = Items.ToList().Where(x => x != null && x.GetBaseItem().IsBed()).Count() > 0;

            if (HasBed)
                return;

            if (Session.GetHabbo().Effects().CurrentEffect > 0)
                Session.GetHabbo().Effects().ApplyEffect(0);

            if (!User.Statusses.ContainsKey("lay"))
            {
                if ((User.RotBody % 2) == 0)
                {
                    if (User == null)
                        return;

                    try
                    {
                        User.Statusses.Add("lay", "1.0 null");
                        User.Z -= 0.35;
                        User.isLying = true;
                        User.UpdateNeeded = true;
                    }
                    catch { }
                }
                else
                {
                    User.RotBody--;
                    User.Statusses.Add("lay", "1.0 null");
                    User.Z -= 0.35;
                    User.isLying = true;
                    User.UpdateNeeded = true;
                }
            }
            else
            {
                User.Z += 0.35;
                User.RemoveStatus("lay");
                User.isLying = false;
                User.UpdateNeeded = true;
            }
        }
    }
}