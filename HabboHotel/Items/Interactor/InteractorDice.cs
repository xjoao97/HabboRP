using System;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Gambling;
using System.Linq;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorDice : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            if (Item.ExtraData == "-1")
            {
                Item.ExtraData = "0";
                Item.UpdateNeeded = true;
            }
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            if (Item.ExtraData == "-1")
            {
                Item.ExtraData = "0";
            }
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            #region Check Banker Dice Roll
            if (TexasHoldEmManager.GameList.Count > 0)
            {
                if (TexasHoldEmManager.GameList.Values.Where(x => x != null && x.Banker != null && x.Banker.Values.Where(y => y != null && y.Furni != null && y.Furni == Item).ToList().Count > 0).ToList().Count > 0)
                {
                    if (Item.ExtraData != "-1" && Item.TexasHoldEmData != null && Item.TexasHoldEmData.Value == 0)
                    {
                        Item.ExtraData = "-1";
                        Item.UpdateState(false, true);
                        Item.RequestUpdate(3, true);
                    }
                    return;
                }
            }
            #endregion

            RoomUser User = null;
            if (Session != null)
                User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (TexasHoldEmManager.GameList.Count > 0)
            {
                if (Session.GetRoleplay() != null && Session.GetRoleplay().TexasHoldEmPlayer > 0)
                {
                    TexasHoldEm Game = TexasHoldEmManager.GetGameForUser(Session.GetHabbo().Id);

                    if (Game != null)
                    {
                        if (Game.PlayersTurn == Session.GetRoleplay().TexasHoldEmPlayer)
                            Game.RollDice(Session, Item, Request);
                        return;
                    }
                }
            }

            if (Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                if (Item.ExtraData != "-1")
                {
                    if (Request == -1)
                    {
                        Item.ExtraData = "0";
                        Item.UpdateState();
                    }
                    else
                    {
                        Item.ExtraData = "-1";
                        Item.UpdateState(false, true);
                        Item.RequestUpdate(3, true);
                    }
                }
            }
            else
            {
                User.MoveTo(Item.SquareInFront);
            }
        }

        public void OnWiredTrigger(Item Item)
        {
            Item.ExtraData = "-1";
            Item.UpdateState(false, true);
            Item.RequestUpdate(4, true);
        }
    }
}