using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class GetHelperToolConfigurationMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            RoomUser User = Session.GetRoomUser();
            Group Group = GroupManager.GetJob(Session.GetRoleplay().JobId);

            #region Conditions
            if (User == null)
                return;

            if (Group == null)
            {
                Session.SendNotification("Somente um policial pode usar esta ferramenta!");
                return;
            }

            if (Group.Id <= 0)
            {
                Session.SendNotification("Somente um policial pode usar esta ferramenta!!");
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "guide"))
            {
                Session.SendNotification("Somente um policial pode usar esta ferramenta!!");
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                Session.SendWhisper("Você não pode trabalhar enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                Session.SendWhisper("Você não pode trabalhar enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoleplay().IsWorkingOut)
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                Session.SendWhisper("Você não pode trabalhar enquanto está malhando!", 1);
                return;
            }

            if (Session.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("sendhome"))
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                Session.SendWhisper("Você não pode trabalhar enquanto foi enviado para casa!", 1);
                return;
            }

            if (Session.GetRoleplay().Game != null)
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                Session.SendWhisper("Você não pode trabalhar enquanto estiver dentro de um evento!", 1);
                return;
            }

            if (GroupManager.HasJobCommand(Session, "guide") && RoleplayManager.PurgeStarted)
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                Session.SendWhisper("Você não pode começar a trabalhar como policial enquanto uma purga foi ativada!", 1);
                return;
            }

            if (GroupManager.HasJobCommand(Session, "guide") && Session.GetHabbo().CurrentRoom.RoomData.TurfEnabled && !RoleplayManager.StartWorkInPoliceHQ)
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                Session.SendWhisper("Você não pode trabalhar como policial enquanto está dentro de um território!", 1);
                return;
            }

            if (BlackListManager.BlackList.Contains(Session.GetHabbo().Id))
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                Session.SendWhisper("Desculpe, mas você está na lista negra da corporação policial!", 1);
                return;
            }
            #endregion

            GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
            bool onDuty = Packet.PopBoolean();

            Session.GetRoleplay().HandlingCalls = Packet.PopBoolean();
            Session.GetRoleplay().HandlingJailbreaks = Packet.PopBoolean();
            Session.GetRoleplay().HandlingHeists = Packet.PopBoolean();

            if (onDuty)
            {
                if (Session.GetRoleplay().TryGetCooldown("startwork", true))
                {
                    Session.SendMessage(new HelperToolConfigurationComposer(Session));
                    return;
                }

                if (Session.GetRoleplay().CurEnergy <= 0)
                {
                    Session.SendMessage(new HelperToolConfigurationComposer(Session));
                    Session.SendWhisper("Você não tem energia para trabalhar!", 1);
                    return;
                }

                if (!Session.GetRoleplay().IsWorking)
                {
                    Session.GetRoleplay().IsWorking = true;
                    guideManager.AddGuide(Session);
                    Session.Shout("*Começa a trabalhar como " + GroupManager.GetJob(Session.GetRoleplay().JobId).Name + " no cargo " + GroupManager.GetJobRank(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank).Name + "*", 4);
                    RoleplayManager.GetLookAndMotto(Session);
                    WorkManager.AddWorkerToList(Session);
                    Session.GetRoleplay().TimerManager.CreateTimer("work", 1000, true);
                    Session.GetRoleplay().CooldownManager.CreateCooldown("startwork", 1000, 10);

                    if (Session.GetHabbo().CurrentRoomId != Convert.ToInt32(RoleplayData.GetData("police", "headquartersroomid")))
                    {
                        if (RoleplayManager.StartWorkInPoliceHQ)
                            RoleplayManager.SendUser(Session, Convert.ToInt32(RoleplayData.GetData("police", "headquartersroomid")));
                    }
                }
            }
            else
            {
                if (Session.GetRoleplay().TryGetCooldown("stopwork", true))
                {
                    Session.SendMessage(new HelperToolConfigurationComposer(Session));
                    return;
                }
                if (Session.GetRoleplay().IsWorking)
                {
                    Session.GetRoleplay().IsWorking = false;
                    guideManager.RemoveGuide(Session);
                    Session.Shout("*Para de trabalhar como " + GroupManager.GetJob(Session.GetRoleplay().JobId).Name + " [" + GroupManager.GetJobRank(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank).Name + "]*", 4);
                    WorkManager.RemoveWorkerFromList(Session);
                    Session.GetHabbo().Poof();
                    Session.GetRoleplay().CooldownManager.CreateCooldown("stopwork", 1000, 10);
                }
            }
            Session.SendMessage(new HelperToolConfigurationComposer(Session));
        }
    }
}
