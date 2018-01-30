using System;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class OnGuideSessionDetachedComposer : ServerPacket
    {
        public OnGuideSessionDetachedComposer(int id)
            : base(ServerPacketHeader.OnGuideSessionDetachedComposer)
        {
            if (id == 0 || id == 2)
                base.WriteInteger(id);
        }
    }
}