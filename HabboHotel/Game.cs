
using log4net;
using Plus.Database.Interfaces;

using Plus.Communication.Packets;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Support;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Televisions;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.LandingView;
using Plus.HabboHotel.Global;
using Plus.HabboHotel.Polls;

using Plus.HabboHotel.Games;

using Plus.HabboHotel.Rooms.Chat;
using Plus.HabboHotel.Talents;
using Plus.HabboHotel.Bots;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.Rewards;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Guides;
using System.Threading;
using System.Threading.Tasks;

using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Events;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Food;
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Turfs;
using Plus.HabboRoleplay.Houses;
using Plus.HabboHotel.Items.Crafting;
using Plus.HabboHotel.Roleplay.Web;
using Plus.HabboRoleplay.Farming;
using Plus.HabboRoleplay.Gambling;
using Plus.HabboRoleplay.Web.Util.ChatRoom;

namespace Plus.HabboHotel
{
    public class Game
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Game");

        private readonly PacketManager _packetManager;
        private readonly GameClientManager _clientManager;
        private readonly ModerationManager _modManager;
        private readonly ModerationTool _moderationTool;//TODO: Initialize from the moderation manager.
        private readonly ItemDataManager _itemDataManager;
        private readonly CatalogManager _catalogManager;
        private readonly TelevisionManager _televisionManager;//TODO: Initialize from the item manager.
        private readonly NavigatorManager _navigatorManager;
        private readonly RoomManager _roomManager;
        private readonly ChatManager _chatManager;
        private readonly GroupManager _groupManager;
        private readonly QuestManager _questManager;
        private readonly AchievementManager _achievementManager;
        private readonly TalentTrackManager _talentTrackManager;
        private readonly LandingViewManager _landingViewManager;//TODO: Rename class
        private readonly GameDataManager _gameDataManager;
        private readonly ServerStatusUpdater _globalUpdater;
        private readonly LanguageLocale _languageLocale;
        private readonly AntiMutant _antiMutant;
        private readonly BotManager _botManager;
        private readonly CacheManager _cacheManager;
        private readonly RewardManager _rewardManager;
        private readonly BadgeManager _badgeManager;
        private readonly PermissionManager _permissionManager;
        private readonly SubscriptionManager _subscriptionManager;
        private readonly GuideManager _guideManager;
        private readonly PollManager _pollManager;
        private readonly HouseManager _houseManager;
        private readonly WebEventManager _webEventManager;

        private bool _cycleEnded;
        private bool _cycleActive;
        private Task _gameCycle;
        private int _cycleSleepTime = 10;

        public Game()
        {
            this._packetManager = new PacketManager();

            this._clientManager = new GameClientManager();

            this._modManager = new ModerationManager();

            this._moderationTool = new ModerationTool();


            this._itemDataManager = new ItemDataManager();
            this._itemDataManager.Init();

            this._catalogManager = new CatalogManager();
            this._catalogManager.Init(this._itemDataManager);

            this._televisionManager = new TelevisionManager();

            this._navigatorManager = new NavigatorManager();

            this._roomManager = new RoomManager();

            this._chatManager = new ChatManager();

            this._questManager = new QuestManager();

            this._achievementManager = new AchievementManager();

            this._talentTrackManager = new TalentTrackManager();

            this._landingViewManager = new LandingViewManager();

            this._gameDataManager = new GameDataManager();

            this._globalUpdater = new ServerStatusUpdater();
            this._globalUpdater.Init();

            this._languageLocale = new LanguageLocale();

            this._antiMutant = new AntiMutant();

            this._botManager = new BotManager();

            this._cacheManager = new CacheManager();

            this._rewardManager = new RewardManager();

            this._badgeManager = new BadgeManager();
            this._badgeManager.Init();

            this._permissionManager = new PermissionManager();
            this._permissionManager.Init();

            this._subscriptionManager = new SubscriptionManager();
            this._subscriptionManager.Init();

            this._guideManager = new GuideManager();

            int pollLoaded;
            this._pollManager = new PollManager();
            this._pollManager.Init(out pollLoaded);

            #region Roleplay Section
            RoleplayData.Initialize();
            EventManager.Initialize();
            CombatManager.Initialize();
            RoleplayGameManager.Initialize();

            this._groupManager = new GroupManager();
            this._groupManager.Initialize();

            TexasHoldEmManager.Initialize();
            TurfManager.Initialize();
            WeaponManager.Initialize();
            FoodManager.Initialize();
            FarmingManager.Initialize();
            CraftingManager.Initialize();
            LotteryManager.Initialize();
            ToDoManager.Initialize();
            BlackListManager.Initialize();
            BountyManager.Initialize();
            WebSocketChatManager.Initialiaze();

            this._houseManager = new HouseManager();
            this._houseManager.Init();

            this._webEventManager = new WebEventManager();
            this._webEventManager.Init();
            #endregion
        }

        public void StartGameLoop()
        {
            this._gameCycle = new Task(GameCycle);
            this._gameCycle.Start();

            this._cycleActive = true;
        }

        private void GameCycle()
        {
            while (this._cycleActive)
            {
                this._cycleEnded = false;

                PlusEnvironment.GetGame().GetRoomManager().OnCycle();
                PlusEnvironment.GetGame().GetClientManager().OnCycle();
                RoleplayGameManager.CheckAutomaticGames();

                this._cycleEnded = true;
                Thread.Sleep(this._cycleSleepTime);
            }
        }

        public void StopGameLoop()
        {
            this._cycleActive = false;

            while (!this._cycleEnded)
            {
                Thread.Sleep(this._cycleSleepTime);
            }
        }

        public PacketManager GetPacketManager()
        {
            return _packetManager;
        }

        public GameClientManager GetClientManager()
        {
            return _clientManager;
        }

        public CatalogManager GetCatalog()
        {
            return _catalogManager;
        }

        public NavigatorManager GetNavigator()
        {
            return _navigatorManager;
        }

        public ItemDataManager GetItemManager()
        {
            return _itemDataManager;
        }

        public RoomManager GetRoomManager()
        {
            return _roomManager;
        }

        public AchievementManager GetAchievementManager()
        {
            return _achievementManager;
        }

        public WebEventManager GetWebEventManager()
        {
            return _webEventManager;
        }

        public TalentTrackManager GetTalentTrackManager()
        {
            return _talentTrackManager;
        }

        public ModerationTool GetModerationTool()
        {
            return _moderationTool;
        }

        public ModerationManager GetModerationManager()
        {
            return this._modManager;
        }

        public PermissionManager GetPermissionManager()
        {
            return this._permissionManager;
        }

        public SubscriptionManager GetSubscriptionManager()
        {
            return this._subscriptionManager;
        }

        public QuestManager GetQuestManager()
        {
            return this._questManager;
        }

        public GroupManager GetGroupManager()
        {
            return _groupManager;
        }

        public LandingViewManager GetLandingManager()
        {
            return _landingViewManager;
        }
        public TelevisionManager GetTelevisionManager()
        {
            return _televisionManager;
        }

        internal GuideManager GetGuideManager()
        {
            return _guideManager;
        }
        internal PollManager GetPollManager()
        {
            return _pollManager;
        }

        public ChatManager GetChatManager()
        {
            return this._chatManager;
        }

        public GameDataManager GetGameDataManager()
        {
            return this._gameDataManager;
        }

        public HouseManager GetHouseManager()
        {
            return this._houseManager;
        }

        public LanguageLocale GetLanguageLocale()
        {
            return this._languageLocale;
        }

        public AntiMutant GetAntiMutant()
        {
            return this._antiMutant;
        }

        public BotManager GetBotManager()
        {
            return this._botManager;
        }

        public CacheManager GetCacheManager()
        {
            return this._cacheManager;
        }

        public RewardManager GetRewardManager()
        {
            return this._rewardManager;
        }

        public BadgeManager GetBadgeManager()
        {
            return this._badgeManager;
        }
    }
}