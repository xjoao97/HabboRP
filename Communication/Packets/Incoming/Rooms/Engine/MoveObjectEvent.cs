using System.Linq;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    class MoveObjectEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            int ItemId = Packet.PopInt();

            if (ItemId == 0)
                return;

            Room Room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            bool HasRights = false;

            var RentableItems = Room.GetRoomItemHandler().GetFloor.Where(it => it.GetBaseItem().InteractionType == InteractionType.RENTABLE_SPACE).ToList();
            Item SpaceItem = null;

            if (RentableItems.Count > 0)
            {
                foreach (var RentableSpace in RentableItems)
                {
                    var SpaceData = RentableSpace.RentableSpaceData;

                    if (SpaceData == null)
                        continue;

                    if (SpaceData.FarmingSpace != null)
                        continue;

                    if (SpaceData.OwnerId != Session.GetHabbo().Id)
                        continue;

                    HasRights = true;
                    SpaceItem = RentableSpace;
                    break;
                }
            }

            Item Item = Room.GetRoomItemHandler().GetItem(ItemId);

            if (Room.Group != null)
            {
                if (Room.CheckRights(Session, false, true))
                    HasRights = true;

                if ((Item.GetBaseItem().InteractionType == InteractionType.PURCHASABLE_CLOTHING || Item.GetBaseItem().InteractionType == InteractionType.CRAFTING || Item.GetBaseItem().ItemName.ToLower() == "fxbox_fx192") && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    HasRights = false;

                if (!HasRights)
                { 
                    if (Item == null)
                        return;

                    Session.SendMessage(new ObjectUpdateComposer(Item, Item.UserID));
                }
            }
            else
            {
                if (Room.CheckRights(Session))
                    HasRights = true;

                if ((Item.GetBaseItem().InteractionType == InteractionType.PURCHASABLE_CLOTHING || Item.GetBaseItem().InteractionType == InteractionType.CRAFTING || Item.GetBaseItem().ItemName.ToLower() == "fxbox_fx192") && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    HasRights = false;

                if (!HasRights)
                {
                    if (Item == null)
                        return;

                    Session.SendMessage(new ObjectUpdateComposer(Item, Item.UserID));
                }
            }

            Item = Room.GetRoomItemHandler().GetItem(ItemId);

            if (Item == null)
                return;

            int x = Packet.PopInt();
            int y = Packet.PopInt();
            int Rotation = Packet.PopInt();

            var OldSquares = Item.GetAffectedTiles;
            List<RoomUser> UsersToUpdate = new List<RoomUser>();
            
            foreach (var square in OldSquares)
            {
                if (Room.GetGameMap().SquareHasUsers(square.X, square.Y))
                {
                    foreach (var user in Room.GetGameMap().GetRoomUsers(square))
                    {
                        if (!user.IsWalking)
                        {
                            if (!UsersToUpdate.Contains(user))
                                UsersToUpdate.Add(user);
                        }
                    }
                }
            }

            if ((Item.GetBaseItem().InteractionType == InteractionType.PURCHASABLE_CLOTHING || Item.GetBaseItem().InteractionType == InteractionType.CRAFTING || Item.GetBaseItem().InteractionType == InteractionType.CRAFTING || Item.GetBaseItem().ItemName.ToLower() == "fxbox_fx192") && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendMessage(new ObjectUpdateComposer(Item, Session.GetHabbo().Id));
                return;
            }
            else
            {
                if (!Room.GetRoomItemHandler().SetFloorItem(Session, Item, x, y, Rotation, false, false, true, false, true, SpaceItem))
                {
                    Room.SendMessage(new ObjectUpdateComposer(Item, Session.GetHabbo().Id));
                    return;
                }
            }

            if (!Item.GetBaseItem().IsSeat)
            {
                foreach (var square in Item.GetAffectedTiles)
                {
                    if (Room.GetGameMap().SquareHasUsers(square.X, square.Y))
                    {
                        foreach (var user in Room.GetGameMap().GetRoomUsers(square))
                        {
                            if (!user.IsWalking)
                                user.SetPos(user.Coordinate.X, user.Coordinate.Y, Room.GetGameMap().GetHeightForSquare(square));
                        }
                    }
                }
                if (UsersToUpdate.Count > 0)
                {
                    foreach (var user in UsersToUpdate)
                        user.SetPos(user.Coordinate.X, user.Coordinate.Y, Room.GetGameMap().GetHeightForSquare(new System.Drawing.Point(user.Coordinate.X, user.Coordinate.Y)));
                }
            }
        }
    }
}