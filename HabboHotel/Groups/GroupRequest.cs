using System;

namespace Plus.HabboHotel.Groups
{
    public class GroupRequest
    {
        #region Variables
        public int GroupId;
        public int UserId;
        #endregion

        public GroupRequest(int GroupId, int UserId)
        {
            this.GroupId = GroupId;
            this.UserId = UserId;
        }
    }
}