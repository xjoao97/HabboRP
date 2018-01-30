using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Relationships;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    class BuddyListComposer : ServerPacket
    {
        public BuddyListComposer(ICollection<MessengerBuddy> Friends, Habbo Player)
            : base(ServerPacketHeader.BuddyListMessageComposer)
        {
            base.WriteInteger(1);
            base.WriteInteger(0);

            base.WriteInteger(Friends.Count);
            foreach (MessengerBuddy Friend in Friends.ToList())
            {
                Relationship Relationship = Player.Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(Friend.UserId)).Value;

                base.WriteInteger(Friend.Id);
               base.WriteString(Friend.mUsername);
                base.WriteInteger(1);//Gender.
                base.WriteBoolean(Friend.IsOnline || Friend.isBot);
                base.WriteBoolean(Friend.IsOnline && Friend.InRoom || Friend.isBot);
               base.WriteString(Friend.IsOnline || Friend.isBot ? Friend.mLook : string.Empty);
                base.WriteInteger(0); // category id
               base.WriteString(Friend.IsOnline  || Friend.isBot ? Friend.mMotto : string.Empty);
               base.WriteString(string.Empty);//Alternative name?
               base.WriteString(string.Empty);
                base.WriteBoolean(true);
                base.WriteBoolean(false);
                base.WriteBoolean(false);//Pocket Habbo user.
                base.WriteShort(Relationship == null ? 0 : Relationship.Type);
            }

        }
    }
}
