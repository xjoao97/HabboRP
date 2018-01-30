using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using log4net;

using Plus.HabboHotel.Items;
using Plus.HabboHotel.Catalog.Pets;
using Plus.HabboHotel.Catalog.Vouchers;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.Catalog.Clothing;

using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Catalog
{
    public class CatalogManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Catalog.CatalogManager");

        private MarketplaceManager _marketplace;
        private PetRaceManager _petRaceManager;
        private VoucherManager _voucherManager;
        private ClothingManager _clothingManager;

        private Dictionary<int, int> _itemOffers;
        private Dictionary<int, CatalogPage> _pages;
        private Dictionary<int, CatalogBundle> _bundles;
        private Dictionary<int, CatalogBot> _botPresets;
        private Dictionary<int, Dictionary<int, CatalogItem>> _items;
        private Dictionary<int, Dictionary<int, CatalogDeal>> _deals;
        private List<CatalogItem> _clubitems;

        public CatalogManager()
        {
            this._marketplace = new MarketplaceManager();
            this._petRaceManager = new PetRaceManager();
            this._voucherManager = new VoucherManager();
            this._clothingManager = new ClothingManager();

            this._itemOffers = new Dictionary<int, int>();
            this._pages = new Dictionary<int, CatalogPage>();
            this._bundles = new Dictionary<int, CatalogBundle>();
            this._botPresets = new Dictionary<int, CatalogBot>();
            this._items = new Dictionary<int, Dictionary<int, CatalogItem>>();
            this._deals = new Dictionary<int, Dictionary<int, CatalogDeal>>();
            this._clubitems = new List<CatalogItem>();
        }

        public void Init(ItemDataManager ItemDataManager)
        {
            if (this._pages.Count > 0)
                this._pages.Clear();
            if (this._bundles.Count > 0)
                this._bundles.Clear();
            if (this._botPresets.Count > 0)
                this._botPresets.Clear();
            if (this._items.Count > 0)
                this._items.Clear();
            if (this._deals.Count > 0)
                this._deals.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`item_id`,`catalog_name`,`cost_credits`,`cost_pixels`,`cost_diamonds`,`amount`,`page_id`,`limited_sells`,`limited_stack`,`offer_active`,`extradata`,`badge`,`offer_id` FROM `catalog_items`");
                DataTable CatalogueItems = dbClient.getTable();

                if (CatalogueItems != null)
                {
                    foreach (DataRow Row in CatalogueItems.Rows)
                    {
                        if (Convert.ToInt32(Row["amount"]) <= 0)
                            continue;

                        int ItemId = Convert.ToInt32(Row["id"]);
                        int PageId = Convert.ToInt32(Row["page_id"]);
                        int BaseId = Convert.ToInt32(Row["item_id"]);
                        int OfferId = Convert.ToInt32(Row["offer_id"]);

                        ItemData Data = null;
                        if (!ItemDataManager.GetItem(BaseId, out Data))
                        {
                            log.Error("Não foi possível carregar o item " + ItemId + " no catalogo, nenhum registro de móveis encontrado.");
                            continue;
                        }

                        if (!this._items.ContainsKey(PageId))
                            this._items[PageId] = new Dictionary<int, CatalogItem>();

                        if (OfferId != -1 && !this._itemOffers.ContainsKey(OfferId))
                            this._itemOffers.Add(OfferId, PageId);

                        var Item = new CatalogItem(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["item_id"]),
                            Data, Convert.ToString(Row["catalog_name"]), Convert.ToInt32(Row["page_id"]), Convert.ToInt32(Row["cost_credits"]), Convert.ToInt32(Row["cost_pixels"]), Convert.ToInt32(Row["cost_diamonds"]),
                            Convert.ToInt32(Row["amount"]), Convert.ToInt32(Row["limited_sells"]), Convert.ToInt32(Row["limited_stack"]), PlusEnvironment.EnumToBool(Row["offer_active"].ToString()),
                            Convert.ToString(Row["extradata"]), Convert.ToString(Row["badge"]), Convert.ToInt32(Row["offer_id"]));

                        this._items[PageId].Add(Item.Id, Item);
                    }
                }

                dbClient.SetQuery("SELECT `id`,`item_id`,`catalog_name`,`cost_credits`,`cost_pixels`,`cost_diamonds`,`amount`,`page_id`,`limited_sells`,`limited_stack`,`offer_active`,`extradata`,`badge`,`offer_id` FROM `catalog_items` WHERE `catalog_name` LIKE 'HABBO_CLUB_VIP%'");
                DataTable HoloRPClubItems = dbClient.getTable();

                if (HoloRPClubItems != null)
                {
                    foreach (DataRow Row in HoloRPClubItems.Rows)
                    {
                        if (Convert.ToInt32(Row["amount"]) <= 0)
                            continue;

                        int ItemId = Convert.ToInt32(Row["id"]);
                        int PageId = Convert.ToInt32(Row["page_id"]);
                        int BaseId = Convert.ToInt32(Row["item_id"]);
                        int OfferId = Convert.ToInt32(Row["offer_id"]);

                        ItemData Data = null;
                        if (!ItemDataManager.GetItem(BaseId, out Data))
                        {
                            log.Error("Não foi possível carregar o item " + ItemId + ", no catalogo, nenhum registro de móveis encontrado.");
                            continue;
                        }

                        var Item = new CatalogItem(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["item_id"]),
                        Data, Row.IsNull("catalog_name") ? "HabboRPG Clube" : Convert.ToString(Row["catalog_name"]), Convert.ToInt32(Row["page_id"]), Convert.ToInt32(Row["cost_credits"]), Convert.ToInt32(Row["cost_pixels"]), Convert.ToInt32(Row["cost_diamonds"]),
                        Convert.ToInt32(Row["amount"]), Convert.ToInt32(Row["limited_sells"]), Convert.ToInt32(Row["limited_stack"]), PlusEnvironment.EnumToBool(Row["offer_active"].ToString()),
                        Convert.ToString(Row["extradata"]), Convert.ToString(Row["badge"]), Convert.ToInt32(Row["offer_id"]));

                        if (!this._clubitems.Contains(Item))
                            this._clubitems.Add(Item);
                    }
                }

                dbClient.SetQuery("SELECT * FROM `catalog_deals`");
                DataTable GetDeals = dbClient.getTable();

                if (GetDeals != null)
                {
                    foreach (DataRow Row in GetDeals.Rows)
                    {
                        int Id = Convert.ToInt32(Row["id"]);
                        int PageId = Convert.ToInt32(Row["page_id"]);
                        string Items = Convert.ToString(Row["items"]);
                        string Name = Convert.ToString(Row["name"]);
                        int Credits = Convert.ToInt32(Row["cost_credits"]);
                        int Pixels = Convert.ToInt32(Row["cost_pixels"]);

                        if (!this._deals.ContainsKey(PageId))
                            this._deals[PageId] = new Dictionary<int, CatalogDeal>();

                        CatalogDeal Deal = new CatalogDeal(Id, PageId, Items, Name, Credits, Pixels, ItemDataManager);
                        this._deals[PageId].Add(Deal.Id, Deal);
                    }
                }


                dbClient.SetQuery("SELECT * FROM `catalog_pages` ORDER BY `order_num`");
                DataTable CatalogPages = dbClient.getTable();

                if (CatalogPages != null)
                {
                    foreach (DataRow Row in CatalogPages.Rows)
                    {
                        int Id = Convert.ToInt32(Row["id"]);
                        int ParentId = Convert.ToInt32(Row["parent_id"]);
                        bool Enabled = PlusEnvironment.EnumToBool(Row["enabled"].ToString());
                        string Name = Row["caption"].ToString();
                        string PageLink = Row["page_link"].ToString();
                        int Icon = Convert.ToInt32(Row["icon_image"]);
                        int MinRank = Convert.ToInt32(Row["min_rank"]);
                        int MinVIP = Convert.ToInt32(Row["min_vip"]);
                        bool Visible = PlusEnvironment.EnumToBool(Row["visible"].ToString());
                        string Layout = Row["page_layout"].ToString();
                        string Strings1 = Row["page_strings_1"].ToString();
                        string Strings2 = Row["page_strings_2"].ToString();

                        Dictionary<int, CatalogItem> Items = this._items.ContainsKey(Id) ? this._items[Id] : new Dictionary<int, CatalogItem>();
                        Dictionary<int, CatalogDeal> Deals = this._deals.ContainsKey(Id) ? this._deals[Id] : new Dictionary<int, CatalogDeal>();

                        CatalogPage NewPage = new CatalogPage(Id, ParentId, Enabled, Name, PageLink, Icon, MinRank, MinVIP, Visible, Layout, Strings1, Strings2, Items, Deals, ref this._itemOffers);

                        if (!this._pages.ContainsKey(Id))
                            this._pages.Add(Id, NewPage);
                    }
                }

                dbClient.SetQuery("SELECT * FROM `catalog_pages_bundles`");
                DataTable CatalogBundles = dbClient.getTable();

                if (CatalogBundles != null)
                {
                    foreach (DataRow Row in CatalogBundles.Rows)
                    {
                        int Id = Convert.ToInt32(Row["id"]);
                        string Title = Row["title"].ToString();
                        string Image = Row["image"].ToString();
                        string Link = Row["link"].ToString();

                        CatalogBundle NewBundle = new CatalogBundle(Id, Title, Image, Link);

                        if (!this._bundles.ContainsKey(Id))
                            this._bundles.Add(Id, NewBundle);
                    }
                }

                dbClient.SetQuery("SELECT `id`,`name`,`figure`,`motto`,`gender`,`ai_type` FROM `catalog_bot_presets`");
                DataTable bots = dbClient.getTable();

                if (bots != null)
                {
                    foreach (DataRow Row in bots.Rows)
                    {
                        this._botPresets.Add(Convert.ToInt32(Row[0]), new CatalogBot(Convert.ToInt32(Row[0]), Convert.ToString(Row[1]), Convert.ToString(Row[2]), Convert.ToString(Row[3]), Convert.ToString(Row[4]), Convert.ToString(Row[5])));
                    }
                }

                this._petRaceManager.Init();
                this._clothingManager.Init();
            }

            //log.Info("Catalog Manager -> LOADED");
        }

        public bool TryGetBot(int ItemId, out CatalogBot Bot)
        {
            return this._botPresets.TryGetValue(ItemId, out Bot);
        }

        public Dictionary<int, int> ItemOffers
        {
            get { return this._itemOffers; }
        }

        public bool TryGetPage(int pageId, out CatalogPage page)
        {
            return this._pages.TryGetValue(pageId, out page);
        }

        public ICollection<CatalogPage> GetPages()
        {
            return this._pages.Values;
        }

        public MarketplaceManager GetMarketplace()
        {
            return this._marketplace;
        }

        public PetRaceManager GetPetRaceManager()
        {
            return this._petRaceManager;
        }

        public VoucherManager GetVoucherManager()
        {
            return this._voucherManager;
        }

        public ClothingManager GetClothingManager()
        {
            return this._clothingManager;
        }

        public List<CatalogItem> ClubItems()
        {
            return this._clubitems;
        }

        public List<CatalogPage> GetSubPages(int PageId)
        {
            return this._pages.Values.Where(x => x.ParentId == PageId).ToList();
        }

        public CatalogPage GetSectionPage(int PageId)
        {
            return this._pages.Values.FirstOrDefault(x => x.ParentId == -1 && GetSubPages(x.ParentId).Select(y => y.Id).ToList().Contains(PageId));
        }

        public List<CatalogBundle> GetBundles()
        {
            return this._bundles.Values.ToList();
        }

        public List<CatalogPage> GetChildPages(int PageId)
        {
            return this._pages.Values.Where(x => x.ParentId == PageId).ToList();
        }
    }
}