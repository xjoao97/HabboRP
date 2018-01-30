using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Misc
{
    class InternalLinkComposer : ServerPacket
    {
        public InternalLinkComposer(string link)
            : base(ServerPacketHeader.InternalLinkComposer)
        {
            base.WriteString(link);
        }
    }
}
