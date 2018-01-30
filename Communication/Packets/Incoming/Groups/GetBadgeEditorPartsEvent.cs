using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Groups;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class GetBadgeEditorPartsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new BadgeEditorPartsComposer(
                PlusEnvironment.GetGame().GetGroupManager().Bases,
                PlusEnvironment.GetGame().GetGroupManager().Symbols,
                PlusEnvironment.GetGame().GetGroupManager().BaseColours,
                PlusEnvironment.GetGame().GetGroupManager().SymbolColours,
                PlusEnvironment.GetGame().GetGroupManager().BackGroundColours));
       
        }
    }
}
