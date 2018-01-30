using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown to de-activate a spray
    /// </summary>
    public class SprayTimer : RoleplayTimer
    {
        public SprayTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 60 Seconds converted to milliseconds
            TimeLeft = 60 * 1000;
        }
 
        /// <summary>
        /// Removes the spray
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

                if (!base.Client.GetRoomUser().Frozen)
                {
                    if (!base.Client.GetRoomUser().CanWalk)
                        base.Client.GetRoomUser().CanWalk = true;

                    if (base.Client.GetRoomUser().CurrentEffect == EffectsList.Dizzy)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser().CurrentEffect != EffectsList.Dizzy && base.Client.GetRoomUser().CurrentEffect != EffectsList.Ghost && base.Client.GetRoomUser().CurrentEffect != EffectsList.Cuffed)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.Dizzy);

                TimeLeft -= 1000;

                if (TimeLeft > 0)
                    return;

                base.Client.GetRoomUser().Frozen = false;
                base.Client.GetRoomUser().CanWalk = true;
                base.Client.SendWhisper("Seus olhos deixam de queimar, os efeitos do spray de pimenta desapareceram!", 1);
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