using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class BadgeEditorPartsComposer : ServerPacket
    {
        public BadgeEditorPartsComposer(ICollection<GroupBases> Bases, ICollection<GroupSymbols> Symbols, ICollection<GroupBaseColours> BaseColours, Dictionary<int, GroupSymbolColours> SymbolColours,
            Dictionary<int, GroupBackGroundColours> BackgroundColours)
            : base(ServerPacketHeader.BadgeEditorPartsMessageComposer)
        {
            base.WriteInteger(Bases.Count);
            foreach (GroupBases Item in Bases)
            {
                base.WriteInteger(Item.Id);
               base.WriteString(Item.Value1);
               base.WriteString(Item.Value2);
            }

            base.WriteInteger(Symbols.Count);
            foreach (GroupSymbols Item in Symbols)
            {
                base.WriteInteger(Item.Id);
               base.WriteString(Item.Value1);
               base.WriteString(Item.Value2);
            }

            base.WriteInteger(BaseColours.Count);
            foreach (GroupBaseColours Colour in BaseColours)
            {
                base.WriteInteger(Colour.Id);
               base.WriteString(Colour.Colour);
            }

            base.WriteInteger(SymbolColours.Count);
            foreach (GroupSymbolColours Colour in SymbolColours.Values.ToList())
            {
                base.WriteInteger(Colour.Id);
               base.WriteString(Colour.Colour);
            }

            base.WriteInteger(BackgroundColours.Count);
            foreach (GroupBackGroundColours Colour in BackgroundColours.Values.ToList())
            {
                base.WriteInteger(Colour.Id);
               base.WriteString(Colour.Colour);
            }
        }
    }
}