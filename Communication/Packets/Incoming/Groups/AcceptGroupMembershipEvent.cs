using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class AcceptGroupMembershipEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int UserId = Packet.PopInt();
            Group Group = null;
            GroupRank GroupRank = null;

            if (GroupId < 1000)
            {
                Group = GroupManager.GetJob(GroupId);
                GroupRank = GroupManager.GetJobRank(GroupId, 1);
            }
            else
            {
                Group = GroupManager.GetGang(GroupId);
                GroupRank = GroupManager.GetGangRank(GroupId, 1);
            }

            if (Group == null)
                return;

            if (GroupId < 1000 && !Group.IsMember(Session.GetHabbo().Id) && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                return;

            bool IsAdmin = false;
            if (Group.IsAdmin(Session.GetHabbo().Id))
                IsAdmin = true;
            if (GroupId < 1000 && Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                IsAdmin = true;

            bool IsOwner = false;
            if (Group.CreatorId == Session.GetHabbo().Id)
                IsOwner = true;
            if (GroupId < 1000 && Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
                IsOwner = true;

            if (!IsAdmin && !IsOwner)
                return;

            if (!Group.HasRequest(UserId))
                return;

            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);

            if (Habbo == null)
            {
                Session.SendNotification("Opa, ocorreu um erro ao encontrar este usuário.");
                return;
            }

            if (Group.Id < 1000)
            {
                if (GroupRank.HasCommand("guide"))
                {
                    if (BlackListManager.BlackList.Contains(UserId))
                    {
                        Group.HandleRequest(UserId, false);
                        Session.SendMessage(new GroupInfoComposer(Group, Session));
                        Session.SendMessage(new UnknownGroupComposer(Group.Id, UserId));
                        Session.SendWhisper("Desculpe, mas este usuário está na lista negra e não pode se juntar à corporação policial!", 1);
                        return;
                    }
                }
            }

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Habbo.Id);

            if (Client != null && Client.GetRoleplay() != null)
            {
                if (Group.Id < 1000)
                {
                    if (Client.GetRoleplay().IsWorking)
                    {
                        Client.GetRoleplay().IsWorking = false;
                        WorkManager.RemoveWorkerFromList(Client);
                    }
                    Client.GetRoleplay().TimeWorked = 0;
                    Client.GetRoleplay().JobId = Group.Id;
                    Client.GetRoleplay().JobRank = 1;
                    Client.GetRoleplay().JobRequest = 0;
                }
                else
                {
                    Client.GetRoleplay().GangId = Group.Id;
                    Client.GetRoleplay().GangRank = 1;
                    Client.GetRoleplay().GangRequest = 0;
                }
            }

            Group.HandleRequest(UserId, true);
            Group.SendPackets(Client);

            string Username = Habbo == null ? "someone" : Habbo.Username;

            if (Group.Id < 1000)
                Session.SendWhisper("Sucesso, você aceitou " + Username + " na sua empresa '" + Group.Name + "' no cargo '" + GroupRank.Name + "'!", 1);
            else
                Session.SendWhisper("Sucesso, você aceitou" + Username + " na sua gangue '" + Group.Name + "' no cargo '" + GroupRank.Name + "'!", 1);
            Session.SendMessage(new GroupMemberUpdatedComposer(Group.Id, Habbo, 4));
        }
    }
}