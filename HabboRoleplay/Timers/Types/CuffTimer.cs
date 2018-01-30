using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown to break handcuffs
    /// </summary>
    public class CuffTimer : RoleplayTimer
    {
        public CuffTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 8 minutes converted to miliseconds
            TimeLeft = base.Client.GetRoleplay().CuffedTimeLeft * 60000;
            TimeCount = 60 * (8 - base.Client.GetRoleplay().CuffedTimeLeft);
        }
 
        /// <summary>
        /// Removes the cuff
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

                if (!base.Client.GetRoleplay().Cuffed || base.Client.GetRoleplay().CuffedTimeLeft == 0)
                {
                    base.EndTimer();
                    return;
                }

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60 || TimeCount == 60 * 2 || TimeCount == 60 * 3 || TimeCount == 60 * 4 || TimeCount == 60 * 5 || TimeCount == 60 * 6 || TimeCount == 60 * 7)
                {
                    if (TimeCount == 60)
                        base.Client.SendWhisper("Você começa a lutar contra os punhos, tentando se libertar das algemas!", 1);
                    else if (TimeCount == 60 * 2)
                        base.Client.SendWhisper("Seus pulsos machucam quando você continua tentando se livrar das algemas", 1);
                    else if (TimeCount == 60 * 3)
                        base.Client.SendWhisper("Seus pulsos começam a machucar na forma dos punhos!", 1);
                    else if (TimeCount == 60 * 4)
                        base.Client.SendWhisper("Você geme enquanto a algema começa a pressionar contra o polegar!", 1);
                    else if (TimeCount == 60 * 5)
                        base.Client.SendWhisper("Você ouve um *crack* enquanto seu polegar direito se desloca!", 1);
                    else if (TimeCount == 60 * 6)
                        base.Client.SendWhisper("Você desliza a mão direita dolorida da algema!", 1);
                    else if (TimeCount == 60 * 7)
                        base.Client.SendWhisper("Você coloca o clipe de papel dentro da algema esquerda, tentando libertar-se!", 1);

                    base.Client.GetRoleplay().CuffedTimeLeft--;
                }

                if (TimeLeft > 0)
                    return;

                base.Client.GetRoleplay().Cuffed = false;
                base.Client.GetRoleplay().CuffedTimeLeft = 0;
                RoleplayManager.Shout(base.Client, "*Se solta de suas algemas depois de lutar por tanto tempo!*", 4);
                base.EndTimer();
            }
            catch(Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}