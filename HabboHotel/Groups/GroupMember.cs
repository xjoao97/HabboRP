using System;

namespace Plus.HabboHotel.Groups
{
    public class GroupMember
    {
        #region Variables
        public int GroupId;
        public int UserId;
        public int UserRank;
        public bool IsAdmin;
        #endregion

        public GroupMember(int GroupId, int UserId, int UserRank, bool IsAdmin)
        {
            this.GroupId = GroupId;
            this.UserId = UserId;
            this.UserRank = UserRank;
            this.IsAdmin = IsAdmin;
        }
    }
}