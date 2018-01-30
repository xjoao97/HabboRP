using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting;
using Plus.HabboHotel.Rooms;
using System.Threading;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Farming;
using Plus.Utilities;
using System.Drawing;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorFarming : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {

        }

        public void OnRemove(GameClient Session, Item Item)
        {

        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            RoomUser roomUser = null;

            if (Session != null && Session.GetRoleplay() != null)
                roomUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (roomUser == null)
                return;

            if (Item.FarmingData == null)
            {
                Session.SendWhisper("Desculpe, este item não tem dados agrícolas!", 1);
                return;
            }

            FarmingItem FarmingItem = FarmingManager.GetFarmingItem(Item.GetBaseItem().ItemName);

            if (FarmingItem == null)
            {
                Session.SendWhisper("Desculpe, este não é um item agrícola!", 1);
                return;
            }

            if (Item.FarmingData.OwnerId != 0 && Item.FarmingData.OwnerId != Session.GetHabbo().Id)
            {
                Session.SendWhisper("Alguém mais possui essa planta!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("farming", false))
                return;

            #region Check Rentable Space
            if (Item.GetRoom().GetRoomItemHandler() != null && Item.GetRoom().GetRoomItemHandler().GetFloor != null)
            {
                lock (Item.GetRoom().GetRoomItemHandler().GetFloor)
                {
                    List<Item> OwnedRentableSpaces = Item.GetRoom().GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == InteractionType.RENTABLE_SPACE && x.RentableSpaceData != null && x.RentableSpaceData.FarmingSpace != null && x.RentableSpaceData.FarmingSpace.OwnerId == Session.GetHabbo().Id).ToList();
                    if (OwnedRentableSpaces.Count <= 0)
                    {
                        Session.SendWhisper("Você não possui esse terreno para cultivar!", 1);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                        return;
                    }

                    Item SpaceItem = OwnedRentableSpaces.FirstOrDefault();
                    List<Point> SpacePoints = SpaceItem.GetAffectedTiles;

                    if (!SpacePoints.Contains(Item.Coordinate))
                    {
                        Session.SendWhisper("YVocê não possui esse terreno para cultivar!!", 1);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                        return;
                    }
                }
            }
            #endregion


            if (Item.FarmingData.BeingFarmed)
            {
                if (Item.ExtraData != "0")
                    Session.SendWhisper("Esta planta foi regada recentemente", 1);
                else
                    Session.SendWhisper("Esta semente foi plantada!", 1);
                return;
            }

            if (!Session.GetRoleplay().WateringCan && Item.ExtraData != "8")
            {
                Session.SendWhisper("Você não tem água na sua mão!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                return;
            }

            if (RoleplayManager.FarmingCAPTCHABox)
            {
                if (Session.GetRoleplay().CaptchaSent)
                {
                    Session.SendWhisper("Você deve inserir o código no código AFK para continuar cultivando!", 1);
                    Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                    return;
                }
            }

            CryptoRandom Random = new CryptoRandom();

            if (Gamemap.TilesTouching(Item.GetX, Item.GetY, roomUser.X, roomUser.Y))
            {
                if (Item.ExtraData == "4")
                {
                    if (!Session.GetRoleplay().FarmingStats.HasPlantSatchel)
                    {
                        Session.SendWhisper("Você precisa de uma sacola da planta para pegar esta planta!", 1);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                        return;
                    }

                    if (Item.GetRoom() != null)
                    {
                        Item.GetRoom().GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                        FarmingManager.AddEXP(Session, Random.Next(FarmingItem.MaxExp, (FarmingItem.MaxExp + 4)));
                        FarmingManager.IncreaseSatchelCount(Session, FarmingItem, 1, true);
                        Session.Shout("*Colhe o " + Item.GetBaseItem().PublicName + " e a coloca em sua mochila*", 4);
                        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Farming", 1);

                        Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                        return;
                    }
                }
                else
                {
                    var cracks = 0;
                    int.TryParse(Item.ExtraData, out cracks);
                    cracks++;

                    Item.FarmingData.BeingFarmed = true;
                    Session.Shout("*Põe um pouco de água na " + Item.GetBaseItem().PublicName + " e espera que ele cresça*", 4);
                    FarmingManager.AddEXP(Session, Random.Next(FarmingItem.MinExp, (FarmingItem.MaxExp + 1)));

                    Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);

                    #region Timer
                    new Thread(() =>
                    {
                        int count = 0;
                        while (Session != null && Item != null && Item.FarmingData != null)
                        {
                            if (Session == null || Item == null || Item.FarmingData == null)
                                break;

                            count++;
                            Thread.Sleep(1000);

                            if (count >= 10)
                                break;
                        }

                        if (count >= 10 && Session != null && Item != null && Item.FarmingData != null)
                        {
                            Item.ExtraData = Convert.ToString(cracks);
                            Item.UpdateState(false, true);

                            if (Item.ExtraData != "4")
                                Session.SendWhisper("A planta(s) " + Item.GetBaseItem().PublicName + " que você regou amadureceu um pouco!", 1);
                            else
                                Session.SendWhisper("A planta(s)" + Item.GetBaseItem().PublicName + " você regou amadureceu completamente!", 1);
                        }

                        if (Item != null && Item.FarmingData != null)
                            Item.FarmingData.BeingFarmed = false;
                    }).Start();
                    #endregion
                }

                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}