using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Navigator
{
    class NavigatorPreferencesComposer : ServerPacket
    {
        public NavigatorPreferencesComposer()
            : base(ServerPacketHeader.NavigatorPreferencesMessageComposer)
        {
            base.WriteInteger(68);//X
            base.WriteInteger(42);//Y
            base.WriteInteger(425);//Width
            base.WriteInteger(592);//Height
            base.WriteBoolean(false);//Show or hide saved searches.
            base.WriteInteger(0);//No idea?
        }
    }
}

