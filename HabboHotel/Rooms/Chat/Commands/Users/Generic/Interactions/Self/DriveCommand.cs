using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class DriveCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_drive"; }
        }

        public string Parameters
        {
            get { return "%type%"; }
        }

        public string Description
        {
            get { return "Permite-lhe conduzir o seu HoverBoard VIP ou o carro comprado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                if (Session.GetRoleplay().DrivingCar)
                    StopCar(Session);
                else
                    Session.SendWhisper("Por favor, use o comando ':dirigir carro/vip/policia' para dirigir seu automóvel!", 1);
                return;
            }

            if (Session.GetRoleplay().DrivingCar)
            {
                StopCar(Session);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("carro"))
                return;

            if (Session.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("territorio") || Session.GetRoleplay().CapturingTurf != null)
            {
                Session.SendWhisper("Você não pode dirigir seu carro em um Território!", 1);
                return;
            }

            if (!Room.DriveEnabled)
            {
                Session.SendWhisper("Você não pode dirigir seu carro neste quarto!", 1);
                return;
            }

            switch (Params[1].ToLower())
            {
                case "police":
				case "policia":
				case "policial":
                    {
                        if (!GroupManager.HasJobCommand(Session, "arrest"))
                        {
                            Session.SendWhisper("Apenas um policial pode dirigir um carro de polícia!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para dirigir um carro da polícia!", 1);
                            return;
                        }

                        Session.GetRoleplay().DrivingCar = true;
                        Session.GetRoleplay().CarEnableId = 19;

                        if (Session.GetRoomUser() != null)
                        {
                            if (Session.GetRoomUser().CurrentEffect != Session.GetRoleplay().CarEnableId)
                                Session.GetRoomUser().ApplyEffect(Session.GetRoleplay().CarEnableId);
                        }

                        if (Session.GetRoleplay().EquippedWeapon != null)
                            Session.GetRoleplay().EquippedWeapon = null;

                        Session.GetRoleplay().CarTimer = 0;

                        if (Session.GetRoleplay().SexTimer > 0)
                        {
                            Session.GetRoleplay().SexTimer = 0;
                            Session.GetHabbo().Poof(true);
                        }

                        Session.Shout("*Coloca a chave na ignição e liga seu carro de polícia*", 4);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("carro", 1000, 30);
                        break;
                    }
                case "vip":
				case "hoverboard":
                    {
                        if (Session.GetHabbo().VIPRank < 1)
                        {
                            Session.SendWhisper("Você não é VIP", 1);
                            break;
                        }

                        Session.GetRoleplay().DrivingCar = true;
                        Session.GetRoleplay().CarEnableId = 504;

                        if (Session.GetRoomUser() != null)
                        {
                            if (Session.GetRoomUser().CurrentEffect != Session.GetRoleplay().CarEnableId)
                                Session.GetRoomUser().ApplyEffect(Session.GetRoleplay().CarEnableId);
                        }

                        if (Session.GetRoleplay().EquippedWeapon != null)
                            Session.GetRoleplay().EquippedWeapon = null;

                        Session.GetRoleplay().CarTimer = 0;

                        if (Session.GetRoleplay().SexTimer > 0)
                        {
                            Session.GetRoleplay().SexTimer = 0;
                            Session.GetHabbo().Poof(true);
                        }

                        Session.Shout("*Pega o seu Hoverboard [VIP] e começa a andar*", 4);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("carro", 1000, 90);
                        break;
                    }
                case "car":
				case "carro":
                    {
                        if (Session.GetRoleplay().CarType <= 0)
                        {
                            Session.SendWhisper("Você não tem um carro!", 1);
                            break;
                        }

                        if (Session.GetRoleplay().CarFuel < 0)
                        {
                            Session.SendWhisper("Você não tem mais combustível para seu carro!", 1);
                            break;
                        }

                        string CarType = HabboRoleplay.Misc.RoleplayManager.GetCarName(Session);

                        if (Session.GetRoleplay().CarType == 1)
                            Session.GetRoleplay().CarEnableId = 801;
                        else if (Session.GetRoleplay().CarType == 2)
                            Session.GetRoleplay().CarEnableId = 22;
                        else
                            Session.GetRoleplay().CarEnableId = 69;

                        if (Session.GetRoomUser() != null)
                        {
                            if (Session.GetRoomUser().CurrentEffect != Session.GetRoleplay().CarEnableId)
                                Session.GetRoomUser().ApplyEffect(Session.GetRoleplay().CarEnableId);
                        }

                        if (Session.GetRoleplay().EquippedWeapon != null)
                            Session.GetRoleplay().EquippedWeapon = null;

                        Session.GetRoleplay().DrivingCar = true;
                        Session.GetRoleplay().CarTimer = 0;

                        if (Session.GetRoleplay().SexTimer > 0)
                        {
                            Session.GetRoleplay().SexTimer = 0;
                            Session.GetHabbo().Poof(true);
                        }

                        Session.Shout("*Pega o seu " + CarType + " e começa e dirigir*", 4);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("carro", 1000, 30);
                        break;
                    }
                default:
                    {
                        Session.SendWhisper("Por favor, use o comando ':dirigir carro/vip' para dirigir seu automóvel!", 1);
                        break;
                    }
            }
        }

        public void StopCar(GameClients.GameClient Session)
        {
            Session.GetRoleplay().DrivingCar = false;
            Session.GetRoleplay().CarEnableId = 0;
            Session.Shout("*Sai do seu veículo e o guarda*", 4);

            if (Session.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("conditioncheck"))
                Session.GetRoleplay().TimerManager.ActiveTimers["conditioncheck"].TimeCount = 0;

            if (Session.GetRoleplay().CooldownManager.ActiveCooldowns.ContainsKey("carro"))
                Session.GetRoleplay().CooldownManager.ActiveCooldowns["carro"].Amount = 90;
            else
                Session.GetRoleplay().CooldownManager.CreateCooldown("carro", 1000, 90);

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().CurrentEffect != 0)
                    Session.GetRoomUser().ApplyEffect(0);
            }
            return;
        }
    }
}
