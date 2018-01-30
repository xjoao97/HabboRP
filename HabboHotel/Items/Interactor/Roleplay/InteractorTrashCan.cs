using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Items.Crafting;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorTrashCan : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                if (Item.ExtraData == "" || Item.ExtraData == "0")
                    if (User.CanWalk)
                        User.MoveTo(Item.SquareInFront);
            }
            else
            {
                if (Item.ExtraData == "")
                    Item.ExtraData = "0";

                if (Item.ExtraData == "0")
                {
                    int Minutes = 8;

                    User.ClearMovement(true);
                    User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                    // 135 Cycles approximately 1 minute
                    Item.ExtraData = "1";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(135 * Minutes, true);
                    Session.Shout("*Começa a vasculhar o lixo*", 4);

                    new Thread(() =>
                    {
                        User.CanWalk = false;

                        if (User.CurrentEffect != 4 && Session.GetRoleplay().EquippedWeapon == null)
                            User.ApplyEffect(EffectsList.Twinkle);

                        Thread.Sleep(5000);

                        if (User.CurrentEffect != 0 && Session.GetRoleplay().EquippedWeapon == null)
                            User.ApplyEffect(0);

                        if (Session != null && Session.GetRoleplay() != null && Session.GetHabbo() != null)
                            ChooseReward(Session);
                        if (User != null)
                            User.CanWalk = true;
                    }).Start();
                }
                else
                    Session.SendWhisper("Alguém já vasculhou este lixo!", 1);
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }

        public void ChooseReward(GameClient Session)
        {
            var Random = new CryptoRandom();
            int TotalCraftingItems = CraftingManager.CraftableItems.Count;
            int Chance = Random.Next(1, 101);
            int SecondChance = Random.Next(1, 101);

            if (SecondChance < 4 && Chance > TotalCraftingItems)
                Chance = Random.Next(1, TotalCraftingItems + 1);

            #region Crafting Materials
            if (Chance <= TotalCraftingItems)
            {
                var CraftingItemName = CraftingManager.CraftableItems[Chance - 1];

                ItemData Data = null;
                foreach (var itemdata in PlusEnvironment.GetGame().GetItemManager()._items.Values)
                {
                    if (itemdata.ItemName != CraftingItemName)
                        continue;

                    Data = itemdata;
                    break;
                }

                var Item = ItemFactory.CreateSingleItemNullable(Data, Session.GetHabbo(), "", "");
                Session.GetHabbo().GetInventoryComponent().TryAddItem(Item);

                ICollection<Item> FloorItems = Session.GetHabbo().GetInventoryComponent().GetFloorItems();
                ICollection<Item> WallItems = Session.GetHabbo().GetInventoryComponent().GetWallItems();

                Session.GetRoleplay().CraftingCheck = true;
                Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems, Session.GetRoleplay().CraftingCheck));
                Session.Shout("*Depois de vasculhar o lixo, encontra " + Item.GetBaseItem().PublicName +"*", 4);
            }
            #endregion

            #region Drugs
            else if (Chance > TotalCraftingItems && Chance <= 60)
            {
                int Amount;

                // Cocaine
                if (Chance > 30)
                {
                    Amount = Random.Next(1, 10);
                    Session.GetRoleplay().Cocaine += Amount;
                    Session.Shout("*Depois de vasculhar o lixo, acha pequena bolsa contendo " + Amount + "g de cocaina*", 4);
                }

                // Cigarettes
                else if (Chance <= 30 && Chance > 16)
                {
                    Amount = Random.Next(1, 10);
                    Session.GetRoleplay().Cigarettes += Amount;
                    Session.Shout("*Depois de vasculhar o lixo, acha pequena bolsa contendo " + Amount + " cigarro(s)*", 4);
                }

                // Weed
                else
                {
                    Amount = Random.Next(1, 10);
                    Session.GetRoleplay().Weed += Amount;
                    Session.Shout("*Depois de vasculhar o lixo, acha pequena bolsa contendo " + Amount + "g de maconha*", 4);
                }
            }
            #endregion

            #region Money
            else if (Chance > 30 && Chance <= 55)
            {
                int Amount = Random.Next(3, 10);

                Session.GetHabbo().Credits += Amount;
                Session.GetHabbo().UpdateCreditsBalance();
                Session.Shout("*Depois de revirar o lixo, acha uma carteira contendo R$" + Amount + "*", 4);
            }
            #endregion

            #region Special Bot
            else if (Chance > 75 && Chance <= 78)
            {

            }
            #endregion

            #region No Reward
            else
            {
                Session.Shout("*Não encontra nada depois de revirar o lixo*", 4);
            }
            #endregion
        }
    }
}