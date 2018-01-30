using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using System.Drawing;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    class MoveAvatarEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null || !User.CanWalk)
                return;

            if (!User.IsBot)
                User.GetClient().GetRoleplay().WalkDirection = HabboHotel.Roleplay.Web.Incoming.Interactions.WalkDirections.None;

            int MoveX = Packet.PopInt();
            int MoveY = Packet.PopInt();

            List<Item> BedItems = Room.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().IsBed()).ToList();
            if (BedItems.Count > 0)
            {
                bool HasBed = BedItems.Where(x => x.GetAffectedTiles.Contains(new Point(MoveX, MoveY))).ToList().Count > 0;
                if (HasBed)
                {
                    var Item = BedItems.Where(x => x.GetAffectedTiles.Contains(new Point(MoveX, MoveY))).FirstOrDefault();

                    Point Square;
                    if (Item.GetBedTiles(new Point(MoveX, MoveY), out Square).Count > 0)
                    {
                        MoveX = Square.X;
                        MoveY = Square.Y;
                    }
                }
            }

            if (User.RidingHorse)
            {
                RoomUser Horse = Room.GetRoomUserManager().GetRoomUserByVirtualId(User.HorseID);
                if (Horse != null)
                    Horse.MoveTo(MoveX, MoveY);
            }

            if (User.isLying)
            {
                User.Z += 0.35;
                User.RemoveStatus("lay");
                User.isLying = false;
                User.UpdateNeeded = true;
            }

            if (User.isSitting)
            {
                User.Z += 0.35;
                User.RemoveStatus("sit");
                User.isSitting = false;
                User.UpdateNeeded = true;
            }

            User.MoveTo(MoveX, MoveY);        
        }
    }
}