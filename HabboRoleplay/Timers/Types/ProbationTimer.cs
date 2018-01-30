using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown for probation
    /// </summary>
    public class ProbationTimer : RoleplayTimer
    {
        public ProbationTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            TimeLeft = base.Client.GetRoleplay().ProbationTimeLeft * 60000;
        }
 
        /// <summary>
        /// Removes the user from probation
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetRoleplay() == null)
                {
                    base.EndTimer();
                    return;
                }

                if (!base.Client.GetRoleplay().OnProbation)
                {
                    base.Client.GetRoleplay().OnProbation = false;
                    base.Client.GetRoleplay().ProbationTimeLeft = 0;
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetRoleplay().ProbationTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        base.Client.SendWhisper("Você tem " + (TimeLeft / 60000) + " minuto(s) até que você seja removido da liberdade condicional!", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                base.Client.GetRoleplay().OnProbation = false;
                base.Client.GetRoleplay().ProbationTimeLeft = 0;
                RoleplayManager.Shout(base.Client, "*Completa seu tempo e é retirado da liberdade condicional*", 4);
                base.EndTimer();
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}