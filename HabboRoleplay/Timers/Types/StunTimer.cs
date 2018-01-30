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
    /// Countdown to de-activate a stun
    /// </summary>
    public class StunTimer : RoleplayTimer
    {
        public StunTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 60 seconds converted to miliseconds
            TimeLeft = 60 * 1000;
        }
 
        /// <summary>
        /// Removes the stun
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
                        base.Client.GetRoomUser().ApplyEffect(0);

                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser().CurrentEffect != EffectsList.Dizzy && base.Client.GetRoomUser().CurrentEffect != EffectsList.Ghost && base.Client.GetRoomUser().CurrentEffect != EffectsList.Cuffed)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.Dizzy);

                TimeLeft -= 1000;

                if (TimeLeft > 0)
                    return;

                if (base.Client.GetRoomUser().CurrentEffect == EffectsList.Dizzy)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                base.Client.GetRoomUser().Frozen = false;
                base.Client.GetRoomUser().CanWalk = true;
                base.Client.SendWhisper("Seu corpo para de se sentir entorpecido, os efeitos do atordoamento desapareceram!", 1);
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