using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Cache;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupMembersComposer : ServerPacket
    {
        public GroupMembersComposer(Group Group, List<int> Members, int MembersCount, int Page, bool Admin, int ReqType, string SearchVal)
            : base(ServerPacketHeader.GroupMembersMessageComposer)
        {
            Members = Members.Where(x => PlusEnvironment.GetHabboById(x) != null).ToList();

            base.WriteInteger(Group.Id);
            base.WriteString(Group.Name);
            base.WriteInteger(Group.RoomId);
            base.WriteString(Group.Badge);
            base.WriteInteger(MembersCount);
            base.WriteInteger(Members.Count);

            if (MembersCount > 0)
            {
                foreach (int UserId in Members)
                {
                    if (UserId == 0)
                    {
                        HoloRPOwner();
                        continue;
                    }

                    Habbo Data = PlusEnvironment.GetHabboById(UserId);

                    base.WriteInteger(Group.CreatorId == Data.Id ? 0 : Group.IsAdmin(Data.Id) ? 1 : Group.IsMember(Data.Id) ? 2 : 3);
                    base.WriteInteger(Data.Id);
                    base.WriteString(Data.Username);
                    base.WriteString(Data.Look);
                    base.WriteString(string.Empty);
                }
            }
            base.WriteBoolean(Admin);
            base.WriteInteger(14);
            base.WriteInteger(Page);
            base.WriteInteger(ReqType);
            base.WriteString(SearchVal);
        }

        public void HoloRPOwner()
        {
            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteString("HabboRPG");
            base.WriteString("ch-3032-110-1408.cp-3204-1.ha-3129-100.sh-290-110.hr-831-37.he-1604-63.fa-1206-1325.lg-270-100.hd-180-2.cc-3039-100");
            base.WriteString(string.Empty);
        }
    }
}