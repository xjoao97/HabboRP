using System;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class OnGuideSessionError : ServerPacket
    {
        public OnGuideSessionError()
            : base(ServerPacketHeader.OnGuideSessionError)
        {
            base.WriteInteger(0);
        }
    }
}