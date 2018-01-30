using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Bots.Types;
using Plus.HabboRoleplay.Bots;

namespace Plus.HabboRoleplay.Bots.Types
{
    /// <summary>
    /// This class is to be used as a skeleton by which to create new Bot AI Types
    /// </summary>
    public class BlankBot : RoleplayBotAI
    {
        int VirtualId;

        public BlankBot(int VirtualId)
        {
            this.VirtualId = VirtualId;
        }

        public override void OnDeployed(GameClient Client)
        {
            this.StartActivities();
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

        }

        public override void OnUserSay(RoomUser User, string Message)
        {

        }

        public override void OnUserShout(RoomUser User, string Message)
        {

        }

        public override void OnMessaged(GameClient Client, string Message)
        {

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
    }
}
