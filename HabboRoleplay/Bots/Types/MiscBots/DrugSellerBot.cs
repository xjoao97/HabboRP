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

namespace Plus.HabboRoleplay.Bots.Types
{
    public class DrugSellerBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;

        public DrugSellerBot(int VirtualId)
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
            int WeedCost = Convert.ToInt32(RoleplayData.GetData("drugs", "weedcost"));
            int CocaineCost = Convert.ToInt32(RoleplayData.GetData("drugs", "cocainecost"));

            if (Message.ToLower() == Name)
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", você quer me vender drogas?", true);
            else if (Message.ToLower() == "droga" || Message.ToLower() == "drogas" || Message.ToLower() == "valor")
            {
                string WhisperMessage = "Atualmente, estou comprando 10g de maconha por R$" + String.Format("{0:N0}", WeedCost) + " e 10g de cocaína por R$" + String.Format("{0:N0}", CocaineCost) + "!";
                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
            }
            else if (Message.ToLower() == "maconha")
            {
                if (Client.GetRoleplay().Weed < 15)
                {
                    string WhisperMessage = "Você não tem 15g de maconha para me vender!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                Client.GetRoleplay().Weed -= 10;
                Client.GetHabbo().Credits += WeedCost;
                Client.GetHabbo().UpdateCreditsBalance();
                GetRoomUser().Chat("*Compra 10g de maconha de " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", WeedCost) + "*", true);
            }
            else if (Message.ToLower() == "cocaina")
            {
                if (Client.GetRoleplay().Cocaine < 15)
                {
                    string WhisperMessage = "Você não tem 15g de cocaína para me vender!";
                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    return;
                }

                Client.GetRoleplay().Cocaine -= 15;
                Client.GetHabbo().Credits += CocaineCost;
                Client.GetHabbo().UpdateCreditsBalance();
                GetRoomUser().Chat("*Compra 15g de cocaína de " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", CocaineCost) + "*", true);
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