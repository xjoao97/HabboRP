using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Noob timer
    /// </summary>
    public class NoobTimer : RoleplayTimer
    {
        public NoobTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 15 minutes convert to milliseconds
            TimeLeft = Client.GetRoleplay().NoobTimeLeft * 60000;
        }
 
        /// <summary>
        /// Increases the users hunger
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

                if (!base.Client.GetRoleplay().IsNoob)
                    return;

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                if (base.Client.GetHabbo().CurrentRoom != null)
                {
                    if (base.Client.GetHabbo().CurrentRoom.TutorialEnabled)
                        return;
                }

                if (base.Client.GetHabbo().CurrentRoom == null)
                    return;

                if (base.Client.GetHabbo().CurrentRoomId <= 0)
                    return;

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetRoleplay().NoobTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        base.Client.SendWhisper("Você tem " + (TimeLeft / 60000) + " minutos(s) de proteção, após acabar, ande em locais de segurança!", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                base.Client.SendNotification("Sua proteção de Deus fornecida pelo deus agora acabou, você deve poder cuidar de si mesmo agora.\n\nMantenha-se seguro e boa sorte!");
                base.Client.GetRoleplay().IsNoob = false;
                base.Client.GetRoleplay().NoobTimeLeft = 0;
                base.EndTimer();
                return;

            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}