using System.Linq;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Utilities;
using Plus.HabboHotel.Quests;
using System;
using Plus.Core;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Workout timer
    /// </summary>
    public class WorkoutTimer : RoleplayTimer
    {
        public WorkoutTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params)
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert 80 seconds to milliseconds
            TimeLeft = 80 * 1000;
        }

        /// <summary>
        /// Executes workout tick
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

                if (base.Client.GetRoomUser() == null || base.Client.GetRoomUser().GetRoom() == null)
                {
                    base.Client.GetRoleplay().IsWorkingOut = false;
                    base.EndTimer();
                    return;
                }

                int EffectId = EffectsList.CrossTrainer;
                int ItemId = (int)Params[0];
                bool Strength = (bool)Params[1];

                if (Strength)
                    EffectId = EffectsList.Treadmill;

                Item Treadmill = base.Client.GetRoomUser().GetRoom().GetRoomItemHandler().GetItem(ItemId);

                if (Treadmill == null || !base.Client.GetRoleplay().IsWorkingOut || Treadmill.Coordinate != base.Client.GetRoomUser().Coordinate)
                {
                    RoleplayManager.Shout(base.Client, "*Para de treinar por sair da máquina de treino*", 4);
                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.Client.GetRoleplay().IsWorkingOut = false;
                    base.EndTimer();
                    return;
                }

                if (!base.Client.GetRoomUser().GetRoom().GymEnabled)
                {
                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.Client.GetRoleplay().IsWorkingOut = false;
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser().IsAsleep)
                {
                    RoleplayManager.Shout(base.Client, "*Para de malhar por ficar ausente*", 4);
                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                    base.Client.GetRoleplay().IsWorkingOut = false;
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoleplay().CurEnergy <= 0)
                {
                    base.Client.SendWhisper("Você ficou sem energia! Você está muito fraco para continuar malhando!", 1);

                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.Client.GetRoleplay().IsWorkingOut = false;
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser().CurrentEffect != EffectId)
                    base.Client.GetRoomUser().ApplyEffect(EffectId);

                if (RoleplayManager.WorkoutCAPTCHABox)
                {
                    if (base.Client.GetRoleplay().CaptchaSent)
                        return;

                    if (!base.Client.GetRoleplay().CaptchaSent && base.Client.GetRoleplay().CaptchaTime >= Convert.ToInt32(RoleplayData.GetData("captcha", "workoutinterval")))
                    {
                        base.Client.GetRoleplay().CreateCaptcha("Digite o código na caixa para continuar a trabalhar!");
                        return;
                    }
                }

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeLeft > 0)
                    return;

                CryptoRandom Random = new CryptoRandom();

                int AmountToAdd;

                if (base.Client.GetHabbo().VIPRank > 0)
                    AmountToAdd = Random.Next(1, 6) * 2;
                else
                    AmountToAdd = Random.Next(1, 6);

                int Exp = AmountToAdd * 4;

                LevelManager.AddLevelEXP(base.Client, Exp);

                if (Strength)
                    LevelManager.AddStrengthEXP(base.Client, AmountToAdd);
                else
                    LevelManager.AddStaminaEXP(base.Client, AmountToAdd);

                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.WORKOUT);

                int EnergyLoss = Random.Next(RoleplayManager.WorkoutCAPTCHABox ? 2 : 4, RoleplayManager.WorkoutCAPTCHABox ? 4 : 8);

                if (base.Client.GetRoleplay().CurEnergy - EnergyLoss <= 0)
                    base.Client.GetRoleplay().CurEnergy = 0;
                else
                    base.Client.GetRoleplay().CurEnergy -= EnergyLoss;

                base.Client.SendWhisper("Você perdeu " + EnergyLoss + " energia malhando!", 1);

                if (Strength && base.Client.GetRoleplay().Strength >= RoleplayManager.StrengthCap)
                {
                    base.Client.SendWhisper("Você atingiu o nível máximo de força de: " + RoleplayManager.StrengthCap + "!", 1);
                    base.Client.GetRoleplay().IsWorkingOut = false;

                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.EndTimer();
                    return;
                }

                if (!Strength && base.Client.GetRoleplay().Stamina >= RoleplayManager.StaminaCap)
                {
                    base.Client.SendWhisper("Você alcançou o nível de resistência máxima de: " + RoleplayManager.StaminaCap + "!", 1);
                    base.Client.GetRoleplay().IsWorkingOut = false;

                    if (base.Client.GetRoomUser().CurrentEffect == EffectId)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.EndTimer();
                    return;
                }

                // Convert 80 seconds to milliseconds
                TimeLeft = 80 * 1000;
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