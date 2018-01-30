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
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Timers;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using static Plus.HabboRoleplay.Bots.Manager.TimerHandlers.TimerHandlerManager;
using Plus.HabboRoleplay.Bots.Manager.TimerHandlers;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class MafiaWarsBot : RoleplayBotAI
    {

        int VirtualId;
        CryptoRandom Rand;
        
        public MafiaWarsBot(int VirtualId)
        {
            this.VirtualId = VirtualId;
            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {
           
        }

        public override void OnDeath(GameClient Client)
        {

            bool IsBoss = (GetBotRoleplay().Motto.ToLower().Contains("boss")) ? true : false;

            if (IsBoss)
            {

                RoleplayManager.Shout(Client, "*Dá um soco em " + GetBotRoleplay().Name + ", matando-o e ganhando a Guerra de Máfias " + Client.GetRoleplay().Team.Name + "! [+5 Pontos de Eventos]*", 1);

                Client.GetHabbo().EventPoints += 5;
                Client.GetHabbo().UpdateEventPointsBalance();

                RoleplayTeam DefeatedTeam = Client.GetRoleplay().Game.GetTeams().Values.FirstOrDefault(x => x.CaptureRoom == Client.GetRoomUser().RoomId);
                Client.GetRoleplay().Game.NotifyPlayers("O mestre do time" + DefeatedTeam.Name + " foi derrotado por " + Client.GetHabbo().Username + "! A vitória pertence ao Time " + Client.GetRoleplay().Team.Name + "!");


                foreach (RoomUser RoleplayBot in GetRoom().GetRoomUserManager().GetBotList())
                {
                    if (RoleplayBot == null)
                        continue;
                    if (!RoleplayBot.IsRoleplayBot)
                        continue;
                    if (RoleplayBot.GetBotRoleplay() == null)
                        continue;
                    if (RoleplayBot.GetBotRoleplay().Invisible || (RoleplayBot.GetBotRoleplay().Dead))
                        continue;

                    RoleplayBot.GetBotRoleplay().StopAllHandlers();                   
                }

                    new Thread(() => {
                    Thread.Sleep(5000);
                    Client.GetRoleplay().Game.RemoveTeamMembers(DefeatedTeam);
                }).Start();

            }
            else
            {
                RoleplayManager.Shout(Client, "*Dá um soco em " + GetBotRoleplay().Name + ", matando-o*");

                Client.GetHabbo().EventPoints += 10;
                Client.GetHabbo().UpdateEventPointsBalance();
            }

            GetRoom().SendMessage(new UserRemoveComposer(GetRoomUser().VirtualId));
            GetBotRoleplay().Invisible = true;
            GetBotRoleplay().Dead = true;
        }

        public override void OnArrest(GameClient Client)
        {
            
        }

        public override void OnAttacked(GameClient Client)
        {

            if (this.IsNull())
                return;

            if (this.GetBotRoleplay() == null)
                return;
            
            this.StartAttacking(Client);

            if (GetBotRoleplay().Motto.ToLower().Contains("boss"))
                this.CallOtherBots(Client);

        }

        public override void OnUserLeaveRoom(GameClient Client)
        {

        }

        public override void OnUserEnterRoom(GameClient Client)
        {
            if (Client == null)
                return;

            string Team = GetBotRoleplay().Motto.ToLower().Split(' ')[0];

            if (Client.GetRoleplay().Team == null)
                return;

            if (Client.GetRoleplay().Team.Name.ToLower() == Team || Client.GetRoleplay().Game == null)
                return;

            new Thread(() =>
            {

                int Random = new CryptoRandom().Next(100, 3000);
                Thread.Sleep(Random);

                if (Client != null)
                {
                    if (Client.GetRoomUser() != null)
                    {
                        this.StartAttacking(Client);
                        //GetRoomUser().Chat("imma kill this fat bitch", true);
                    }
                }
                else { }
                   // GetRoomUser().Chat("fuck mate couldnt find a user", true);

            }).Start();
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

        public override void OnTimerTick()
        {

            if (IsNull())
                return;

            IBotHandler AttackInstance;

            if (GetBotRoleplay().Dead) return;

            if (this.ReturnAttackHandler(out AttackInstance))
            {
                if (AttackInstance.Active)
                {
                    AttackInstance.ExecuteHandler();
                    return;
                }
            }
            
            GetBotRoleplay().HandleRoaming();
           
        }

        private void FindUserToAttack(out GameClient User, bool MultiCheck)
        {
            User = null;

            if (this.IsNull())
                return;

            foreach(RoomUser RoomUser in this.GetRoom().GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUser == null) continue;
                if (RoomUser.GetClient() == null) continue;
                if (BeingAttacked(RoomUser.GetClient()) && MultiCheck) continue;
                User = RoomUser.GetClient();
            }
        }

        private bool BeingAttacked(GameClient Client)
        {
            foreach (RoomUser MafiaBot in this.GetRoom().GetRoomUserManager().GetBotList())
            {
                if (MafiaBot == this.GetRoomUser())
                    continue;

                if (MafiaBot.GetBotRoleplay() == null)
                    continue;

                if (!MafiaBot.GetBotRoleplay().Attacking)
                    continue;

                if (!MafiaBot.GetBotRoleplay().ActiveHandlers.ContainsKey(Handlers.ATTACK))
                    continue;

                if (MafiaBot.GetBotRoleplay().ActiveHandlers[Handlers.ATTACK].InteractingUser == Client)
                    return true;
            }

            return false;
        }

        private void CallOtherBots(GameClient Client)
        {        
            foreach (RoomUser MafiaBot in this.GetRoom().GetRoomUserManager().GetBotList())
            {
                if (MafiaBot == this.GetRoomUser())
                    continue;
                if (MafiaBot == null)
                    continue;
                if (MafiaBot.GetBotRoleplayAI() == null)
                    continue;
                if (MafiaBot.GetBotRoleplay() == null)
                    continue;
                IBotHandler AttackHandler;
                if (MafiaBot.GetBotRoleplay().TryGetHandler(Handlers.ATTACK, out AttackHandler))
                    continue;

                MafiaBot.GetBotRoleplayAI().OnAttacked(Client);
            }
        }

        private void StartAttacking(GameClient TargetClient)
        {
            IBotHandler AttackInstance;

            if (ReturnAttackHandler(out AttackInstance))
            {
                if (AttackInstance.InteractingUser != TargetClient)
                AttackInstance.AssignInteractingUser(TargetClient);
                AttackInstance.Active = true;
                return;
            }

            this.GetBotRoleplay().StartHandler(Handlers.ATTACK, out AttackInstance, TargetClient);
            AttackInstance.SetValues("attack_pos", Convert.ToString(this.GetBotRoleplay().DefaultAttackPosition));
        }

        private bool ReturnAttackHandler(out IBotHandler AttackInstance)
        {
            AttackInstance = null;
            if (this.GetBotRoleplay() == null) return false;
            if (this.GetBotRoleplay().ActiveHandlers == null) return false;
            if (!this.GetBotRoleplay().ActiveHandlers.ContainsKey(Handlers.ATTACK)) return false;

            AttackInstance = this.GetBotData().ActiveHandlers[Handlers.ATTACK];

            return true;
        }

    }
}
