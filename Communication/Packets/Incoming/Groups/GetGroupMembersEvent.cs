using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.Users.Authenticator;
using Plus.HabboHotel.Users.UserDataManagement;
using Plus.HabboHotel.Cache;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class GetGroupMembersEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int Page = Packet.PopInt();
            string SearchVal = Packet.PopString();
            int RequestType = Packet.PopInt();

            Group Group = null;

            if (GroupId < 1000)
                Group = GroupManager.GetJob(GroupId);
            else
                Group = GroupManager.GetGang(GroupId);

            if (Group.Members == null)
                return;

            List<int> Members = new List<int>();
            List<GroupMember> Administrators = Group.Members.Values.Where(x => x.IsAdmin).OrderBy(x => x.UserId).ToList();
            List<GroupMember> NonAdministrators = Group.Members.Values.Where(x => !x.IsAdmin).OrderBy(x => x.UserId).ToList();

            List<GroupMember> MembersToCheck = new List<GroupMember>();
            MembersToCheck.AddRange(Administrators);
            MembersToCheck.AddRange(NonAdministrators);

            MembersToCheck = MembersToCheck.Take(500).ToList();

            switch (RequestType)
            {
                case 1:
                    {
                        MembersToCheck = MembersToCheck.Where(x => x.IsAdmin).ToList();
                        break;
                    }
                case 2:
                    {
                        Members = Group.Requests;
                        break;
                    }
            }

            if (RequestType == 0 || RequestType == 1)
            {
                if (Group.Id <= 1000 && !Members.Contains(0))
                    Members.Add(0);

                foreach (GroupMember Member in MembersToCheck)
                {
                    if (!Members.Contains(Member.UserId))
                        Members.Add(Member.UserId);
                }
            }


            if (!string.IsNullOrEmpty(SearchVal))
                Members = Group.Members.Values.Where(x => PlusEnvironment.GetHabboById(x.UserId) != null && PlusEnvironment.GetHabboById(x.UserId).Username.StartsWith(SearchVal)).Select(x => x.UserId).ToList();

            int StartIndex = ((Page - 1) * 14 + 14);
            int FinishIndex = Members.Count;

            bool CanEditGroup = Group.Id == 1 ? false : true;

            bool CanEdit = false;
            if (CanEditGroup == true)
            {
                if (Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager") || Group.CreatorId == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id))
                    CanEdit = true;
            }

            Session.SendMessage(new GroupMembersComposer(Group, Members.Skip(StartIndex).Take(FinishIndex - StartIndex).ToList(), Members.Count, Page, CanEdit == true, RequestType, SearchVal));
        }
    }
}