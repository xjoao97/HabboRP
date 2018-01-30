using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.Core;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Checks the users roleplay conditions
    /// </summary>
    public class ConditionCheckTimer : RoleplayTimer
    {
        public ConditionCheckTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            TimeCount = 0;
        }

        /// <summary>
        /// Checks the users roleplay conditions
        /// </summary>
        public override void Execute()
        {
            try
            {
                #region Base Conditions
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetRoleplay() == null)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetHabbo().CurrentRoom == null)
                    return;
                #endregion

                #region Variables
                bool Equipped = base.Client.GetRoleplay().EquippedWeapon == null ? false : true;
                bool Hygiene = base.Client.GetRoleplay().Hygiene == 0 ? true : false;
                bool Healing = base.Client.GetRoleplay().BeingHealed;
                bool Farming = base.Client.GetRoleplay().WateringCan;
                int Effect = base.Client.GetRoomUser().CurrentEffect;
                int Item = base.Client.GetRoomUser().CarryItemID;
                #endregion

                #region Random Checks
                if (base.Client.GetRoleplay().IsWorking || base.Client.GetRoleplay().IsWorkingOut || base.Client.GetRoleplay().BankAccount <= 2 && !base.Client.GetRoomUser().IsAsleep)
                {
                    if (!base.Client.GetRoleplay().CaptchaSent)
                        base.Client.GetRoleplay().CaptchaTime++;
                }

                if (base.Client.GetRoomUser().IsAsleep)
                {
                    if (Equipped)
                    {
                        base.Client.GetRoleplay().EquippedWeapon = null;
                        if (base.Client.GetRoomUser().CurrentEffect != EffectsList.None)
                            base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        if (base.Client.GetRoomUser().CarryItemID != 0)
                            base.Client.GetRoomUser().CarryItem(0);
                    }
                }
                if (!Equipped)
                {
                    if (Effect > 0 && WeaponManager.Weapons.Values.Where(x => x.EffectID == Effect).ToList().Count > 0)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    if (Item > 0 && WeaponManager.Weapons.Values.Where(x => x.HandItem == Item).ToList().Count > 0)
                        base.Client.GetRoomUser().CarryItem(EffectsList.None);
                }
                #endregion

                #region Anti-Enable Checks
                // Repairing Fence & Event Capturing Check
                if (Effect == EffectsList.SunnyD && (!base.Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("repair") || !base.Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("capture")))
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Working Out
                if ((Effect == EffectsList.Treadmill || Effect == EffectsList.CrossTrainer) && !base.Client.GetRoleplay().IsWorkingOut)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Farming Check
                if (Effect == EffectsList.WateringCan && !Farming)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Hygiene Check
                if (Effect == EffectsList.Flies && !Hygiene)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Stun & Frozen Check
                if ((Effect == EffectsList.Dizzy || Effect == EffectsList.Ice) && !base.Client.GetRoomUser().Frozen)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Riding Horse Check
                if (Effect == EffectsList.HorseRiding && !base.Client.GetRoomUser().RidingHorse)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Healing check
                if (Effect == EffectsList.GreenGlow && !Healing)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Staff On Duty Check
                if (Effect == EffectsList.Staff && !base.Client.GetRoleplay().StaffOnDuty)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Ambassador On Duty Check
                if (Effect == EffectsList.Ambassador && !base.Client.GetRoleplay().AmbassadorOnDuty)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Police On Duty Check
                if (Effect == EffectsList.HoloRPPolice && (!base.Client.GetRoleplay().IsWorking || !GroupManager.HasJobCommand(base.Client, "guide")))
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Hosptial On Duty Check
                if (Effect == EffectsList.Medic && (!base.Client.GetRoleplay().IsWorking || !GroupManager.HasJobCommand(base.Client, "heal")))
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Car Driving Check
                if ((Effect == EffectsList.CarDollar || Effect == EffectsList.CarTopFuel || Effect == EffectsList.CarMini || Effect == EffectsList.HoverboardYellow) && !base.Client.GetRoleplay().DrivingCar)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Cuffed Check
                if (Effect == EffectsList.Cuffed && !base.Client.GetRoleplay().Cuffed)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Taxi Check (Regular Users)
                if (Effect == EffectsList.Taxi && !base.Client.GetRoleplay().InsideTaxi)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Taxi Check (Police)
                if (Effect == EffectsList.PoliceTaxi && !base.Client.GetRoleplay().InsideTaxi)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                #endregion

                #region Main Checks
                if (base.Client.GetRoleplay().DrivingCar)
                {
                    base.Client.GetRoleplay().CarTimer++;

                    if (base.Client.GetRoleplay().SexTimer > 0)
                    {
                        base.Client.GetRoleplay().SexTimer = 0;
                        base.Client.GetHabbo().Poof(true);
                    }

                    if (base.Client.GetHabbo().CurrentRoom == null)
                    {
                        base.Client.GetRoleplay().DrivingCar = false;
                        base.Client.GetRoleplay().CarEnableId = 0;
                    }
                    else
                    {
                        if (base.Client.GetRoleplay().CarEnableId == EffectsList.CarPolice)
                        {
                            if (!GroupManager.HasJobCommand(base.Client, "arrest") || !base.Client.GetRoleplay().IsWorking)
                            {
                                base.Client.GetRoleplay().DrivingCar = false;
                                base.Client.GetRoleplay().CarEnableId = EffectsList.None;
                                RoleplayManager.Shout(base.Client, "*Entra em seu carro de polícia*", 4);
                            }
                        }

                        if (!base.Client.GetHabbo().CurrentRoom.DriveEnabled && base.Client.GetRoleplay().DrivingCar)
                        {
                            bool VipCar = base.Client.GetRoleplay().CarEnableId == EffectsList.HoverBoardWhite;

                            base.Client.GetRoleplay().DrivingCar = false;
                            base.Client.GetRoleplay().CarEnableId = EffectsList.None;
                        }
                    }

                    if (base.Client.GetRoleplay().DrivingCar && base.Client.GetRoleplay().CarEnableId != EffectsList.HoverBoardWhite && base.Client.GetRoleplay().CarEnableId != EffectsList.CarPolice)
                    {
                        if (base.Client.GetRoleplay().CarTimer >= 25)
                        {
                            if (base.Client.GetRoleplay().CarType == 1)
                            {
                                base.Client.GetRoleplay().CarFuel -= 3;
                                base.Client.SendWhisper("Você acabou de perder 3 litros de gasolina, agora você tem " + base.Client.GetRoleplay().CarFuel + " galões sobrando!", 1);
                            }
                            else if (base.Client.GetRoleplay().CarType == 2)
                            {
                                base.Client.GetRoleplay().CarFuel -= 2;
                                base.Client.SendWhisper("Você acabou de perder 2 litros de gasolina, agora você tem " + base.Client.GetRoleplay().CarFuel + " galões sobrando!", 1);
                            }
                            else
                            {
                                base.Client.GetRoleplay().CarFuel -= 1;
                                base.Client.SendWhisper("Você acabou de perder 1 litro de gasolina, agora você tem " + base.Client.GetRoleplay().CarFuel + " galões sobrando!", 1);
                            }
                            base.Client.GetRoleplay().CarTimer = 0;
                        }

                        if (base.Client.GetRoleplay().CarFuel <= 0)
                        {
                            base.Client.GetRoleplay().CarFuel = 0;
                            base.Client.GetRoleplay().DrivingCar = false;
                            base.Client.GetRoleplay().CarEnableId = EffectsList.None;

                            string CarName = RoleplayManager.GetCarName(base.Client);
                            RoleplayManager.Shout(base.Client, "*Sente o seu " + CarName + " parar, acho que acabou a gasolina*", 4);
                        }
                    }

                    if (base.Client.GetRoleplay().DrivingCar && (base.Client.GetRoleplay().CarEnableId == EffectsList.HoverBoardWhite || base.Client.GetRoleplay().CarEnableId == EffectsList.CarPolice))
                    {
                        if (base.Client.GetRoleplay().CarTimer >= 360)
                        {
                            base.Client.GetRoleplay().CarTimer = 0;
                            base.Client.GetRoleplay().DrivingCar = false;

                            if (Effect != EffectsList.None)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                            if (base.Client.GetRoleplay().CarEnableId == EffectsList.HoverBoardWhite)
                                RoleplayManager.Shout(base.Client, "*Parece que o seu hoverboard VIP parou, a bateria deve ter morrido*", 4);
                            else
                                RoleplayManager.Shout(base.Client, "*Parece que seu carro de polícia ficou sem gasolina, vá comprar mais*", 4);
                            base.Client.GetRoleplay().CarEnableId = EffectsList.None;
                        }
                    }

                    if (base.Client.GetRoleplay().DrivingCar && Effect != base.Client.GetRoleplay().CarEnableId)
                        base.Client.GetRoomUser().ApplyEffect(base.Client.GetRoleplay().CarEnableId);

                    if (!base.Client.GetRoleplay().DrivingCar)
                    {
                        base.Client.GetRoleplay().CarTimer = 0;

                        if (Effect != EffectsList.None)
                            base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                        if (base.Client.GetRoleplay().CooldownManager.ActiveCooldowns.ContainsKey("car"))
                            base.Client.GetRoleplay().CooldownManager.ActiveCooldowns["car"].Amount = 90;
                        else
                            base.Client.GetRoleplay().CooldownManager.CreateCooldown("car", 1000, 90);
                    }
                }
                else if (base.Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("repair") || base.Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("capture"))
                {
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.SunnyD);
                    return;
                }
                else if (Healing)
                {
                    if (Equipped && Item != base.Client.GetRoleplay().EquippedWeapon.HandItem)
                        base.Client.GetRoomUser().CarryItem(0);
                    if (Effect != EffectsList.GreenGlow)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.GreenGlow);
                    return;
                }
                else if (base.Client.GetRoleplay().IsWorkingOut)
                    return;
                else if (base.Client.GetRoleplay().TextTimer > 0)
                {
                    if (Effect == EffectsList.CellPhone)
                    {
                        if (base.Client.GetRoleplay().TextTimer == 1)
                        {
                            base.Client.GetRoleplay().TextTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetRoleplay().TextTimer--;
                    }
                    else
                        base.Client.GetRoleplay().TextTimer = 0;
                }
                else if (base.Client.GetRoleplay().SexTimer > 0)
                {
                    if (Effect == EffectsList.RunningMan)
                    {
                        if (base.Client.GetRoleplay().SexTimer == 1)
                        {
                            base.Client.GetRoleplay().SexTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                            base.Client.GetHabbo().Poof(true);
                        }
                        else
                            base.Client.GetRoleplay().SexTimer--;
                    }
                    else
                    {
                        base.Client.GetRoleplay().SexTimer = 0;
                        base.Client.GetHabbo().Poof(true);
                    }
                }
                else if (base.Client.GetRoleplay().RapeTimer > 0)
                {
                    if (Effect == EffectsList.Twinkle)
                    {
                        if (base.Client.GetRoleplay().RapeTimer == 1)
                        {
                            base.Client.GetRoleplay().RapeTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetRoleplay().RapeTimer--;
                    }
                    else
                        base.Client.GetRoleplay().RapeTimer = 0;
                }
                else if (base.Client.GetRoleplay().KissTimer > 0)
                {
                    if (Effect == EffectsList.Love)
                    {
                        if (base.Client.GetRoleplay().KissTimer == 1)
                        {
                            base.Client.GetRoleplay().KissTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetRoleplay().KissTimer--;
                    }
                    else
                        base.Client.GetRoleplay().KissTimer = 0;
                }
                else if (base.Client.GetRoleplay().HugTimer > 0)
                {
                    if (Effect == EffectsList.Love)
                    {
                        if (base.Client.GetRoleplay().HugTimer == 1)
                        {
                            base.Client.GetRoleplay().HugTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetRoleplay().HugTimer--;
                    }
                    else
                        base.Client.GetRoleplay().HugTimer = 0;
                }
                else if (Effect == EffectsList.HorseRiding && base.Client.GetRoomUser().RidingHorse)
                    return;
                else if ((Effect == EffectsList.Dizzy || Effect == EffectsList.Ice) && base.Client.GetRoomUser().Frozen)
                    return;
                else if (base.Client.GetRoleplay().InsideTaxi)
                {
                    if (GroupManager.HasJobCommand(base.Client, "guide") && base.Client.GetRoleplay().IsWorking)
                    {
                        if (Effect != EffectsList.PoliceTaxi)
                            base.Client.GetRoomUser().ApplyEffect(EffectsList.PoliceTaxi);
                    }
                    else
                    {
                        if (Effect != EffectsList.Taxi)
                            base.Client.GetRoomUser().ApplyEffect(EffectsList.Taxi);
                    }
                    return;
                }
                else if (base.Client.GetRoleplay().Cuffed)
                {
                    if (Effect != EffectsList.Cuffed)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Cuffed);
                    return;
                }
                else if (Equipped)
                {
                    if (base.Client.GetRoleplay() != null && base.Client.GetRoleplay().EquippedWeapon != null && base.Client.GetRoomUser() != null)
                    {
                        if (base.Client.GetRoleplay().EquippedWeapon.EffectID > 0 && base.Client.GetRoomUser().CurrentEffect != base.Client.GetRoleplay().EquippedWeapon.EffectID)
                            base.Client.GetRoomUser().ApplyEffect(base.Client.GetRoleplay().EquippedWeapon.EffectID);
                    }
                    if (base.Client.GetRoleplay() != null && base.Client.GetRoleplay().EquippedWeapon != null && base.Client.GetRoomUser() != null)
                    {
                        if (base.Client.GetRoleplay().EquippedWeapon.HandItem > 0 && base.Client.GetRoomUser().CarryItemID != base.Client.GetRoleplay().EquippedWeapon.HandItem)
                            base.Client.GetRoomUser().CarryItem(base.Client.GetRoleplay().EquippedWeapon.HandItem);
                    }
                    return;
                }
                else if (Farming)
                {
                    if (Effect != EffectsList.WateringCan)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.WateringCan);
                    return;
                }
                else if (Hygiene)
                {
                    if (Effect != EffectsList.Flies)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Flies);
                    return;
                }
                else if (base.Client.GetRoleplay().StaffOnDuty)
                {
                    if (Effect != EffectsList.Staff)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Staff);
                    return;
                }
                else if (base.Client.GetRoleplay().AmbassadorOnDuty)
                {
                    if (Effect != EffectsList.Ambassador)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Ambassador);
                    return;
                }
                else if (base.Client.GetRoleplay().IsWorking && GroupManager.HasJobCommand(base.Client, "guide"))
                {
                    if (Effect != EffectsList.HoloRPPolice)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.HoloRPPolice);
                    return;
                }
                else if (base.Client.GetRoleplay().IsWorking && GroupManager.HasJobCommand(base.Client, "heal"))
                {
                    if (Effect != EffectsList.Medic)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Medic);
                    return;
                }
                #endregion
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
            }
        }
        
    }
}