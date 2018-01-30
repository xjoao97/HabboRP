using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Cache;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class RemoveGroupMemberEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int UserId = Packet.PopInt();

            Group Group = null;
            Group RemoveGroup = null;

            if (GroupId < 1000)
                Group = GroupManager.GetJob(GroupId);
            else
                Group = GroupManager.GetGang(GroupId);

            if (Group == null)
                return;

            if (GroupId == 1)
                return;

            if (GroupId == 1000)
                return;

            if (!Group.IsMember(UserId))
                return;

            UserCache Junk = null;
            PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(UserId, out Junk);
            PlusEnvironment.GetGame().GetCacheManager().GenerateUser(UserId);

            bool CanRemove = false;
            if (UserId == Session.GetHabbo().Id || Group.CreatorId == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id) || Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                CanRemove = true;

            if (!CanRemove)
            {
                if (Group.Id < 1000)
                {
                    if (Group.IsAdmin(UserId) && Group.CreatorId != Session.GetHabbo().Id)
                    {
                        if (!Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
                        {
                            Session.SendWhisper("Desculpe, apenas um gerente pode adicionar outro!", 1);
                            return;
                        }
                    }
                }
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);

            Group NewGroup;
            if (GroupId < 1000)
                NewGroup = GroupManager.GetJob(1);
            else
                NewGroup = GroupManager.GetGang(1000);

            #region (Disabled) Remove Room Rights
            /*
            if ((Group.AdminOnlyDeco == 0 || Group.IsAdmin(UserId)) && Client != null && Client.GetRoomUser() != null)
            {
                Room Room;
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
                    return;

                RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                if (User != null)
                {
                    User.RemoveStatus("flatctrl 1");
                    User.UpdateNeeded = true;
                    Client.SendMessage(new YouAreControllerComposer(0));
                }
            }*/
            #endregion

            if (Session.GetHabbo().Id == UserId)
            {
                UpdateGroupData(Group, Session);
                NewGroup.AddNewMember(UserId);
                NewGroup.SendPackets(Session);

                if (Group.Id < 1000)
                    Session.Shout("*Se demite da empresa " + Group.Name + "*", 4);
                else
                    Session.Shout("*Sai da gangue " + Group.Name + "*", 4);
            }
            else
            {
                GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                if (Client != null && Client.GetHabbo() != null && Client.GetRoleplay() != null)
                {
                    UpdateGroupData(Group, Client);
                    NewGroup.AddNewMember(UserId);
                    NewGroup.SendPackets(Client);
                    Group.SendMembersPackets(Client);
                }
                else
                {
                    NewGroup.AddNewMember(UserId);
                    Group.SendMembersPackets(Session);
                }

                int Bubble = Session.GetHabbo().GetPermissions().HasRight("mod_tool") ? 23 : 4;
                if (Group.CreatorId == Session.GetHabbo().Id)
                    Bubble = 4;

                string Username = Habbo == null ? "alguém" : Habbo.Username;
                if (Group.Id < 1000)
                    Session.Shout("*Demite " + Username + " da empresa " + Group.Name + "*", Bubble);
                else
                    Session.Shout("*Expulsa " + Username + " da gangue " + Group.Name + "*", Bubble);
            }


            if (Group.Id >= 1000)
            { 
                foreach (int Member in Group.Members.Keys)
                {
                    GameClient GangMember = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);

                    if (GangMember == null)
                        continue;

                    GangMember.SendWhisper("[GANGUE] " + Habbo == null ? "alguém" : Habbo.Username + " acabou de sair do grupo!", 34);
                }
            }
        }

        public void UpdateGroupData(Group Group, GameClient Session)
        {
            if (Group == null || Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null)
                return;

            if (Group.Id < 1000)
            {
                if (Session.GetRoleplay().IsWorking)
                {
                    WorkManager.RemoveWorkerFromList(Session);
                    Session.GetRoleplay().IsWorking = false;
                    Session.GetHabbo().Poof();

                    if (GroupManager.HasJobCommand(Session, "guide"))
                    {
                        PlusEnvironment.GetGame().GetGuideManager().RemoveGuide(Session);
                        Session.SendMessage(new HelperToolConfigurationComposer(Session));
                    }
                }
                Session.GetRoleplay().TimeWorked = 0;
                Session.GetRoleplay().JobId = 1;
                Session.GetRoleplay().JobRank = 1;
                Session.GetRoleplay().JobRequest = 0;
            }
            else
            {
                Session.GetRoleplay().GangId = 1000;
                Session.GetRoleplay().GangRank = 1;
                Session.GetRoleplay().GangRequest = 0;
            }
        }
    }
}