using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorVendor : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";
            Item.UpdateNeeded = true;

            if (Item.InteractingUser > 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.CanWalk = true;
                }
            }
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser > 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.CanWalk = true;
                }
            }
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Item.ExtraData != "1" && Item.InteractingUser == 0 && Session != null)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                if (Item.GetBaseItem().VendingIds.Count >= 1)
                {
                    if (User == null)
                        return;

                    if (!Gamemap.TilesTouching(User.X, User.Y, Item.GetX, Item.GetY))
                    {
                        User.MoveTo(Item.SquareInFront);
                        return;
                    }

                    Item.InteractingUser = Session.GetHabbo().Id;

                    User.CanWalk = false;
                    User.ClearMovement(true);
                    User.SetRot(Rotation.Calculate(User.X, User.Y, Item.GetX, Item.GetY), false);

                    Item.RequestUpdate(2, true);

                    Item.ExtraData = "1";
                    Item.UpdateState(false, true);
                }
                else if (Item.GetBaseItem().EffectId > 0)
                {
                    if (User == null)
                        return;

                    if (!User.IsBot)
                    {
                        if (!Gamemap.TilesTouching(User.X, User.Y, Item.GetX, Item.GetY))
                        {
                            User.MoveTo(Item.SquareInFront);
                            return;
                        }

                        if (Item == null || Item.GetBaseItem() == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Effects() == null)
                            return;

                        if (Item.GetBaseItem().EffectId == 0 && User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                            return;

                        User.GetClient().GetHabbo().Effects().ApplyEffect(Item.GetBaseItem().EffectId);
                        Item.InteractingUser = Session.GetHabbo().Id;

                        User.CanWalk = false;
                        User.ClearMovement(true);
                        User.SetRot(Rotation.Calculate(User.X, User.Y, Item.GetX, Item.GetY), false);

                        Item.RequestUpdate(2, true);

                        Item.ExtraData = "1";
                        Item.UpdateState(false, true);
                    }
                }
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}