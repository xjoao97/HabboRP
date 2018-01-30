using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Polls;
using Plus.HabboHotel.Polls;
using Plus.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users.Effects;

namespace Plus.Communication.Packets.Incoming.Navigator
{
    class GetGuestRoomEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int roomID = Packet.PopInt();
            bool IsVip = Session.GetHabbo().VIPRank < 1 ? false : true;
            int Cost = IsVip ? 0 : 3;
            int Time = IsVip ? (5 + DayNightManager.GetTaxiTime()) : (10 + DayNightManager.GetTaxiTime());
            string TaxiText = IsVip ? " VIP" : "";
            bool RoomLoaded = false;
            bool OnDuty = false;

            if (Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Session.GetRoleplay().StaffOnDuty)
                OnDuty = true;
            if (Session.GetHabbo().VIPRank > 1)
                OnDuty = true;

            #region Conditions
            if (Session.GetRoleplay().IsJailed || Session.GetRoleplay().IsDead)
                return;

            if (Session.GetRoleplay().Cuffed)
            {
                Session.SendWhisper("Os taxistas não querem levar você, pois estão com medo de você algemado!", 1);
                return;
            }

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().Frozen)
                {
                    Session.SendWhisper("Você não pode pegar um taxi enquanto está atordoado!", 1);
                    return;
                }
            }
            #endregion

            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomID);

            if (roomData == null)
                return;

            if (RoomLoaded)
                return;

            if (roomData.TutorialEnabled && !OnDuty)
            {
                Session.SendWhisper("[RPG Taxi] Você não pode ir para uma sala de Tutorial!", 1);
                return;
            }

            if (Session.GetRoomUser() != null && Session.GetRoomUser().GetRoom() != null)
            {
                if (Session.GetRoomUser().GetRoom().TutorialEnabled && !OnDuty)
                {
                    Session.SendWhisper("[RPG Taxi] Você não pode usar taxi em uma sala de tutorial, termine ele primeiro!", 1);
                    return;
                }

                if (!Session.GetRoomUser().GetRoom().TaxiFromEnabled && !OnDuty)
                {
                    Session.SendWhisper("[RPG Taxi] Desculpe, não podemos pegar você neste quarto!", 1);
                    return;
                }
            }

            if (roomID != Session.GetHabbo().CurrentRoomId)
            {
                if (Session.GetRoleplay().Game != null)
                {
                    Session.SendWhisper("Você não pode pegar um texi no evento!", 1);
                    return;
                }

                bool PoliceCost = false;
                if (HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide") && Session.GetRoleplay().IsWorking)
                    PoliceCost = true;

                if (Session.GetHabbo().Credits < Cost && Cost > 0 && !OnDuty && !PoliceCost)
                {
                    Session.SendWhisper("[RPG Taxi] Você não tem dinheiro suficiente para dar uma volta!", 1);
                    return;
                }

                if (Session.GetRoleplay().InsideTaxi)
                {
                    Session.SendWhisper("[RPG Taxi] Já estou indo pegar você! Digite ':ptaxi' se mudar de ideia!", 1);
                    return;
                }

                bool PoliceTool = false;
                if (Session.GetRoleplay().GuideOtherUser != null)
                {
                    if (HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide"))
                    {
                        if (Session.GetRoleplay().GuideOtherUser.GetHabbo() != null && Session.GetRoleplay().GuideOtherUser.GetRoomUser() != null)
                        {
                            if (roomID == Session.GetRoleplay().GuideOtherUser.GetRoomUser().RoomId)
                                PoliceTool = true;
                        }
                    }
                }

                if (!roomData.TaxiToEnabled && !OnDuty && !PoliceTool)
                {
                    Session.SendWhisper("[RPG Taxi] Desculpe, não podemos buscar você neste quarto!", 1);
                    return;
                }

                bool Event = false;
                if (roomData.RoleplayEvent != null)
                {
                    if (!roomData.RoleplayEvent.HasGameStarted())
                        Event = true;
                }

                Session.GetRoleplay().InsideTaxi = true;
                bool PoliceTaxi = false;

                if (!OnDuty && !PoliceTool && Session.GetHabbo().CurrentRoomId > 0 && !Event)
                {
                    if (HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide") && Session.GetRoleplay().IsWorking)
                    {
                        Cost = 0;
                        Time = 5;

                        if (Session.GetRoomUser() != null)
                            Session.GetRoomUser().ApplyEffect(EffectsList.PoliceTaxi);

                        Session.Shout("*Puxa o rádio da Polícia e vai rapidamente para " + roomData.Name + " [ID: " + roomID + "]*", 37);
                        PoliceTaxi = true;
                    }
                    else
                    {
                        if (Session.GetRoomUser() != null)
                            Session.GetRoomUser().ApplyEffect(EffectsList.Taxi);

                        Session.Shout("*Chama um Taxi" + TaxiText + " por " + roomData.Name + " [ID: " + roomID + "]*", 4);
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
                                    Session.Shout("*Entra no meu carro de polícia e vai para o local*", 37);
                                }
                                else
                                    Session.Shout("*Pula dentro do meu Taxi" + TaxiText + " e vai rapidamente para o local*", 4);
                                RoleplayManager.SendUser(Session, roomData.Id);
                            }
                        }
                    }).Start();
                }
                else
                {
                    if (PoliceTool)
                    {
                        if (Session.GetRoomUser() != null)
                            Session.GetRoomUser().ApplyEffect(EffectsList.CarPolice);
                        Session.Shout("*Entra no meu carro de polícia e dirige para ajudar um cidadão em necessidade*", 4);
                    }
                    else if (OnDuty)
                        Session.Shout("*Pula dentro do meu Carro Staff*", 23);
                    RoleplayManager.SendUser(Session, roomData.Id);
                }
            }
            else
                Session.SendMessage(new GetGuestRoomResultComposer(Session, roomData, true, false));
        }
    }
}
