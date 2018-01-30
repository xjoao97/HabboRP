using System;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class GuideSessionEndedMessageComposer : ServerPacket
    {
        public GuideSessionEndedMessageComposer(int id)
            : base(ServerPacketHeader.GuideSessionEndedMessageComposer)
        {
            if (id == 0 || id == 2)
                base.WriteInteger(id);
        }
    }
}