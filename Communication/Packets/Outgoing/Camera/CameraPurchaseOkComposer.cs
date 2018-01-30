using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;


namespace Plus.Communication.Packets.Outgoing.Camera
{
    class CameraPurchaseOkComposer : ServerPacket
    {
        public CameraPurchaseOkComposer()
            : base(ServerPacketHeader.CameraPurchaseOkComposer)
        {

        }
    }
}
