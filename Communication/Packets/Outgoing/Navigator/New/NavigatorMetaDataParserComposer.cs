using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator
{
    class NavigatorMetaDataParserComposer : ServerPacket
    {
        public NavigatorMetaDataParserComposer(ICollection<TopLevelItem> TopLevelItems)
            : base(ServerPacketHeader.NavigatorMetaDataParserMessageComposer)
        {
            base.WriteInteger(TopLevelItems.Count);//Count
            foreach (TopLevelItem TopLevelItem in TopLevelItems.ToList())
            {
                //TopLevelContext
               base.WriteString(TopLevelItem.SearchCode);//Search code
                base.WriteInteger(0);//Count of saved searches?
                /*{
                    //SavedSearch
                    base.WriteInteger(TopLevelItem.Id);//Id
                   base.WriteString(TopLevelItem.SearchCode);//Search code
                   base.WriteString(TopLevelItem.Filter);//Filter
                   base.WriteString(TopLevelItem.Localization);//localization
                }*/
            }
        }
    }
}
