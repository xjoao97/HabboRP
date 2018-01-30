using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Guides;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Communication.Packets.Outgoing.Guides;
using Plus.HabboHotel.Quests;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Work-Timer
    /// </summary>
    public class WorkTimer : RoleplayTimer
    {
        public WorkTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params)
            : base(Type, Client, Time, Forever, Params)
        {
            TimeCount = 0;
            TimeCount2 = 0;

            // Convert to milliseconds
            double TimeDivisible = Math.Floor((double)base.Client.GetRoleplay().TimeWorked / 15);
            int TimeRemaining = base.Client.GetRoleplay().TimeWorked - Convert.ToInt32(TimeDivisible) * 15;
            TimeLeft = (15 - TimeRemaining) * 60000;
            OriginalTime = 15;

            Client.GetRoleplay().UpdateTimerDialogue("Work-Timer", "add", (TimeLeft / 60000), OriginalTime);            

            base.Client.SendWhisper("Você irá receber seu pagamento em " + (TimeLeft / 60000) + " minuto(s)!", 1);
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

                GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
                GroupRank JobRank = GroupManager.GetJobRank(base.Client.GetRoleplay().JobId, base.Client.GetRoleplay().JobRank);

                if (!base.Client.GetRoleplay().IsWorking || base.Client.GetRoleplay().JobId == 1)
                {
                    Client.GetRoleplay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);            

                    WorkManager.RemoveWorkerFromList(base.Client);
                    base.Client.GetRoleplay().IsWorking = false;
                    base.Client.GetHabbo().Poof();

                    if (GroupManager.HasJobCommand(base.Client, "guide"))
                    {
                        guideManager.RemoveGuide(base.Client);
                        base.Client.SendMessage(new HelperToolConfigurationComposer(base.Client));

                        #region End Existing Calls
                        if (base.Client.GetRoleplay().GuideOtherUser != null)
                        {
                            base.Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                            base.Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                            if (base.Client.GetRoleplay().GuideOtherUser.GetRoleplay() != null)
                            {
                                base.Client.GetRoleplay().GuideOtherUser.GetRoleplay().Sent911Call = false;
                                base.Client.GetRoleplay().GuideOtherUser.GetRoleplay().GuideOtherUser = null;
                            }

                            base.Client.GetRoleplay().GuideOtherUser = null;
                            base.Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                            base.Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                        }
                        #endregion
                    }
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoleplay().CurEnergy <= 0)
                {
                    Client.GetRoleplay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);            

                    RoleplayManager.Shout(base.Client, "*Para de trabalhar por ficar sem energia*", 4);

                    WorkManager.RemoveWorkerFromList(base.Client);
                    base.Client.GetRoleplay().IsWorking = false;
                    base.Client.GetHabbo().Poof();

                    if (GroupManager.HasJobCommand(base.Client, "guide"))
                    {
                        guideManager.RemoveGuide(base.Client);
                        base.Client.SendMessage(new HelperToolConfigurationComposer(base.Client));

                        #region End Existing Calls
                        if (base.Client.GetRoleplay().GuideOtherUser != null)
                        {
                            base.Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                            base.Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                            if (base.Client.GetRoleplay().GuideOtherUser.GetRoleplay() != null)
                            {
                                base.Client.GetRoleplay().GuideOtherUser.GetRoleplay().Sent911Call = false;
                                base.Client.GetRoleplay().GuideOtherUser.GetRoleplay().GuideOtherUser = null;
                            }

                            base.Client.GetRoleplay().GuideOtherUser = null;
                            base.Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                            base.Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                        }
                        #endregion
                    }

                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() != null)
                {
                    if (base.Client.GetRoomUser().IsAsleep)
                    {
                        Client.GetRoleplay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);            
                        RoleplayManager.Shout(base.Client, "*Para de trabalhar por ficar ausente*", 4);

                        WorkManager.RemoveWorkerFromList(base.Client);
                        base.Client.GetRoleplay().IsWorking = false;
                        base.Client.GetHabbo().Poof();

                        if (GroupManager.HasJobCommand(base.Client, "guide"))
                        {
                            guideManager.RemoveGuide(base.Client);
                            base.Client.SendMessage(new HelperToolConfigurationComposer(base.Client));

                            #region End Existing Calls
                            if (base.Client.GetRoleplay().GuideOtherUser != null)
                            {
                                base.Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                                base.Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                                if (base.Client.GetRoleplay().GuideOtherUser.GetRoleplay() != null)
                                {
                                    base.Client.GetRoleplay().GuideOtherUser.GetRoleplay().Sent911Call = false;
                                    base.Client.GetRoleplay().GuideOtherUser.GetRoleplay().GuideOtherUser = null;
                                }

                                base.Client.GetRoleplay().GuideOtherUser = null;
                                base.Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                                base.Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                            }
                            #endregion
                        }

                        base.EndTimer();
                        return;
                    }
                    
                    if (base.Client.GetHabbo().CurrentRoom != null)
                    {
                        if (base.Client.GetHabbo().CurrentRoom.TurfEnabled)
                        {
                            if (GroupManager.HasJobCommand(base.Client, "guide"))
                            {
                                Client.GetRoleplay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);

                                WorkManager.RemoveWorkerFromList(base.Client);
                                base.Client.GetRoleplay().IsWorking = false;
                                base.Client.GetHabbo().Poof();

                                guideManager.RemoveGuide(base.Client);
                                base.Client.SendMessage(new HelperToolConfigurationComposer(base.Client));

                                #region End Existing Calls
                                if (base.Client.GetRoleplay().GuideOtherUser != null)
                                {
                                    base.Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                                    base.Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                                    if (base.Client.GetRoleplay().GuideOtherUser.GetRoleplay() != null)
                                    {
                                        base.Client.GetRoleplay().GuideOtherUser.GetRoleplay().Sent911Call = false;
                                        base.Client.GetRoleplay().GuideOtherUser.GetRoleplay().GuideOtherUser = null;
                                    }

                                    base.Client.GetRoleplay().GuideOtherUser = null;
                                    base.Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                                    base.Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                                }
                                #endregion

                                base.EndTimer();
                                return;
                            }
                        }
                    }
                }

                if (base.Client.GetRoomUser() != null)
                    base.Client.GetRoomUser().IdleTime += 3;

                if (RoleplayManager.JobCAPTCHABox)
                {
                    if (base.Client.GetRoleplay().CaptchaSent)
                        return;

                    if (!base.Client.GetRoleplay().CaptchaSent && base.Client.GetRoleplay().CaptchaTime >= Convert.ToInt32(RoleplayData.GetData("captcha", "jobinterval")))
                    {
                        base.Client.GetRoleplay().CreateCaptcha("Digite o código na caixa para continuar cobrando cheques de pagamento!");
                        return;
                    }
                }

                TimeCount++;
                TimeCount2++;
                TimeLeft -= 1000;

                if (TimeCount == 30 || TimeCount == 60)
                {
                    var Timers = base.Client.GetRoleplay().TimerManager;

                    if (Timers != null)
                    {
                        if (Timers.ActiveTimers != null)
                        {
                            if (Timers.ActiveTimers.ContainsKey("hunger"))
                            {
                                int hungercount = Random.Next(20, 46);
                                Timers.ActiveTimers["hunger"].TimeCount += hungercount;
                            }
                            if (Timers.ActiveTimers.ContainsKey("hygiene"))
                            {
                                int hygienecount = Random.Next(20, 46);
                                Timers.ActiveTimers["hygiene"].TimeCount += hygienecount;
                            }
                        }
                    }
                }

                if (TimeCount2 == 60)
                {
                    base.Client.GetRoleplay().TimeWorked++;
                    TimeCount2 = 0;
                }

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        int EnergyLoss = Random.Next(2, 6);

                        if (base.Client.GetRoleplay().CurEnergy - EnergyLoss <= 0)
                            base.Client.GetRoleplay().CurEnergy = 0;
                        else
                            base.Client.GetRoleplay().CurEnergy -= EnergyLoss;
                        Client.GetRoleplay().UpdateTimerDialogue("Work-Timer", "decrement", (TimeLeft / 60000), OriginalTime);            

                        base.Client.SendWhisper("Você irá receber seu pagamento em " + (TimeLeft / 60000) + " minuto(s)! E você perdeu " + EnergyLoss + " energia trabalhando!", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                if (JobRank == null)
                    return;

                int Pay = JobRank.Pay;

                if (base.Client.GetRoleplay().Class.ToLower() == "civilian")
                {
                    Random Random = new Random();
                    int ExtraPay = Random.Next(1, 6);

                    Pay += ExtraPay;
                }

                RoleplayManager.Shout(base.Client, "*Recebe meu pagamento*", 4);
                base.Client.SendWhisper("Você recebeu R$" + Pay + "." + (base.Client.GetRoleplay().BankAccount > 0 ? " O salário foi depositado automaticamente em sua conta bancária!" : ""), 1);

                if (base.Client.GetRoleplay().BankAccount > 0)
                    base.Client.GetRoleplay().BankChequings += Pay;
                else
                {
                    base.Client.GetHabbo().Credits += Pay;
                    base.Client.GetHabbo().UpdateCreditsBalance();
                }

                LevelManager.AddLevelEXP(base.Client, GetExp());
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.WORK_CYCLE);

                #region Timer Restart Calculation

                double TimeDivisible2 = Math.Floor(Convert.ToDouble(base.Client.GetRoleplay().TimeWorked) / 15);
                int TimeRemaining2 = base.Client.GetRoleplay().TimeWorked - Convert.ToInt32(TimeDivisible2) * 15;
                TimeLeft = (15 - TimeRemaining2) * 60000;
                TimeCount = 0;
                TimeCount2 = 0;

                #endregion            

                base.Client.SendWhisper("Começa a trabalhar no novo turno! Você irá receber seu salário em " + (TimeLeft / 60000) + " minutos!", 1);
                return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }

        /// <summary>
        /// Calculates the exp earned from finishing a shift
        /// </summary>
        public int GetExp()
        {
            int Multiplier = 1;

            int Chance = Random.Next(1, 101);

            if (Chance <= 42)
            {
                if (Chance <= 8)
                    Multiplier = 4;
                else if (Chance <= 16)
                    Multiplier = 3;
                else if (Chance <= 32)
                    Multiplier = 2;
                else
                    Multiplier = 1;
            }

            int Amount = Random.Next(15, 51) * Multiplier;

            return Amount;
        }
    }
}