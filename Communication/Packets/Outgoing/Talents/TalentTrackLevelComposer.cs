using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Talents
{
    class TalentTrackLevelComposer : ServerPacket
    {
        public TalentTrackLevelComposer()
            : base(ServerPacketHeader.TalentTrackLevelMessageComposer)
        {
           base.WriteString("Cidadania");
            base.WriteInteger(0);
            base.WriteInteger(4);
        }
    }
}