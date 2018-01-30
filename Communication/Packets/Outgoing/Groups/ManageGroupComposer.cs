using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class ManageGroupComposer : ServerPacket
    {
        public ManageGroupComposer(Group Group)
            : base(ServerPacketHeader.ManageGroupMessageComposer)
        {
            base.WriteInteger(0);
            base.WriteBoolean(true);
            base.WriteInteger(Group.Id);
            base.WriteString(Group.Name);
            base.WriteString(Group.Description);
            base.WriteInteger(1);
            base.WriteInteger(Group.Colour1);
            base.WriteInteger(Group.Colour2);
            base.WriteInteger(Group.GroupType == GroupType.OPEN ? 0 : Group.GroupType == GroupType.LOCKED ? 1 : 2);
            base.WriteInteger(Group.AdminOnlyDeco);
            base.WriteBoolean(false);
            base.WriteString("");

            string FakeBadge = "b05114s06114";
            string[] BadgeSplit = null;

            if (Group.Id < 1000)
                BadgeSplit = FakeBadge.Replace("b", "").Split('s');
            else
                BadgeSplit = Group.Badge.Replace("b", "").Split('s');

            this.WriteInteger(5);
            int Req = 5 - BadgeSplit.Length;
            int Final = 0;
            string[] array2 = BadgeSplit;
            for (int i = 0; i < array2.Length; i++)
            {
                string Symbol = array2[i];
                this.WriteInteger((Symbol.Length >= 6) ? int.Parse(Symbol.Substring(0, 3)) : int.Parse(Symbol.Substring(0, 2)));
                this.WriteInteger((Symbol.Length >= 6) ? int.Parse(Symbol.Substring(3, 2)) : int.Parse(Symbol.Substring(2, 2)));
                this.WriteInteger(Symbol.Length < 5 ? 0 : Symbol.Length >= 6 ? int.Parse(Symbol.Substring(5, 1)) : int.Parse(Symbol.Substring(4, 1)));
            }

            while (Final != Req)
            {
                this.WriteInteger(0);
                this.WriteInteger(0);
                this.WriteInteger(0);
                Final++;
            }          

            base.WriteString(Group.Badge);
            base.WriteInteger(Group.Id < 1000 ? (Group.Members.Count + 1) : Group.Members.Count);
        }
    }
}
