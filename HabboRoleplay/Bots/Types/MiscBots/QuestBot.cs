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
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class QuestBot : RoleplayBotAI
    {

        int VirtualId;
        CryptoRandom Rand;

        public QuestBot(int VirtualId)
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
                RoleplayManager.Shout(Client, "*Dá um soco em " + GetBotRoleplay().Name + ", matando-o e roubando R$" + Amount + " de sua carteira*", 6);
            else
                RoleplayManager.Shout(Client, "*Dá um soco em " + GetBotRoleplay().Name + ", matando-o mas não consegue roubar, pois a carteira está vazia", 6);


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
            List<string> Replies = new List<string>();
            Replies.Add("O que você quer?");
            Replies.Add("Você pode parar de me enviar mensagens??");
            Replies.Add("Estou bastante ocupado, vamos falar depois..");
            Replies.Add("Cara, vai se foder? obrigado.");
            Replies.Add("Uh....");

            string Reply = Replies[new CryptoRandom().Next(0, Replies.Count - 1)];
            int ReplyTime = new CryptoRandom().Next(3000, 7000);

            new Thread(() =>
            {
                Thread.Sleep(ReplyTime);
                GetBotRoleplay().MessageFriend(Client.GetHabbo().Id, Reply);
            }).Start();
        }

    }
}
