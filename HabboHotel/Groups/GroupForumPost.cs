using System;
using System.Data;

namespace Plus.HabboHotel.Groups
{
    /// <summary>
    /// Class GroupForumPost.
    /// </summary>
    internal class GroupForumPost
    {
        /// <summary>
        /// The identifier
        /// </summary>
        internal int Id;

        /// <summary>
        /// The parent identifier
        /// </summary>
        internal int ParentId;

        /// <summary>
        /// The group identifier
        /// </summary>
        internal int GroupId;

        /// <summary>
        /// The timestamp
        /// </summary>
        internal int Timestamp;

        /// <summary>
        /// The pinned
        /// </summary>
        internal bool Pinned;

        /// <summary>
        /// The locked
        /// </summary>
        internal bool Locked;

        /// <summary>
        /// The hidden
        /// </summary>
        internal bool Hidden;

        /// <summary>
        /// The poster identifier
        /// </summary>
        internal int PosterId;

        /// <summary>
        /// The poster name
        /// </summary>
        internal string PosterName;

        /// <summary>
        /// The poster look
        /// </summary>
        internal string PosterLook;

        /// <summary>
        /// The subject
        /// </summary>
        internal string Subject;

        /// <summary>
        /// The post content
        /// </summary>
        internal string PostContent;

        /// <summary>
        /// The message count
        /// </summary>
        internal int MessageCount;

        /// <summary>
        /// The hider
        /// </summary>
        internal int Hider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupForumPost" /> class.
        /// </summary>
        /// <param name="row">The row.</param>
        internal GroupForumPost(DataRow row)
        {
            Id = Convert.ToInt32(row["id"]);
            ParentId = Convert.ToInt32(row["parent_id"]);
            GroupId = Convert.ToInt32(row["group_id"]);
            Timestamp = Convert.ToInt32(row["timestamp"]);
            Pinned = row["pinned"].ToString() == "1";
            Locked = row["locked"].ToString() == "1";
            Hidden = row["hidden"].ToString() == "1";
            PosterId = Convert.ToInt32(row["poster_id"]);
            PosterName = row["poster_name"].ToString();
            PosterLook = row["poster_look"].ToString();
            Subject = row["subject"].ToString();
            PostContent = row["post_content"].ToString();
            Hider = Convert.ToInt32(row["post_hider"]);
            MessageCount = 0;

            if (ParentId == 0)
                MessageCount = GroupManager.GetMessageCountForThread(Id);
        }
    }
}