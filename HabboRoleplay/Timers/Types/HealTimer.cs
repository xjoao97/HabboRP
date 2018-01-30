using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Begins healing the users health
    /// </summary>
    public class HealTimer : RoleplayTimer
    {

        public HealTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params) 
        {
            // Convert to milliseconds
            TimeLeft = 5 * 1000;
        }

        /// <summary>
        /// Decrease our users timer
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

                if (base.Client.GetRoleplay().CurHealth >= base.Client.GetRoleplay().MaxHealth)
                {
                    base.Client.GetRoleplay().CurHealth = base.Client.GetRoleplay().MaxHealth;
                    base.Client.GetRoleplay().BeingHealed = false;
                    base.EndTimer();
                    return;
                }

                if (!base.Client.GetRoleplay().BeingHealed)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetHabbo().CurrentRoomId != Convert.ToInt32(RoleplayData.GetData("hospital", "roomid")) && base.Client.GetHabbo().CurrentRoomId != Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2")))
                {
                    base.Client.GetRoleplay().BeingHealed = false;
                    base.Client.SendWhisper("Você deixou o hospital antes de ter sido totalmente curado!", 1);
                    base.EndTimer();
                    return;
                }

                TimeLeft -= 1000;

                if (TimeLeft > 0)
                    return;

                int NewHealth;

                if (base.Client.GetHabbo().VIPRank > 0)
                    NewHealth = Random.Next(8, 20);
                else
                    NewHealth = Random.Next(5, 16);

                int CurHealth = base.Client.GetRoleplay().CurHealth;
                int MaxHealth = base.Client.GetRoleplay().MaxHealth;

                if (CurHealth + NewHealth < MaxHealth)
                {
                    base.Client.GetRoleplay().BeingHealed = true;
                    base.Client.GetRoleplay().CurHealth += NewHealth;
                    TimeLeft = 5 * 1000;
                    base.Client.SendWhisper("Sua saúde está " + base.Client.GetRoleplay().CurHealth + "/" + base.Client.GetRoleplay().MaxHealth + "!", 1);
                    return;
                }
                else
                {
                    base.Client.GetRoleplay().BeingHealed = false;
                    base.Client.GetRoleplay().CurHealth = MaxHealth;
                    base.Client.SendWhisper("Sua saúde está cheia agora - " + base.Client.GetRoleplay().CurHealth + "/" + base.Client.GetRoleplay().MaxHealth + "!", 1);
                    base.EndTimer();
                    return;
                }
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}