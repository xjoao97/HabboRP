using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.Database.Interfaces;

using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Users;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class JoinGroupEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            int GroupId = Packet.PopInt();
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

            if (GroupId >= 1000)
            {
                Group CurrentGang = GroupManager.GetGang(Session.GetRoleplay().GangId);

                if (CurrentGang != null && CurrentGang.CreatorId == Session.GetHabbo().Id)
                {
                    Session.SendNotification("Você deve excluir sua primeira gangue se quiser se juntar a outra gangue!");
                    return;
                }
            }

            if (Group == null)
                return;

            if (Group.IsMember(Session.GetHabbo().Id) || Group.IsAdmin(Session.GetHabbo().Id))
                return;

            if (Group.GroupType == GroupType.LOCKED && Group.HasRequest(Session.GetHabbo().Id))
                return;

            if (Group.GroupType == GroupType.PRIVATE)
                return;

            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id);

            if (Groups.Count >= 3)
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("Opa, parece que você atingiu o limite do grupo! Você só pode juntar até 1.500 grupos."));
                return;
            }

            if (Group.Id < 1000 && GroupRank.HasCommand("guide"))
            {
                if (BlackListManager.BlackList.Contains(Session.GetHabbo().Id))
                {
                    Session.SendWhisper("Você está na lista negra e não pode se juntar à corporação da polícia!", 1);
                    return;
                }
            }

            if (GroupId < 1000)
            {
                if (Group.GroupType == GroupType.LOCKED && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                {
                    Session.GetRoleplay().JobRequest = GroupId;
                    Group.Requests.Add(Session.GetHabbo().Id);

                    Session.SendWhisper("Sucesso, você solicitou com sucesso para se juntar à empresa '" + Group.Name + "'!", 1);
                    List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && (Group.IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("corporation_rights")) select Client).ToList();
                    foreach (GameClient Client in GroupAdmins)
                    {
                        Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, Session.GetHabbo(), 3));
                    }
                    Session.SendMessage(new GroupInfoComposer(Group, Session));
                    return;
                }
                else
                {
                    int Bubble = 4;
                    if (Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                        Bubble = 23;

                    Session.Shout("*Se contrata como um " + Group.Name + " no cargo " + GroupRank.Name + "*", Bubble);

                    //Session.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id)));

                    Session.GetRoleplay().TimeWorked = 0;
                    Session.GetRoleplay().JobId = Group.Id;
                    Session.GetRoleplay().JobRank = 1;
                    Session.GetRoleplay().JobRequest = 0;

                    Group.AddNewMember(Session.GetHabbo().Id);
                    Group.SendPackets(Session);
                }
            }
            else
            {
                if (Group.GroupType == GroupType.LOCKED)
                {
                    Session.GetRoleplay().GangRequest = GroupId;
                    Group.Requests.Add(Session.GetHabbo().Id);

                    Session.SendWhisper("Sucesso, você solicitou com sucesso para se juntar à gangue '" + Group.Name + "'!", 1);

                    List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && (Group.IsAdmin(Client.GetHabbo().Id)) select Client).ToList();
                    foreach (GameClient Client in GroupAdmins)
                    {
                        Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, Session.GetHabbo(), 3));
                    }

                    Session.SendMessage(new GroupInfoComposer(Group, Session));

                    UserCache Junk = null;
                    PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(Session.GetHabbo().Id, out Junk);
                    PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Session.GetHabbo().Id);
                }
                else
                {
                    Session.Shout("*Entra na gangue '" + Group.Name + "' no cargo " + GroupRank.Name + "*", 4);

                    Session.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id)));

                    Session.GetRoleplay().GangId = Group.Id;
                    Session.GetRoleplay().GangRank = 1;
                    Session.GetRoleplay().GangRequest = 0;

                    Group.AddNewMember(Session.GetHabbo().Id);
                    Group.SendPackets(Session);
                }
            }
        }
    }
}
