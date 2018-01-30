using System;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using System.Linq;
using System.Drawing;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorGenericSwitch : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            int Modes = Item.GetBaseItem().Modes - 1;

            if (Session == null || !HasRights || Modes <= 0)
                return;

            int CurrentMode = 0;
            int NewMode = 0;

            int.TryParse(Item.ExtraData, out CurrentMode);

            if (CurrentMode <= 0)
                NewMode = 1;
            else if (CurrentMode >= Modes)
                NewMode = 0;
            else
                NewMode = CurrentMode + 1;

            Item.ExtraData = NewMode.ToString();
            Item.UpdateState();

            var RoomUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (Item.GetBaseItem().AdjustableHeights.Count > 0)
            {
                if (RoomUser.Z != RoomUser.GetRoom().GetGameMap().GetHeightForSquare(RoomUser.Coordinate))
                {
                    RoomUser.Z = RoomUser.GetRoom().GetGameMap().GetHeightForSquare(RoomUser.Coordinate);
                    RoomUser.ClearMovement(true);
                }

                if (RoomUser.Z < Item.TotalHeight)
                {
                    List<Point> PointList = Item.GetAffectedTiles;
                    Item.GetRoom().GetGameMap().updateMapForItem(Item);

                    foreach (Point point in PointList)
                    {
                        if (point.X == RoomUser.X && point.Y == RoomUser.Y)
                        {
                            RoomUser.Z = Item.TotalHeight;
                            RoomUser.ClearMovement(true);
                        }
                    }
                }
            }
        }

        public void OnWiredTrigger(Item Item)
        {
            int Modes = Item.GetBaseItem().Modes - 1;

            if (Modes == 0)
            {
                return;
            }

            int CurrentMode = 0;
            int NewMode = 0;

            if (string.IsNullOrEmpty(Item.ExtraData))
                Item.ExtraData = "0";

            if (!int.TryParse(Item.ExtraData, out CurrentMode))
            {
                return;
            }

            if (CurrentMode <= 0)
            {
                NewMode = 1;
            }
            else if (CurrentMode >= Modes)
            {
                NewMode = 0;
            }
            else
            {
                NewMode = CurrentMode + 1;
            }

            Item.ExtraData = NewMode.ToString();
            Item.UpdateState();
        }
    }
}