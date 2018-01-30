using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Waits specified time then releases user from hospital
    /// </summary>
    public class DeathTimer : RoleplayTimer
    {

        public DeathTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params) 
        {
            // Convert to milliseconds
            OriginalTime = RoleplayManager.DeathTime;
            TimeLeft = Client.GetRoleplay().DeadTimeLeft * 60000;
            Client.GetRoleplay().UpdateTimerDialogue("Dead-Timer", "add", Client.GetRoleplay().DeadTimeLeft, OriginalTime);            
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

                if (base.Client.GetRoomUser() == null)
                    return;

                if (!base.Client.GetRoleplay().IsDead)
                {
                    Client.GetRoleplay().UpdateTimerDialogue("Dead-Timer", "remove", Client.GetRoleplay().DeadTimeLeft, OriginalTime);

                    if (base.Client.GetRoomUser().Frozen)
                        base.Client.GetRoomUser().Frozen = false;

                    if (base.Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2")))
                        RoleplayManager.SpawnChairs(base.Client, "pura_mdl3*4");

                    base.Client.GetRoleplay().BeingHealed = false;
                    base.Client.GetRoleplay().IsDead = false;
                    base.Client.GetRoleplay().DeadTimeLeft = 0;
                    base.Client.GetRoleplay().ReplenishStats(true);
                    base.EndTimer();
                    return;
                }

                if (!base.Client.GetRoomUser().Frozen)
                    base.Client.GetRoomUser().Frozen = true;

                if (base.Client.GetRoleplay().BeingHealed)
                    return;

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetRoleplay().DeadTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        Client.GetRoleplay().UpdateTimerDialogue("Dead-Timer", "decrement", Client.GetRoleplay().DeadTimeLeft, OriginalTime);
                        base.Client.SendWhisper("Você tem " + base.Client.GetRoleplay().DeadTimeLeft + " minuto(s) para ser liberado do hospital", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                if (base.Client.GetRoomUser().Frozen)
                    base.Client.GetRoomUser().Frozen = false;

                if (base.Client.GetRoomUser().RoomId == Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2")))
                    RoleplayManager.SpawnChairs(base.Client, "pura_mdl3*4");

                Client.GetRoleplay().UpdateTimerDialogue("Dead-Timer", "remove", Client.GetRoleplay().DeadTimeLeft, OriginalTime);

                RoleplayManager.Shout(base.Client, "*Recupera a consciência*", 4);
                Client.SendWhisper("Seu visual voltará ao original após sair do quarto!", 1);
                base.Client.GetRoleplay().BeingHealed = false;
                base.Client.GetRoleplay().IsDead = false;
                base.Client.GetRoleplay().DeadTimeLeft = 0;
                base.Client.GetRoleplay().ReplenishStats(true);
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