using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Sendhome timer
    /// </summary>
    public class SendhomeTimer : RoleplayTimer
    {
        public SendhomeTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params)
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            TimeLeft = base.Client.GetRoleplay().SendHomeTimeLeft * 60000;
        }

        /// <summary>
        /// Pays user after shift
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

                if (base.Client.GetRoleplay().SendHomeTimeLeft <= 0)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetRoleplay().SendHomeTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        base.Client.SendWhisper("Você tem " + (TimeLeft / 60000) + " minuto(s) restantes até a sua duração de enviado para casa expirar!", 1);
                        TimeCount = 0;

                        if (base.Client.GetRoleplay().SendHomeTimeLeft * 60000 != TimeLeft)
                        {
                            TimeLeft = base.Client.GetRoleplay().SendHomeTimeLeft * 60000;
                            base.Client.SendWhisper("A sua duração de tempo em casa mudou! Você agora tem " + (TimeLeft / 60000) + " minutos restantes!", 1);
                        }
                    }
                    return;
                }

                base.Client.GetRoleplay().SendHomeTimeLeft = 0;
                RoleplayManager.Shout(base.Client, "*Completa a duração de ficar em caa e finalmente, pode voltar ao trabalho*", 4);

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