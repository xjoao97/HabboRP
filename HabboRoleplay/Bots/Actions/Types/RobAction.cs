using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboRoleplay.Bots.Actions.Types
{
    public class RobAction : RoleplayBotAction
    {
        private bool RobbedUser = false;
        public GameClient Robbing;
        public RobAction(RoomUser Bot, GameClient Robbing) : base(Bot)
        {
            base.RoleplayBot = Bot;
            base.ActionUserInteractors.TryAdd("robbing", Robbing);
            this.Robbing = base.ActionUserInteractors.Values.First();
        }

        public override void TickAction()
        {

            #region Inhibit checks
            if (base.ActionUserInteractors.Count <= 0)
            {
                base.StopAction();
                return;
            }

            if (base.RoleplayBot == null)
            {
                base.StopAction();
                return;
            }

            if (base.RoleplayBot.GetRoom() == null)
            {
                base.StopAction();
                return;
            }
            
            if (this.Robbing == null)
            {
                base.StopAction();
                return;
            }

            if (this.Robbing.GetRoomUser() == null)
            {
                base.StopAction();
                return;
            }
            #endregion

            #region Execute action
            if (!this.RobbedUser)
            {
                //start robbing this bitch
            }
            else
                base.StopAction();
            #endregion
        }

    }
}
