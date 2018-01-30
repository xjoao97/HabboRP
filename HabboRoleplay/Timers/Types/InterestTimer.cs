using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Utilities;
using Plus.Core;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Increases bank savings amount over time
    /// </summary>
    public class InterestTimer : RoleplayTimer
    {
        public InterestTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {

        }

        /// <summary>
        /// Increases bank savings amount over time
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

                if (base.Client.GetRoleplay().BankAccount < 2)
                    return;

                if (base.Client.GetRoleplay().BankSavings < 300)
                    return;

                if (base.Client.GetRoleplay().IsDead || base.Client.GetRoleplay().IsJailed)
                    return;

                if (base.Client.GetHabbo().CurrentRoom == null)
                    return;

                if (base.Client.GetHabbo().CurrentRoomId <= 0)
                    return;

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                if (base.Client.GetRoleplay().CaptchaSent)
                    return;

                if (!base.Client.GetRoleplay().CaptchaSent && base.Client.GetRoleplay().CaptchaTime >= Convert.ToInt32(RoleplayData.GetData("captcha", "interestinterval")))
                {
                    base.Client.GetRoleplay().CreateCaptcha("Digite o código na caixa para continuar colecionando juros!");
                    return;
                }

                TimeCount++;

                // 45 Minutes into seconds
                if (TimeCount < 2700)
                    return;

                TimeCount = 0;

                CryptoRandom Random = new CryptoRandom();
                int Chance = Random.Next(1, 51);
                double InterestRate = 0.002;

                /*if (Chance >= 100)
                    InterestRate = 0.008;
                else*/ if (Chance <= 10)
                    InterestRate = 0.006;
                else
                    InterestRate = 0.002;

                int Interest = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(base.Client.GetRoleplay().BankSavings) * InterestRate));

                if (Interest > 125)
                    Interest = 125;

                base.Client.GetRoleplay().BankSavings += Interest;
                base.Client.SendWhisper("Você recebeu uma notificação no seu telefone, sua conta de poupança ganhou R$" + Interest + " em juros.", 1);
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