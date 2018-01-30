using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;

using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.Users.Effects;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    class FollowFriendEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            if (!RoleplayManager.FollowFriends)
            {
                Session.SendWhisper("Opa, o gerenciamento do servidor desativou a capacidade de seguir seus amigos.", 1);
                return;
            }

            int BuddyId = Packet.PopInt();
            bool IsVip = Session.GetHabbo().VIPRank < 1 ? false : true;
            int Cost = IsVip ? 0 : 3;
            int Time = IsVip ? (5 + DayNightManager.GetTaxiTime()) : (10 + DayNightManager.GetTaxiTime());
            string TaxiText = IsVip ? " VIP" : "";
            bool OnDuty = false;

            if (BuddyId == 0 || BuddyId == Session.GetHabbo().Id)
                return;

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(BuddyId);
            if (Client == null || Client.GetHabbo() == null)
                return;

            if (!Client.GetHabbo().InRoom)
            {
                Session.SendMessage(new FollowFriendFailedComposer(2));
                Session.GetHabbo().GetMessenger().UpdateFriend(Client.GetHabbo().Id, Client, true);
                return;
            }
            else if (Session.GetHabbo().CurrentRoom != null && Client.GetHabbo().CurrentRoom != null)
            {
                if (Session.GetHabbo().CurrentRoom.RoomId == Client.GetHabbo().CurrentRoom.RoomId)
                    return;
            }

            if (!Client.GetHabbo().AllowConsoleMessages)
            {
                if (Session.GetHabbo().InRoom)
                    Session.SendWhisper("Desculpe, mas esse cidadão desligou o telefone, então você não pode segui-lo.", 1);
                else
                    Session.SendNotification("Desculpe, mas esse cidadão desligou o telefone, então você não pode segui-lo.");
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode seguir seu amigo enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode seguir seu amigo enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().Frozen)
                {
                    Session.SendWhisper("Você não pode seguir seu amigo enquanto está congelado ou atordoado!", 1);
                    return;
                }
            }

            if (Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Session.GetRoleplay().StaffOnDuty)
                OnDuty = true;
            if (Session.GetHabbo().VIPRank > 1)
                OnDuty = true;

            if (Session.GetHabbo().CurrentRoom != null)
            {
                if (!Session.GetHabbo().CurrentRoom.TaxiFromEnabled && !OnDuty)
                {
                    Session.SendWhisper("[RPG Taxi] Desculpe, não podemos levá-lo para fora deste quarto!", 1);
                    return;
                }
            }

            if (Session.GetRoleplay().Cuffed)
            {
                Session.SendWhisper("Os motoristas de taxi estão muito assustados para levá-lo com as algemas presas em você!", 1);
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
                Session.SendWhisper("[RPG Taxi] Já estou indo te buscar! Digite ':ptaxi' se mudar de ideia!", 1);
                return;
            }

            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Client.GetHabbo().CurrentRoomId);

            if (roomData == null)
            {
                Session.SendWhisper("[RPG Taxi] Desculpe, não conseguimos encontrar esse quarto!", 1);
                return;
            }

            if (!roomData.TaxiToEnabled && !OnDuty)
            {
                Session.SendWhisper("[RPG Taxi] Desculpe, não podemos táxi você para este quarto!", 1);
                return;
            }

            if (roomData.TutorialEnabled && !OnDuty)
            {
                Session.SendWhisper("Você não pode ir para uma sala de tutorial, desculpe!", 1);
                return;
            }

            if (Session.GetHabbo().CurrentRoom != null)
            {
                if (Session.GetHabbo().CurrentRoom.TutorialEnabled && !OnDuty)
                {
                    Session.SendWhisper("Você não pode sair de uma sala de tutorial! Somente após concluir!", 1);
                    return;
                }
            }

            if (Session.GetRoleplay().Game != null)
            {
                Session.SendWhisper("Você não pode pegar um taxi enquanto está dentro de um evento!", 1);
                return;
            }

            if (Session.GetRoleplay().TexasHoldEmPlayer > 0)
            {
                Session.SendWhisper("Você não pode pegar um taxi no meio de um jogo de Texas Hold!", 1);
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

                    Session.Shout("*Puxa o rádio da Polícia e vai rapidamente para " + roomData.Name + " [ID: " + roomData.Id + "]*", 37);
                    PoliceTaxi = true;
                }
                else
                {
                    if (Session.GetRoomUser() != null)
                        Session.GetRoomUser().ApplyEffect(596);

                    Session.Shout("*Chama um Taxi" + TaxiText + " para " + roomData.Name + " [ID: " + roomData.Id + "]*", 4);
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
                                Session.Shout("*Pula dentro do carro da polícia do meu parceiro e vai para o local*", 37);
                            }
                            else
                                Session.Shout("*Pula no meu Taxi" + TaxiText + " e vai para o local*", 4);
                            RoleplayManager.SendUser(Session, roomData.Id);
                        }
                    }
                }).Start();
            }
            else
            {
                Session.Shout("*Segue imediatamente " + Client.GetHabbo().Username + "*", 23);
                RoleplayManager.SendUser(Session, Client.GetHabbo().CurrentRoomId);
                PlusEnvironment.GetGame().GetChatManager().GetCommands().LogCommand(Session.GetHabbo().Id, "follow " + Client.GetHabbo().Username, Session.GetHabbo().MachineId, "staff");
            }
        }
    }
}
