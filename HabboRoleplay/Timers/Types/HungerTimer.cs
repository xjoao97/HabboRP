using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Makes the citizen get hungry over time
    /// </summary>
    public class HungerTimer : RoleplayTimer
    {
        public HungerTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {

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

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                if (base.Client.GetRoleplay().StaffOnDuty)
                    return;

                if (base.Client.GetRoleplay().AmbassadorOnDuty)
                    return;

                if (base.Client.GetRoleplay().TexasHoldEmPlayer > 0)
                    return;

                if (base.Client.GetHabbo().CurrentRoom != null)
                {
                    if (base.Client.GetHabbo().CurrentRoom.TutorialEnabled)
                        return;
                }

                TimeCount++;

                if (TimeCount < 300)
                    return;

                int AmountOfHunger = Random.Next(1, 5);
                base.Client.GetRoleplay().Hunger += AmountOfHunger;
                TimeCount = 0;

                if (base.Client.GetRoleplay().Hunger < 100)
                    return;

                base.Client.GetRoleplay().Hunger = 100;

                if (base.Client.GetRoleplay().CurHealth - 5 <= 0 && base.Client.GetRoleplay().IsJailed)
                    return;

                if (base.Client.GetRoleplay().CurHealth - 5 <= 0)
                    base.Client.GetRoleplay().CurHealth = 0;
                else
                {
                    //base.Client.SendWhisper("You have lost 5 HP as you have not ate in days! Replenish your hunger to avoid losing more health!", 1);
                    base.Client.SendMessage(new RoomNotificationComposer("hunger_high_warning", "message", "Você está com fome! Visite o Restaurante para obter comida e abaixe sua fome."));
                    base.Client.GetRoleplay().CurHealth -= 5;
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