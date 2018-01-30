using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Navigator
{
    class NavigatorSearchResultSetComposer : ServerPacket
    {
        public NavigatorSearchResultSetComposer(string Category, string Data, ICollection<SearchResultList> SearchResultLists, GameClient Session, int GoBack = 1, int FetchLimit = 100)
            : base(ServerPacketHeader.NavigatorSearchResultSetMessageComposer)
        {
           base.WriteString(Category);//Search code.
           base.WriteString(Data);//Text?

            base.WriteInteger(SearchResultLists.Count);//Count
            foreach (SearchResultList SearchResult in SearchResultLists.ToList())
            {
               base.WriteString(SearchResult.CategoryIdentifier);
               base.WriteString(SearchResult.PublicName);
                base.WriteInteger(NavigatorSearchAllowanceUtility.GetIntegerValue(SearchResult.SearchAllowance) != 0 ? GoBack : NavigatorSearchAllowanceUtility.GetIntegerValue(SearchResult.SearchAllowance));//0 = nothing, 1 = show more, 2 = back Action allowed.
                base.WriteBoolean(false);//True = minimized, false = open.
                base.WriteInteger(SearchResult.ViewMode == NavigatorViewMode.REGULAR ? 0 : SearchResult.ViewMode == NavigatorViewMode.THUMBNAIL ? 1 : 0);//View mode, 0 = tiny/regular, 1 = thumbnail

                NavigatorHandler.Search(this, SearchResult, Data, Session, FetchLimit);
            }
        }
    }
}
