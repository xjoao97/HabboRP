using System;
using System.Linq;
using System.Text;

using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class UpdateFavouriteGroupComposer : ServerPacket
    {
        public UpdateFavouriteGroupComposer(int Id, Group Group, int VirtualId)
            : base(ServerPacketHeader.UpdateFavouriteGroupMessageComposer)
        {
            base.WriteInteger(VirtualId);//Sends 0 on .COM
            base.WriteInteger(Group.Id);
            base.WriteInteger(3);
            base.WriteString(Group.Name);
        }
    }
}
