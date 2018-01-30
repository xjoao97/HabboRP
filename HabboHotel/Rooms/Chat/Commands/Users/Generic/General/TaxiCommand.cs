using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class TaxiCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_taxi"; }
        }

        public string Parameters
        {
            get { return "%quarto_id%"; }
        }

        public string Description
        {
            get { return "Solicita um taxi para enviá-lo para o ID do quarto escolhido."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            UInt32 RoomId = 0;
            bool IsVip = Session.GetHabbo().VIPRank < 1 ? false : true;
            int Cost = IsVip ? 0 : 3;
            int Time = IsVip ? (5 + DayNightManager.GetTaxiTime()) : (10 + DayNightManager.GetTaxiTime());
            string TaxiText = IsVip ? "[Uber]" : "";
            bool OnDuty = false;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um ID de Quarto!", 1);
                return;
            }

            if (!UInt32.TryParse(Params[1].ToString(), out RoomId))
            {
                Session.SendWhisper("Por favor insira um número válido.", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode táxi enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode táxi enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().Frozen)
                {
                    Session.SendWhisper("Você não pode táxi enquanto está congelado ou atordoado!", 1);
                    return;
                }
            }

            if (Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Session.GetRoleplay().StaffOnDuty)
                OnDuty = true;
            if (Session.GetHabbo().VIPRank > 1)
                OnDuty = true;

            if (!Room.TaxiFromEnabled && !OnDuty)
            {
                Session.SendWhisper("[RPG TAXI]Desculpe, não podemos levá-lo fora deste quarto!", 1);
                return;
            }

            if (Session.GetRoleplay().Cuffed)
            {
                Session.SendWhisper("Os motoristas de táxi estão muito assustados para levá-lo com essas algemas presas a você!", 1);
                return;
            }

            if (RoomId == Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper("Você já está nesta sala!", 1);
                return;
            }

            bool PoliceCost = false;
            if (HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide") && Session.GetRoleplay().IsWorking)
                PoliceCost = true;

            if (Session.GetHabbo().Credits < Cost && Cost > 0 && !OnDuty && !PoliceCost)
            {
                Session.SendWhisper("[RPG TAXI] Você não tem dinheiro suficiente para dar uma volta!", 1);
                return;
            }

            if (Session.GetRoleplay().InsideTaxi)
            {
                Session.SendWhisper("[RPG TAXI] Já estou indo te pegar! Digite ':ptaxi' se você mudar de ideia!", 1);
                return;
            }

            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Convert.ToInt32(RoomId));

            if (roomData == null)
            {
                Session.SendWhisper("[RPG TAXI] Desculpe, não conseguimos encontrar esse quarto!", 1);
                return;
            }

            if (!roomData.TaxiToEnabled && !OnDuty)
            {
                Session.SendWhisper("[RPG TAXI] Desculpe, não podemos levá-lo para este quarto!", 1);
                return;
            }

            if (roomData.TutorialEnabled && !OnDuty)
            {
                Session.SendWhisper("Você não pode pegar taxi em uma sala de tutorial, desculpe!", 1);
                return;
            }

            if (Session.GetHabbo().CurrentRoom != null)
            {
                if (Session.GetHabbo().CurrentRoom.TutorialEnabled && !OnDuty)
                {
                    Session.SendWhisper("Você não pode pegar um taxi na sala de Tutorial! Conclua o tutorial para sair.", 1);
                    return;
                }
            }

            if (Session.GetRoleplay().Game != null)
            {
                Session.SendWhisper("Você não pode taxi enquanto está dentro de um evento!", 1);
                return;
            }
            
            if (Session.GetRoleplay().TexasHoldEmPlayer > 0)
            {
                Session.SendWhisper("Você não pode táxi enquanto está dentro de um evento", 1);
                return;
            }

            Session.GetRoleplay().InsideTaxi = true;
            bool PoliceTaxi = false;

            if (!OnDuty)
            {
                if (HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide") && Session.GetRoleplay().IsWorking)
                {
                    Cost = 0;
                    Time = 5;

                    if (Session.GetRoomUser() != null)
                        Session.GetRoomUser().ApplyEffect(597);

                    Session.Shout("*Entra no seu carro de Polícia e vai rapidamente para o " + roomData.Name + " [ID: " + RoomId + "]*", 37);
                    PoliceTaxi = true;
                }
                else
                {
                    if (Session.GetRoomUser() != null)
                        Session.GetRoomUser().ApplyEffect(596);

                    Session.Shout("*Pega um Taxi " + TaxiText + " para " + roomData.Name + " [ID: " + RoomId + "]*", 4);
                }

                new Thread(() =>
                {
                    for (int i = 0; i < (Time + 1) * 10; i++)
                    {
                        if (Session.GetRoleplay() == null)
                            break;

                        if (Session.GetRoleplay().InsideTaxi)
                            Thread.Sleep(100);
                        else
                            break;
                    }
                    if (Session.GetRoleplay() != null)
                    {
                        if (Session.GetRoleplay().InsideTaxi)
                        {
                            if (Cost > 0)
                            {
                                Session.GetHabbo().Credits -= Cost;
                                Session.GetHabbo().UpdateCreditsBalance();
                            }

                            if (PoliceTaxi)
                            {
                                if (Session.GetRoomUser() != null)
                                    Session.GetRoomUser().ApplyEffect(EffectsList.CarPolice);
                                Session.Shout("*Pula dentro do carro da polícia de seu parceiro e vai para o local*", 37);
                            }
                            else
                                Session.Shout("*Pula dentro do seu " + TaxiText + " Taxi e vai para o local*", 4);
                            RoleplayManager.SendUser(Session, roomData.Id);
                        }
                    }
                }).Start();
            }
            else
            {
                Session.Shout("*Pula no seu Carro Staff e vai para " + roomData.Name + " [ID: " + RoomId + "]*", 23);
                RoleplayManager.SendUser(Session, roomData.Id);
            }
        }
    }
}