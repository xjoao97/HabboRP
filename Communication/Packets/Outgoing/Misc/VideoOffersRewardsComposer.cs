using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Handshake
{
    class VideoOffersRewardsComposer : ServerPacket
    {
        public VideoOffersRewardsComposer(int Id, string Type, string Message)
            : base(ServerPacketHeader.VideoOffersRewardsMessageComposer)
        {
            base.WriteString(Type);
            base.WriteInteger(Id);
            base.WriteString(Message);
            base.WriteString("");
        }
    }
}

 