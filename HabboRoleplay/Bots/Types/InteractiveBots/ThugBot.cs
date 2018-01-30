using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Bots.Types;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.Utilities;
using System.Threading;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Combat;
using Plus.HabboHotel.Quests;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class ThugBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;

        public ThugBot(int VirtualId)
        {
            this.VirtualId = VirtualId;
            Rand = new CryptoRandom();
        }


        public override void OnDeployed(GameClient Client)
        {

        }

        public override void OnDeath(GameClient Client)
        {
            int Amount = CombatManager.GetCombatType("fist").GetCoins(null, GetBotRoleplay());

            Client.GetHabbo().Credits += Amount;
            Client.GetHabbo().UpdateCreditsBalance();

            CombatManager.GetCombatType("fist").GetRewards(Client, null, GetBotRoleplay());

            if (Amount > 0)
                RoleplayManager.Shout(Client, "*Dá um nocaute em " + GetBotRoleplay().Name + ", matando-o e roubando R$" + Amount + " da sua carteira*", 6);
            else
                RoleplayManager.Shout(Client, "*Dá um nocaute em  " + GetBotRoleplay().Name + ", matando-o e não consegue roubar, pois a carteira está vazia*", 6);


            GetBotRoleplay().InitiateDeath();
        }

        public override void OnArrest(GameClient Client)
        {

        }

        public override void OnAttacked(GameClient Client)
        {

            GetBotRoleplay().UserAttacking = Client;

            if (!GetBotRoleplay().ActiveTimers.ContainsKey("attack"))
            {

                GetBotRoleplay().ActiveTimers.TryAdd("attack", GetBotRoleplay().TimerManager.CreateTimer("attack", GetBotRoleplay(), 10, true, Client.GetHabbo().Id));

                if (GetBotRoleplay().UserAttacking == null)
                    GetRoomUser().Chat("Seu desgraçado! Eu vou pegar você " + Client.GetHabbo().Username + "!", true, 4);
            }
            else
            {
                if (GetBotRoleplay().ActiveTimers["attack"] == null)
                    GetBotRoleplay().ActiveTimers["attack"] = GetBotRoleplay().TimerManager.CreateTimer("attack", GetBotRoleplay(), 10, true, Client.GetHabbo().Id);
            }

        }

        public override void OnUserLeaveRoom(GameClient Client)
        {

        }

        public override void OnUserEnterRoom(GameClient Client)
        {

        }

        public override void OnUserUseTeleport(GameClient Client, object[] Params)
        {
            if (Client == null || Client.GetRoomUser() == null)
                return;
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
            HandleRequest(User.GetClient(), Message);
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
            HandleRequest(User.GetClient(), Message);
        }

        public override void HandleRequest(GameClient Client, string Message)
        {
            if (RespondToSpeech(Client, Message))
                return;
        }

        public override void StartActivities()
        {

        }

        public override void StopActivities()
        {

        }

        public override void OnMessaged(GameClient Client, string Message)
        {

        }

    }
}
