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
    public class JuryBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;

        public JuryBot(int VirtualId)
        {
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

        public override void OnTimerTick()
        {
            if (GetBotRoleplay() == null)
                return;
            
            #region Handle Trial
            if (RoleplayManager.CourtTrialStarted)
            {
                GetBotRoleplay().HandleJuryCase(GetRoomUser(), GetRoom());
                return;
            }
            #endregion
        }

    }
}