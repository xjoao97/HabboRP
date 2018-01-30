using System;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class OnGuideSessionPartnerIsTypingComposer : ServerPacket
    {
        public OnGuideSessionPartnerIsTypingComposer(bool Typing)
            : base(ServerPacketHeader.OnGuideSessionPartnerIsTypingComposer)
        {
            base.WriteBoolean(Typing);
        }
    }
}