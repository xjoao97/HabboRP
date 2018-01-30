using System;
using System.Linq;
using System.Text;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboRoleplay.Farming;
using Plus.HabboHotel.Rooms.Chat.Commands;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class SupermarketBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;

        public SupermarketBot(int VirtualId)
        {
            this.OnDuty = true;
            this.CheckForOtherWorkers = true;
            this.CurOnDutyCheckTime = 0;
            this.VirtualId = VirtualId;

            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {

        }

        public override void OnDeath(GameClient Client)
        {

        }

        public override void OnArrest(GameClient Client)
        {

        }

        public override void OnAttacked(GameClient Client)
        {

        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
            if (!OnDuty)
                return;
        }

        public override void OnUserEnterRoom(GameClient Client)
        {
            if (!OnDuty)
                return;

            if (!GetRoomUser().IsWalking)
            {
                // Look at the user 
            }
        }

        public override void OnUserUseTeleport(GameClient Client, object[] Params)
        {
            if (!OnDuty)
                return;

            if (Client == null) return;
            if (Client.GetRoomUser() == null) return;

            if (Client == GetBotRoleplay().UserFollowing || Client == GetBotRoleplay().UserAttacking)
                GetBotRoleplay().StartTeleporting(GetRoomUser(), GetRoom(), Params);
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
            if (!OnDuty)
                return;

            GameClient Client = User.GetClient();

            if (Client == null)
                return;
            HandleRequest(Client, Message);
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
            if (!OnDuty)
                return;

            if (User.GetClient() == null)
                return;
            HandleRequest(User.GetClient(), Message);
        }

        public override void OnMessaged(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;
        }

        public override void HandleRequest(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;

            if (GetBotRoleplay().WalkingToItem)
                return;

            if (RespondToSpeech(Client, Message))
                return;

            string Name = GetBotRoleplay().Name.ToLower();

            string[] Params = Message.Split(' ');

            #region Satchel
            if (Message.ToLower() == "saco")
            {
                string WhisperMessage = "Por favor digite 'saco de sementes' para comprar uma sacola de sementes, ou 'saco de plantas' para comprar!";
                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                return;
            }
            #endregion

            #region Plant Satchel
            if (Message.StartsWith("sacodeplantas"))
            {
                if (Client.GetRoleplay().FarmingStats.HasPlantSatchel)
                {
                    string WhisperMessage = "Desculpe, você já tem um Saco de Plantas!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                if (Client.GetRoleplay().OfferManager.ActiveOffers.Values.Where(x => x.Type.ToLower() == "plantsatchel").ToList().Count > 0)
                {
                    string WhisperMessage = "Desculpe, mas você já recebeu uma oferta de sacola de plantas!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                int Cost = Convert.ToInt32(RoleplayData.GetData("farming", "plantsatchelcost"));

                if (Client.GetHabbo().Credits < Cost)
                {
                    string WhisperMessage = "Desculpe, você não tem R$" + String.Format("{0:N0}", Cost) + " para comprar o Saco de Plantas!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                GetRoomUser().Chat("*Oferece um Saco de Plantas para " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                Client.GetRoleplay().OfferManager.CreateOffer("sacodeplantas", 0, Cost, this);
                Client.SendWhisper("Você recebeu uma oferta de Saco de Plantas por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar sacodeplantas' para comprar!", 1);
                return;
            }
            #endregion

            #region Seed Satchel
            if (Message.StartsWith("sacodesementes"))
            {
                if (Client.GetRoleplay().FarmingStats.HasSeedSatchel)
                {
                    string WhisperMessage = "Desculpe, você já possui um Saco de sementes!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                if (Client.GetRoleplay().OfferManager.ActiveOffers.Values.Where(x => x.Type.ToLower() == "seedsatchel").ToList().Count > 0)
                {
                    string WhisperMessage = "Desculpe, mas você já recebeu uma oferta de Saco de sementes!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                int Cost = Convert.ToInt32(RoleplayData.GetData("farming", "seedsatchelcost"));

                if (Client.GetHabbo().Credits < Cost)
                {
                    string WhisperMessage = "Desculpe, você não tem R$" + String.Format("{0:N0}", Cost) + " para comprar um Saco de Sementes!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                GetRoomUser().Chat("*Oferece um Saco de sementes para " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                Client.GetRoleplay().OfferManager.CreateOffer("sacodesementes", 0, Cost, this);
                Client.SendWhisper("Você recebe uma oferta de Saco de Sementes por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar sacodesementes' para comprar!", 1);
                return;
            }
            #endregion

            #region Seeds
            if (Message.ToLower() == "seeds" || Message.ToLower() == "seed")
            {
                GetRoomUser().Chat("Welcome " + Client.GetHabbo().Username + " to the supermarket!", true);

                StringBuilder FarmingList = new StringBuilder().Append("--- Seeds For Sale ---\n");
                FarmingList.Append("To buy any of the following, type 'seed <id> <amount>'!\n\n");

                foreach (FarmingItem Item in FarmingManager.FarmingItems.Values.OrderBy(x => x.Id))
                {
                    if (Item != null)
                    {
                        ItemData Furni;
                        if (PlusEnvironment.GetGame().GetItemManager().GetItem(Item.BaseItem, out Furni))
                        {
                            FarmingList.Append("--- " + Furni.PublicName + " [" + Item.Id + "] ---\n");
                            FarmingList.Append("Farming Level Required: " + Item.LevelRequired + "\n");
                            FarmingList.Append("Cost: $" + String.Format("{0:N0}", Item.BuyPrice) + "\n\n");
                        }
                    }
                }

                Client.SendMessage(new MOTDNotificationComposer(FarmingList.ToString()));
                return;
            }
            #endregion

            #region Buying Seeds
            if (Params[0].ToLower() == "seed" && Params.Length > 2)
            {
                int Id;
                if (!int.TryParse(Params[1], out Id))
                {
                    string WhisperMessage = "Please type 'seed <id> <amount>' to buy some seeds, or type 'seeds' to see what I have for sale!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                FarmingItem Item = FarmingManager.GetFarmingItem(Id);

                ItemData Furni;
                if (!PlusEnvironment.GetGame().GetItemManager().GetItem(Item.BaseItem, out Furni) || Item == null)
                {
                    string WhisperMessage = "Sorry, but there is no seed for sale with that id!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                int Amount;
                if (!int.TryParse(Params[2], out Amount))
                {
                    string WhisperMessage = "Please type 'seed <id> <amount>' to buy some seeds, or type 'seeds' to see what I have for sale!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                if (Client.GetRoleplay().OfferManager.ActiveOffers.Values.Where(x => x.Type.ToLower() == "seed").ToList().Count > 0)
                {
                    string WhisperMessage = "Sorry, but you already been offered some seeds!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                if (!Client.GetRoleplay().FarmingStats.HasSeedSatchel)
                {
                    string WhisperMessage = "You don't have a seed satchel to carry any seeds! Type 'seed satchel' to buy one.";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                if (Item.LevelRequired > Client.GetRoleplay().FarmingStats.Level)
                {
                    string WhisperMessage = "Sorry, but you do not have a high enough farming level for this seed type!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                int Cost = (Amount * Item.BuyPrice);
                if (Client.GetHabbo().Credits < Cost)
                {
                    string WhisperMessage = "You don't have $" + String.Format("{0:N0}", Cost) + " to buy " + Amount + " " + Furni.PublicName + "'s!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                object[] Objects = new object[] { this, Item };
                GetRoomUser().Chat("*Offers " + Amount + " " + Furni.PublicName + " seeds to " + Client.GetHabbo().Username + " for $" + String.Format("{0:N0}", Cost) + "*", true);
                Client.GetRoleplay().OfferManager.CreateOffer("sementes", 0, Amount, Objects);
                Client.SendWhisper("You have just been offered " + Amount + " " + Furni.PublicName + " seeds for $" + String.Format("{0:N0}", Cost) + "! Type ':accept seeds' to buy them!", 1);
                return;
            }
            #endregion
        }

        public override void StopActivities()
        {
            if (!OnDuty)
                return;

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("startwork"))
                GetBotRoleplay().TimerManager.ActiveTimers["startwork"].EndTimer();

            GetRoomUser().Chat("Que bom que você chegou, meu turno já está feito, até mais!", true);
            OnDuty = false;
            GetBotRoleplay().WalkingToItem = false;

            if (GetBotRoleplay().WorkUniform != "none")
                GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            Item Item;
            if (GetBotRoleplay().GetStopWorkItem(this.GetRoom(), out Item))
            {
                var Point = new System.Drawing.Point(Item.GetX, Item.GetY);
                GetRoomUser().MoveTo(Point);
                GetBotRoleplay().TimerManager.CreateTimer("stopwork", GetBotRoleplay(), 10, true, null);
            }
        }

        public override void StartActivities()
        {
            if (OnDuty)
                return;

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("stopwork"))
                GetBotRoleplay().TimerManager.ActiveTimers["stopwork"].EndTimer();

            GetBotRoleplay().Invisible = false;
            GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            if (GetRoomUser() == null)
                return;

            GetRoomUser().Chat("Merda! Hora de voltar ao trabalho!", true);
            OnDuty = true;

            if (GetBotRoleplay().WorkUniform != "none")
                GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            Item Item;
            if (GetBotRoleplay().GetStopWorkItem(this.GetRoom(), out Item))
            {
                var ItemPoint = new System.Drawing.Point(Item.GetX, Item.GetY);
                if (GetRoomUser().Coordinate == ItemPoint)
                {
                    Item.ExtraData = "2";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(2, true);
                }
            }

            var Point = new System.Drawing.Point(GetBotRoleplay().oX, GetBotRoleplay().oY);
            GetRoomUser().MoveTo(Point);
            GetBotRoleplay().TimerManager.CreateTimer("startwork", GetBotRoleplay(), 10, true, null);
        }
    }
}