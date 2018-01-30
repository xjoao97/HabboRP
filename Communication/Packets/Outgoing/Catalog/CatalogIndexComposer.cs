using System.Collections.Generic;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    public class CatalogIndexComposer : ServerPacket
    {
        public CatalogIndexComposer(GameClient Session)
            : base(ServerPacketHeader.CatalogIndexMessageComposer)
        {
            WriteRootIndex(Session);

            foreach (CatalogPage Page in PlusEnvironment.GetGame().GetCatalog().GetChildPages(-1))
            {
                if (!CanAccessPage(Session, Page))
                    continue;

                WritePage(Page, CalcTreeSize(Session, PlusEnvironment.GetGame().GetCatalog().GetChildPages(Page.Id), Page.Id));

                foreach (CatalogPage PageChild in PlusEnvironment.GetGame().GetCatalog().GetChildPages(Page.Id))
                {
                    if (!CanAccessPage(Session, PageChild))
                        continue;

                    WritePage(PageChild, CalcTreeSize(Session, PlusEnvironment.GetGame().GetCatalog().GetChildPages(PageChild.Id), PageChild.Id));

                    foreach (CatalogPage SubPage in PlusEnvironment.GetGame().GetCatalog().GetChildPages(PageChild.Id))
                    {
                        if (!CanAccessPage(Session, SubPage))
                            continue;

                        WritePage(SubPage, 0);
                    }
                }
            }

            base.WriteBoolean(false);
            base.WriteString("NORMAL");
        }

        public void WriteRootIndex(GameClient Session)
        {
            base.WriteBoolean(true);
            base.WriteInteger(0);
            base.WriteInteger(-1);
            base.WriteString("root");
            base.WriteString(string.Empty);
            base.WriteInteger(0);
            base.WriteInteger(CalcTreeSize(Session, PlusEnvironment.GetGame().GetCatalog().GetChildPages(-1), -1));
        }

        public void WritePage(CatalogPage Page, int TreeSize)
        {
            base.WriteBoolean(Page.Visible);
            base.WriteInteger(Page.Icon);
            base.WriteInteger(Page.Id);
            base.WriteString(Page.PageLink);
            base.WriteString(Page.Caption);
            base.WriteInteger(Page.ItemOffers.Count);

            foreach (int i in Page.ItemOffers.Keys)
            {
                base.WriteInteger(i);
            }

            base.WriteInteger(TreeSize);
        }

        public int CalcTreeSize(GameClient Session, ICollection<CatalogPage> Pages, int ParentId)
        {
            int i = 0;
            foreach (CatalogPage Page in Pages)
            {
                if (Page.MinimumRank > Session.GetHabbo().Rank || (Page.MinimumVIP > Session.GetHabbo().VIPRank && Session.GetHabbo().Rank == 1) || Page.ParentId != ParentId)
                    continue;

                if (Page.ParentId == ParentId)
                    i++;
            }

            return i;
        }

        public bool CanAccessPage(GameClient Session, CatalogPage Page)
        {
            if (Page.MinimumRank > 0)
            {
                if (Session.GetHabbo().Rank < Page.MinimumRank)
                    return false;
            }

            if (Page.MinimumVIP > 0)
            {
                if (Session.GetHabbo().VIPRank < Page.MinimumVIP)
                    return false;
            }

            return true;
        }
    }
}