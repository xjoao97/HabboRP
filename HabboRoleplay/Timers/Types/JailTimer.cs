using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Prison timer
    /// </summary>
    public class JailTimer : RoleplayTimer
    {
        public JailTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            OriginalTime = Client.GetRoleplay().WantedLevel * 5;
            TimeLeft = Client.GetRoleplay().JailedTimeLeft * 60000;
            
            Client.GetRoleplay().UpdateTimerDialogue("Jail-Timer", "add", Client.GetRoleplay().JailedTimeLeft, OriginalTime);            

        }

        /// <summary>
        /// Prison timer
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

                if (!base.Client.GetRoleplay().IsJailed)
                {
                    Client.GetRoleplay().UpdateTimerDialogue("Jail-Timer", "remove", Client.GetRoleplay().JailedTimeLeft, OriginalTime);

                    if (base.Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid")))
                        RoleplayManager.SpawnChairs(base.Client, "room_wl15_sofa");

                    if (base.Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")))
                        RoleplayManager.SpawnChairs(base.Client, "es_bench");

                    RoleplayManager.Shout(base.Client, "*Obtém-se liberdade da prisão*", 4);

                    if (base.Client.GetRoleplay().JailedTimeLeft != -5)
                    {
                        base.Client.GetRoleplay().OnProbation = true;
                        base.Client.GetRoleplay().ProbationTimeLeft = 5;
                        base.Client.GetRoleplay().TimerManager.CreateTimer("probation", 1000, false);
                        base.Client.SendWhisper("Você foi colocado em liberdade condicional " + base.Client.GetRoleplay().ProbationTimeLeft + " minutos!", 1);
                        base.Client.SendWhisper("Se você cometer algum crime durante a liberdade condicional, você será procurado um nível de estrela extra!", 1);
                    }

                    base.Client.GetRoleplay().WantedFor = "";
                    base.Client.GetRoleplay().Trialled = false;
                    base.Client.GetRoleplay().Jailbroken = false;
                    base.Client.GetRoleplay().IsJailed = false;
                    base.Client.GetRoleplay().JailedTimeLeft = 0;
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoleplay().Jailbroken)
                    return;

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetRoleplay().JailedTimeLeft--;

                if (base.Client.GetRoleplay().Cuffed)
                    base.Client.GetRoleplay().Cuffed = false;

                if (RoleplayManager.WantedList.ContainsKey(base.Client.GetHabbo().Id))
                {
                    Wanted Junk;
                    RoleplayManager.WantedList.TryRemove(base.Client.GetHabbo().Id, out Junk);
                }

                if (base.Client.GetRoleplay().IsWanted || base.Client.GetRoleplay().WantedLevel != 0 || base.Client.GetRoleplay().WantedTimeLeft != 0)
                {
                    base.Client.GetRoleplay().IsWanted = false;
                    base.Client.GetRoleplay().WantedLevel = 0;
                    base.Client.GetRoleplay().WantedTimeLeft = 0;
                }

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        Client.GetRoleplay().UpdateTimerDialogue("Jail-Timer", "decrement", Client.GetRoleplay().JailedTimeLeft, OriginalTime);            
                        base.Client.SendWhisper("Você tem " + base.Client.GetRoleplay().JailedTimeLeft + " minuto(s) até você ser libertado da prisão!", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                if (base.Client.GetRoomUser() != null)
                {
                    if (base.Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid")))
                        RoleplayManager.SpawnChairs(base.Client, "room_wl15_sofa");

                    if (base.Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")))
                        RoleplayManager.SpawnChairs(base.Client, "es_bench");
                }

                base.Client.GetRoleplay().OnProbation = true;
                base.Client.GetRoleplay().ProbationTimeLeft = 5;
                base.Client.SendWhisper("Você foi colocado em liberdade condicional por " + base.Client.GetRoleplay().ProbationTimeLeft + " minutos!", 1);
                base.Client.SendWhisper("Se você cometer algum crime durante a liberdade condicional, você será procurado um nível de estrela extra!", 1);
                base.Client.GetRoleplay().TimerManager.CreateTimer("probation", 1000, false);

                base.Client.GetRoleplay().WantedFor = "";
                base.Client.GetRoleplay().Trialled = false;
                base.Client.GetRoleplay().Jailbroken = false;
                base.Client.GetRoleplay().IsJailed = false;
                base.Client.GetRoleplay().JailedTimeLeft = 0;
                Client.GetRoleplay().UpdateTimerDialogue("Jail-Timer", "remove", Client.GetRoleplay().JailedTimeLeft, OriginalTime);
                RoleplayManager.Shout(base.Client, "*Obtém-se liberdade da prisão, por cumprir a sentença*", 4);
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