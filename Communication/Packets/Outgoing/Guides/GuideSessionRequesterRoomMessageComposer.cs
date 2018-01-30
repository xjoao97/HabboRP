using System;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class GuideSessionRequesterRoomMessageComposer : ServerPacket
    {
        public GuideSessionRequesterRoomMessageComposer(int id)
            : base(ServerPacketHeader.GuideSessionRequesterRoomMessageComposer)
        {
            if (id == 0 || id == 2)
                base.WriteInteger(id);
        }
    }
}