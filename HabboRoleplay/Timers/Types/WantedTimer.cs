using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Houses;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown to evade police
    /// </summary>
    public class WantedTimer : RoleplayTimer
    {
        public WantedTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 10 minutes converted to miliseconds
            TimeLeft = base.Client.GetRoleplay().WantedTimeLeft * 60000;
        }
 
        /// <summary>
        /// Removes the user from the wanted list
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

                if (base.Client.GetRoleplay().Jailbroken || !base.Client.GetRoleplay().IsWanted || base.Client.GetRoleplay().IsJailed)
                {
                    base.Client.GetRoleplay().IsWanted = false;
                    base.Client.GetRoleplay().WantedLevel = 0;
                    base.Client.GetRoleplay().WantedTimeLeft = 0;
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoleplay().Game != null)
                    return;

                RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(base.Client.GetHabbo().CurrentRoomId);

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                House House;
                if (roomData.TurfEnabled || base.Client.GetHabbo().CurrentRoom.TryGetHouse(out House))
                    return;

                if (base.Client.GetRoleplay().TexasHoldEmPlayer > 0)
                    return;

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetRoleplay().WantedTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        base.Client.SendWhisper("Você tem " + (TimeLeft / 60000) + " minuto(s) para escapar da polícia!", 1);
                        base.Client.SendMessage(new RoomNotificationComposer("wanted_level", "message", "Você é procurado! Você tem " + (TimeLeft / 60000) + " minuto(s) para escapar da polícia."));
                        TimeCount = 0;
                    }
                    return;
                }

                base.Client.GetRoleplay().IsWanted = false;
                base.Client.GetRoleplay().WantedLevel = 0;
                base.Client.GetRoleplay().WantedTimeLeft = 0;
                base.Client.GetRoleplay().Evasions++;
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(base.Client, "ACH_Evasions", 1);

                Wanted Junk;
                RoleplayManager.WantedList.TryRemove(base.Client.GetHabbo().Id, out Junk);
                PlusEnvironment.GetGame().GetClientManager().JailAlert("[RÁDIO] " + base.Client.GetHabbo().Username + " fugiu das autoridades! Mais sorte da próxima vez.");
                RoleplayManager.Shout(base.Client, "*Finalmente escapa da polícia após fugir por 10 minutos*", 4);
                base.Client.SendMessage(new RoomNotificationComposer("evasion_success_notice", "message", "Se considere foda! Você fugiu com sucesso das autoridades."));
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