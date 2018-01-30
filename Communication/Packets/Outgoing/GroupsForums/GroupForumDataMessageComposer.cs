using System;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupForumDataMessageComposer : ServerPacket
    {
        public GroupForumDataMessageComposer(Group group, GameClient Session)
            : base(ServerPacketHeader.GroupForumDataMessageComposer)
        {
            string string1 = string.Empty, string2 = string.Empty, string3 = string.Empty, string4 = string.Empty;

            bool IsMember = false;
            bool IsAdmin = false;
            bool IsOwner = false;

            if (Session.GetHabbo().GetPermissions().HasRight("all_groups_member") || group.IsMember(Session.GetHabbo().Id))
                IsMember = true;
            if (Session.GetHabbo().GetPermissions().HasRight("all_groups_admin") || group.IsAdmin(Session.GetHabbo().Id))
                IsAdmin = true;
            if (Session.GetHabbo().GetPermissions().HasRight("all_groups_owner") || group.CreatorId == Session.GetHabbo().Id)
                IsOwner = true;

            base.WriteInteger(group.Id);
            base.WriteString(group.Name);
            base.WriteString(group.Description);
            base.WriteString(group.Badge);
            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(group.ForumMessagesCount);
            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(group.ForumLastPosterId);
            base.WriteString(group.ForumLastPosterName);
            base.WriteInteger(group.ForumLastPostTime);
            base.WriteInteger(group.WhoCanRead);
            base.WriteInteger(group.WhoCanPost);
            base.WriteInteger(group.WhoCanThread);
            base.WriteInteger(group.WhoCanMod);

            if (group.WhoCanRead == 1 && !IsMember)
                string1 = "not_member";
            if (group.WhoCanRead == 2 && !IsAdmin)
                string1 = "not_admin";
            if (group.WhoCanRead == 3 && !IsOwner)
                string1 = "not_owner";

            if (group.WhoCanPost == 1 && !IsMember)
                string2 = "not_member";
            if (group.WhoCanPost == 2 && !IsAdmin)
                string2 = "not_admin";
            if (group.WhoCanPost == 3 && !IsOwner)
                string2 = "not_owner";

            if (group.WhoCanThread == 1 && !IsMember)
                string3 = "not_member";
            if (group.WhoCanThread == 2 && !IsAdmin)
                string3 = "not_admin";
            if (group.WhoCanThread == 3 && !IsOwner)
                string3 = "not_owner";

            if (group.WhoCanMod == 2 && !IsAdmin)
                string4 = "not_admin";
            if (group.WhoCanMod == 3 && !IsOwner)
                string4 = "not_owner";

            base.WriteString(string1);
            base.WriteString(string2);
            base.WriteString(string3);
            base.WriteString(string4);
            base.WriteString(string.Empty);
            base.WriteBoolean(Session.GetHabbo().Id == group.CreatorId);
            base.WriteBoolean(true);
        }
    }
}