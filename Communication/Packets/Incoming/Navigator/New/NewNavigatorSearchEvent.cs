using System.Collections.Generic;

using Plus.HabboHotel.Navigator;
using Plus.Communication.Packets.Outgoing.Navigator;

namespace Plus.Communication.Packets.Incoming.Navigator
{
    class NewNavigatorSearchEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            string Category = Packet.PopString();
            string Unknown = Packet.PopString();

            if (!string.IsNullOrEmpty(Unknown))
            {
                Category = "hotel_view";
                ICollection<SearchResultList> Test = new List<SearchResultList>();

                SearchResultList Null = null;
                if (PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(0, out Null))
                {
                    Test.Add(Null);
                    Session.SendMessage(new NavigatorSearchResultSetComposer(Category, Unknown, Test, Session));
                }
            }
            else
            {
                //Fetch the categorys.
                ICollection<SearchResultList> Test = PlusEnvironment.GetGame().GetNavigator().GetCategorysForSearch(Category);
                if (Test.Count == 0)
                {
                    ICollection<SearchResultList> SecondTest = PlusEnvironment.GetGame().GetNavigator().GetResultByIdentifier(Category);
                    if (SecondTest.Count > 0)
                    {
                        Session.SendMessage(new NavigatorSearchResultSetComposer(Category, Unknown, SecondTest, Session, 2, 100));
                        return;
                    }
                }

                Session.SendMessage(new NavigatorSearchResultSetComposer(Category, Unknown, Test, Session));
            }
        }
    }
}
