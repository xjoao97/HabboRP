using System;
using System.Linq;
using System.Text;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Guides;
using Plus.HabboRoleplay.Farming;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class PlantSellerBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;

        public PlantSellerBot(int VirtualId)
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

            if (Message.ToLower() == Name)
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", Você quer vender algumas plantas?", true);
            else if (Message.ToLower() == "sim" || Message.ToLower() == "planta" || Message.ToLower() == "plantas" || Message.ToLower() == "vender")
            {
                StringBuilder Plants = new StringBuilder().Append("<----- Preços das plantas ----->\n");
                Plants.Append("Digite 'vender plantas' para me vender todas as suas plantas!\n\n");

                foreach (var Item in FarmingManager.FarmingItems.Values)
                {
                    if (Item == null)
                        continue;

                    ItemData Furni;
                    if (PlusEnvironment.GetGame().GetItemManager().GetItem(Item.BaseItem, out Furni))
                    {
                        Plants.Append("<----- " + Furni.PublicName + " ----->\n");
                        Plants.Append("Lucro: " + String.Format("{0:N0}", Item.SellPrice) + " por planta \n\n");
                    }
                }

                GetRoomUser().Chat("Aqui está " + Client.GetHabbo().Username + ", esta a lista de plantas que eu estou comprando!", true);
                Client.SendMessage(new MOTDNotificationComposer(Plants.ToString()));
            }
            else if (Message.ToLower() == "vender plantas")
            {
                int Amount = FarmingManager.SellPlants(Client);

                if (Amount > 0)
                {
                    Client.GetHabbo().Credits += Amount;
                    Client.GetHabbo().UpdateCreditsBalance();
                    RoleplayManager.Shout(Client, "*Vende todas as suas plantas para " + this.GetBotRoleplay().Name + "*", 4);
                    Client.SendWhisper("Você acabou de ganhar R$" + String.Format("{0:N0}", Amount) + " por vender suas plantas!", 1);
                    GetRoomUser().Chat("Obrigado por todas as suas plantas " + Client.GetHabbo().Username + "!", true);
                    return;
                }
                else
                {
                    string WhisperMessage = "Você não tem plantas para me vender, " + Client.GetHabbo().Username + "!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }
            }
        }

        public override void StopActivities()
        {

        }

        public override void StartActivities()
        {

        }

    }
}