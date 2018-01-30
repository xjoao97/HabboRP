using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;


namespace Plus.Communication.Packets.Outgoing.Camera
{
    class SetCameraPriceMessageComposer : ServerPacket
    {
        public SetCameraPriceMessageComposer()
            : base(ServerPacketHeader.SetCameraPriceMessageComposer)
        {
            base.WriteInteger(2); //credits
            base.WriteInteger(0); //duckets
        }
    }
}
