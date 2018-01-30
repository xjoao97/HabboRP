using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboRoleplay.Timers;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Bots.Manager
{
    public class RoleplayBotCycler : BotRoleplayTimer
    {
        /// <summary>
        /// Creates new RoleplayBotCycler Instance
        /// </summary>
        public RoleplayBotCycler() : base(null, null, 1000, true, null)
        {

        }

        /// <summary>
        /// Ontick for Roleplay Bots
        /// </summary>
        public override void Execute()
        {
            try
            {
                #region Available Bot Timer Manager
                foreach (RoomUser RoleplayBot in RoleplayBotManager.DeployedRoleplayBots.Values)
                {
                    if (RoleplayBot == null)
                        continue;

                    if (RoleplayBot.GetBotRoleplay() == null)
                        continue;

                    if (!RoleplayBot.GetBotRoleplay().Deployed)
                        continue;

                    RoleplayBot.GetBotRoleplayAI().RandomSpeechTick();
                    RoleplayBot.GetBotRoleplayAI().OnTimerTick();
                }
                #endregion
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
