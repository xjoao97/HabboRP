using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Timers;
using Plus.HabboRoleplay.Cooldowns;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Events;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboHotel.Catalog.Clothing;
using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboRoleplay.Bots;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Rooms;
using Fleck;
using Plus.HabboHotel.Polls;
using Plus.HabboRoleplay.Farming;
using Plus.HabboRoleplay.Web.Outgoing.Statistics;
using Plus.HabboHotel.Roleplay.Web.Incoming.Interactions;
using Plus.HabboRoleplay.Web.Util.ChatRoom;
using Newtonsoft.Json;
using Plus.HabboRoleplay.Timers.Types;

namespace Plus.HabboRoleplay.RoleplayUsers
{
    public class RoleplayUser
    {
        #region Saved Variables
        // Client Info
        GameClient Client;

        // Basic Info
        private uint mId;
        private int mLevel;
        private int mLevelEXP;
        private string mClass;
        private bool mPermanentClass;

        // Jobs
        private int mJobId;
        private int mJobRank;
        private int mJobRequest;

        // Human Needs
        private int mMaxHealth;
        private int mCurHealth;
        private int mMaxEnergy;
        private int mCurEnergy;
        private int mHunger;
        private int mHygiene;

        // Levelable Stats
        private int mIntelligence;
        private int mStrength;
        private int mStamina;

        // Jailed/Dead - Wanted/Probation - Sendhome - Cuffed
        private bool mIsDead;
        private int mDeadTimeLeft;
        private bool mIsJailed;
        private int mJailedTimeLeft;
        private bool mIsWanted;
        private int mWantedLevel;
        private int mWantedTimeLeft;
        private bool mOnProbation;
        private int mProbationTimeLeft;
        private int mSendHomeTimeLeft;
        private bool mCuffed;
        private int mCuffedTimeLeft;

        // Statistics
        private int mPunches;
        private int mKills;
        private int mHitKills;
        private int mGunKills;
        private int mDeaths;
        private int mCopDeaths;
        private int mTimeWorked;
        private int mArrests;
        private int mArrested;
        private int mEvasions;

        // Banking
        private int mBankAccount;
        private int mBankChequings;
        private int mBankSavings;

        // Special Data
        private int mLastKilled;
        private int mMarriedTo;

        // Gangs
        private int mGangId;
        private int mGangRank;
        private int mGangRequest;

        // Inventory
        private int mPhoneType;
        private List<string> mPhoneApps;
        private int mCarType;
        private int mCarFuel;
        private int mWeed;
        private int mCocaine;
        private int mCigarettes;
        private int mBullets;
        private int mDynamite;
        private ConcurrentDictionary<string, Weapon> mOwnedWeapons;

        // Farming
        private FarmingStats mFarmingStats;

        // Extra Variables for Levelable Stats
        private int mStrengthEXP;
        private int mStaminaEXP;
        private int mIntelligenceEXP;

        // Misc
        private string[] mRPQuests;
        private int mBrawlWins;
        private int mCwWins;
        private int mMwWins;
        private int mSoloQueueWins;
        private int mVIPBanned;
        private string mLastCoordinates;

        // Noob
        private bool mIsNoob;
        private int mNoobTimeLeft;

        // WebSocket
        private bool mBannedFromChatting;
        private bool mBannedFromMakingChat;
        #endregion

        #region UnSaved Variables
        // Toggles
        public bool DisableVIPA = false;
        public bool DisableRadio = false;

        // Work
        public bool IsWorking = false;

        // Noob
        public bool NoobWarned = false;
        public bool NoobWarned2 = false;

        // Outfits
        public string OriginalOutfit = null;
        public ClothingItem Clothing = null;
        public bool PurchasingClothing = false;

        // Combat
        public bool AmbassadorOnDuty = false;
        public bool StaffOnDuty = false;
        public bool InCombat = false;
        public bool CombatMode = false;
        public Weapon EquippedWeapon = null;
        public string LastCommand = "";
        public int GunShots = 0;

        // Police Related
        public bool PoliceTrial = false;
        public bool Jailbroken = false;
        public string WantedFor = "";
        public bool Trialled = false;

        // Farming
        public bool WateringCan = false;
        public FarmingItem FarmingItem = null;

        // Misc
        public int TextTimer = 0;
        public int RapeTimer = 0;
        public int KissTimer = 0;
        public int HugTimer = 0;
        public int SexTimer = 0;
        public int TexasHoldEmPlayer = 0;
        public bool CraftingCheck = true;
        public bool InsideTaxi = false;
        public bool AntiArrowCheck = false;
        public bool BeingHealed = false;
        public bool IsWorkingOut = false;
        public bool InShower = false;
        public Item RobItem = null;
        public bool FreeNameChange = false;
        public bool HighOffCocaine = false;
        public bool HighOffWeed = false;
        public ConcurrentDictionary<int, List<PollQuestion>> AnsweredPollQuestions;

        // Cars
        public bool DrivingCar = false;
        public int CarEnableId = 0;
        public int CarTimer = 0;

        // ATM Poll
        public string ATMAccount = "";
        public string ATMAction = "";
        public bool ATMFailed = false;

        // Manages the timers for the user
        public TimerManager TimerManager;

        // Manages the cooldowns for the user
        public CooldownManager CooldownManager;

        // Saved Cooldowns
        public ConcurrentDictionary<string, int> SpecialCooldowns = new ConcurrentDictionary<string, int>();

        // Manages the offers for the user
        public OfferManager OfferManager;

        // Handles the users data
        public UserDataHandler UserDataHandler;

        // Minigames
        public IGame Game = null;
        public RoleplayTeam Team = null;
        public bool GameSpawned = false;
        public bool NeedsRespawn = false;

        // Police
        public GameClient GuideOtherUser = null;
        public bool SentRealCall = false;
        public bool Sent911Call = false;
        public string CallMessage = "";
        public bool HandlingCalls = false;
        public bool HandlingJailbreaks = false;
        public bool HandlingHeists = false;

        // Turfs
        public Turf CapturingTurf = null;

        // WebSocket
        public int UserViewing = 0;
        public IWebSocketConnection WebSocketConnection
        {
            get
            {
                if (PlusEnvironment.GetGame().GetWebEventManager() != null)
                    return PlusEnvironment.GetGame().GetWebEventManager().GetUsersConnection(Client);
                else
                    return null;
            }
        }
        public WalkDirections WalkDirection = WalkDirections.None;
        public bool ArrowEnabled = false;
        public bool CaptchaSent = false;
        public int CaptchaTime = 0;
        public List<int> ATMAmount = new List<int>();
        public bool UsingAtm = false;
        public ConcurrentDictionary<string, WebSocketChatRoom> ChatRooms = new ConcurrentDictionary<string, WebSocketChatRoom>();
        public int socketChatSpamCount = 0;
        public int socketChatSpamTicks = 0;
        public double socketChatFloodTime = 0;
        public bool DownloadingApplication = false;

        #endregion

        #region Getters & Setters
        public int Level
        {
            get { return mLevel; }
            set { mLevel = value; }
        }
        public int LevelEXP
        {
            get { return mLevelEXP; }
            set { mLevelEXP = value; }
        }
        public string Class
        {
            get { return mClass; }
            set { mClass = value; }
        }
        public bool PermanentClass
        {
            get { return mPermanentClass; }
            set { mPermanentClass = value; }
        }
        public int JobId
        {
            get { return mJobId; }
            set { mJobId = value; }
        }
        public int JobRank
        {
            get { return mJobRank; }
            set { mJobRank = value; }
        }
        public int JobRequest
        {
            get { return mJobRequest; }
            set { mJobRequest = value; }
        }
        public int SendHomeTimeLeft
        {
            get { return mSendHomeTimeLeft; }
            set { mSendHomeTimeLeft = value; }
        }
        public int MaxHealth
        {
            get { return mMaxHealth; }
            set { mMaxHealth = value; }
        }
        public int CurHealth
        {
            get { return mCurHealth; }
            set { mCurHealth = value; EventManager.TriggerEvent("OnHealthChange", Client); }
        }
        public int MaxEnergy
        {
            get { return mMaxEnergy; }
            set { mMaxEnergy = value; }
        }
        public int CurEnergy
        {
            get { return mCurEnergy; }
            set
            {
                mCurEnergy = value;
                RefreshStatDialogue();
                UpdateInteractingUserDialogues();
            }
        }
        public int Hunger
        {
            get { return mHunger; }
            set { mHunger = value; }
        }
        public int Hygiene
        {
            get { return mHygiene; }
            set { mHygiene = value; }
        }
        public int Intelligence
        {
            get { return mIntelligence; }
            set { mIntelligence = value; }
        }
        public int Strength
        {
            get { return mStrength; }
            set { mStrength = value; }
        }
        public int Stamina
        {
            get { return mStamina; }
            set { mStamina = value; }
        }
        public bool IsDead
        {
            get { return mIsDead; }
            set { mIsDead = value; }
        }
        public int DeadTimeLeft
        {
            get { return mDeadTimeLeft; }
            set { mDeadTimeLeft = value; }
        }
        public bool IsJailed
        {
            get { return mIsJailed; }
            set { mIsJailed = value; }
        }
        public int JailedTimeLeft
        {
            get { return mJailedTimeLeft; }
            set { mJailedTimeLeft = value; }
        }
        public bool IsWanted
        {
            get { return mIsWanted; }
            set { mIsWanted = value; }
        }
        public int WantedLevel
        {
            get { return mWantedLevel; }
            set { mWantedLevel = value; }
        }
        public int WantedTimeLeft
        {
            get { return mWantedTimeLeft; }
            set { mWantedTimeLeft = value; }
        }
        public bool OnProbation
        {
            get { return mOnProbation; }
            set { mOnProbation = value; }
        }
        public int ProbationTimeLeft
        {
            get { return mProbationTimeLeft; }
            set { mProbationTimeLeft = value; }
        }
        public bool Cuffed
        {
            get { return mCuffed; }
            set { mCuffed = value; }
        }
        public int CuffedTimeLeft
        {
            get { return mCuffedTimeLeft; }
            set { mCuffedTimeLeft = value; }
        }
        public int Punches
        {
            get { return mPunches; }
            set { mPunches = value; }
        }
        public int Kills
        {
            get { return mKills; }
            set { mKills = value; }
        }
        public int HitKills
        {
            get { return mHitKills; }
            set { mHitKills = value; }
        }
        public int GunKills
        {
            get { return mGunKills; }
            set { mGunKills = value; }
        }
        public int Deaths
        {
            get { return mDeaths; }
            set { mDeaths = value; }
        }
        public int CopDeaths
        {
            get { return mCopDeaths; }
            set { mCopDeaths = value; }
        }
        public int TimeWorked
        {
            get { return mTimeWorked; }
            set { mTimeWorked = value; }
        }
        public int Arrests
        {
            get { return mArrests; }
            set { mArrests = value; }
        }
        public int Arrested
        {
            get { return mArrested; }
            set { mArrested = value; }
        }
        public int Evasions
        {
            get { return mEvasions; }
            set { mEvasions = value; }
        }
        public int BankAccount
        {
            get { return mBankAccount; }
            set { mBankAccount = value; }
        }
        public int BankChequings
        {
            get { return mBankChequings; }
            set { mBankChequings = value; }
        }
        public int BankSavings
        {
            get { return mBankSavings; }
            set { mBankSavings = value; }
        }
        public int PhoneType
        {
            get { return mPhoneType; }
            set { mPhoneType = value; }
        }

        public List<string> PhoneApps
        {
            get { return mPhoneApps; }
            set { mPhoneApps = value; }
        }

        public int CarType
        {
            get { return mCarType; }
            set { mCarType = value; }
        }
        public int CarFuel
        {
            get { return mCarFuel; }
            set { mCarFuel = value; }
        }
        public int Weed
        {
            get { return mWeed; }
            set { mWeed = value; }
        }
        public int Cocaine
        {
            get { return mCocaine; }
            set { mCocaine = value; }
        }
        public int Cigarettes
        {
            get { return mCigarettes; }
            set { mCigarettes = value; }
        }
        public int Bullets
        {
            get { return mBullets; }
            set { mBullets = value; }
        }
        public int Dynamite
        {
            get { return mDynamite; }
            set { mDynamite = value; }
        }
        public int LastKilled
        {
            get { return mLastKilled; }
            set { mLastKilled = value; }
        }
        public int MarriedTo
        {
            get { return mMarriedTo; }
            set { mMarriedTo = value; }
        }
        public int GangId
        {
            get { return mGangId; }
            set { mGangId = value; }
        }
        public int GangRank
        {
            get { return mGangRank; }
            set { mGangRank = value; }
        }
        public int GangRequest
        {
            get { return mGangRequest; }
            set { mGangRequest = value; }
        }
        public FarmingStats FarmingStats
        {
            get { return mFarmingStats; }
            set { mFarmingStats = value; }
        }
        public ConcurrentDictionary<string, Weapon> OwnedWeapons
        {
            get { return mOwnedWeapons; }
            set { mOwnedWeapons = value; }
        }
        public int StrengthEXP
        {
            get { return mStrengthEXP; }
            set { mStrengthEXP = value; }
        }
        public int StaminaEXP
        {
            get { return mStaminaEXP; }
            set { mStaminaEXP = value; }
        }
        public int IntelligenceEXP
        {
            get { return mIntelligenceEXP; }
            set { mIntelligenceEXP = value; }
        }
        public string[] RPQuests
        {
            get { return mRPQuests; }
            set { mRPQuests = value; }
        }
        public string LastCoordinates
        {
            get { return mLastCoordinates; }
            set { mLastCoordinates = value; }
        }
        public int BrawlWins
        {
            get { return mBrawlWins; }
            set { mBrawlWins = value; }
        }
        public int CwWins
        {
            get { return mCwWins; }
            set { mCwWins = value; }
        }
        public int MwWins
        {
            get { return mMwWins; }
            set { mMwWins = value; }
        }
        public int SoloQueueWins
        {
            get { return mSoloQueueWins; }
            set { mSoloQueueWins = value; }
        }
        public bool IsNoob
        {
            get { return mIsNoob; }
            set { mIsNoob = value; }
        }
        public int NoobTimeLeft
        {
            get { return mNoobTimeLeft; }
            set { mNoobTimeLeft = value; }
        }

        public bool BannedFromChatting
        {
            get { return mBannedFromChatting; }
            set { mBannedFromChatting = value; }
        }

        public bool BannedFromMakingChat
        {
            get { return mBannedFromMakingChat; }
            set { mBannedFromMakingChat = value; }
        }

        public int VIPBanned
        {
            get { return mVIPBanned; }
            set { mVIPBanned = value; }
        }

        public ConcurrentDictionary<int, RoleplayBot> BotFriendShips
        {
            get;
            private set;
        }
        public bool Invisible
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the class
        /// </summary>
        public RoleplayUser(GameClient Client, DataRow user, DataRow cooldown, DataRow farming)
        {
            // Client Info
            this.Client = Client;

            // Basic Info
            this.mId = Convert.ToUInt32(user["id"]);
            this.mLevel = Convert.ToInt32(user["level"]);
            this.mLevelEXP = Convert.ToInt32(user["level_exp"]);
            this.mClass = Convert.ToString(user["class"]);
            this.mPermanentClass = PlusEnvironment.EnumToBool(user["permanent_class"].ToString());

            // Work Related
            this.mJobId = Convert.ToInt32(user["job_id"]);
            this.mJobRank = Convert.ToInt32(user["job_rank"]);
            this.mJobRequest = Convert.ToInt32(user["job_request"]);
            this.mSendHomeTimeLeft = Convert.ToInt32(user["sendhome_time_left"]);

            // Human Needs
            this.mMaxHealth = Convert.ToInt32(user["maxhealth"]);
            this.mCurHealth = Convert.ToInt32(user["curhealth"]);
            this.mMaxEnergy = Convert.ToInt32(user["maxenergy"]);
            this.mCurEnergy = Convert.ToInt32(user["curenergy"]);
            this.mHunger = Convert.ToInt32(user["hunger"]);
            this.mHygiene = Convert.ToInt32(user["hygiene"]);

            // Levelable Statistics
            this.mIntelligence = Convert.ToInt32(user["intelligence"]);
            this.mStrength = Convert.ToInt32(user["strength"]);
            this.mStamina = Convert.ToInt32(user["stamina"]);

            // Extra Variables for Levelable Stats
            this.mIntelligence = Convert.ToInt32(user["intelligence_exp"]);
            this.mStrengthEXP = Convert.ToInt32(user["strength_exp"]);
            this.mStaminaEXP = Convert.ToInt32(user["stamina_exp"]);

            // Jailed - Dead - Wanted - Probation
            this.mIsDead = PlusEnvironment.EnumToBool(user["is_dead"].ToString());
            this.mDeadTimeLeft = Convert.ToInt32(user["dead_time_left"]);
            this.mIsJailed = PlusEnvironment.EnumToBool(user["is_jailed"].ToString());
            this.mJailedTimeLeft = Convert.ToInt32(user["jailed_time_left"]);
            this.mIsWanted = PlusEnvironment.EnumToBool(user["is_wanted"].ToString());
            this.mWantedLevel = Convert.ToInt32(user["wanted_level"]);
            this.mWantedTimeLeft = Convert.ToInt32(user["wanted_time_left"]);
            this.mOnProbation = PlusEnvironment.EnumToBool(user["on_probation"].ToString());
            this.mProbationTimeLeft = Convert.ToInt32(user["probation_time_left"]);
            this.mCuffed = PlusEnvironment.EnumToBool(user["is_cuffed"].ToString());
            this.mCuffedTimeLeft = Convert.ToInt32(user["cuffed_time_left"]);

            // Statistics
            this.mPunches = Convert.ToInt32(user["punches"]);
            this.mKills = Convert.ToInt32(user["kills"]);
            this.mHitKills = Convert.ToInt32(user["hit_kills"]);
            this.mGunKills = Convert.ToInt32(user["gun_kills"]);
            this.mDeaths = Convert.ToInt32(user["deaths"]);
            this.mCopDeaths = Convert.ToInt32(user["cop_deaths"]);
            this.mTimeWorked = Convert.ToInt32(user["time_worked"]);
            this.mArrests = Convert.ToInt32(user["arrests"]);
            this.mArrested = Convert.ToInt32(user["arrested"]);
            this.mEvasions = Convert.ToInt32(user["evasions"]);

            // Banking
            this.mBankAccount = Convert.ToInt32(user["bank_account"]);
            this.mBankChequings = Convert.ToInt32(user["bank_chequings"]);
            this.mBankSavings = Convert.ToInt32(user["bank_savings"]);

            // Affiliations
            this.mLastKilled = Convert.ToInt32(user["last_killed"]);
            this.mMarriedTo = Convert.ToInt32(user["married_to"]);

            // Gangs
            this.mGangId = Convert.ToInt32(user["gang_id"]);
            this.mGangRank = Convert.ToInt32(user["gang_rank"]);
            this.mGangRequest = Convert.ToInt32(user["gang_request"]);

            // Inventory
            this.mPhoneApps = Convert.ToString(user["phone_apps"]).Split(':').ToList();
            this.mPhoneType = Convert.ToInt32(user["phone"]);
            this.mCarType = Convert.ToInt32(user["car"]);
            this.mCarFuel = Convert.ToInt32(user["car_fuel"]);
            this.mWeed = Convert.ToInt32(user["weed"]);
            this.mCocaine = Convert.ToInt32(user["cocaine"]);
            this.mCigarettes = Convert.ToInt32(user["cigarette"]);
            this.mBullets = Convert.ToInt32(user["bullets"]);
            this.mDynamite = Convert.ToInt32(user["dynamite"]);
            this.mOwnedWeapons = LoadAndReturnWeapons();

            // Farming
            this.FarmingStats = new FarmingStats(farming);

            // Misc
            this.mRPQuests = user["unlocked_quests"].ToString().Split(',');
            this.mBrawlWins = Convert.ToInt32(user["brawl_wins"]);
            this.mCwWins = Convert.ToInt32(user["cw_wins"]);
            this.mMwWins = Convert.ToInt32(user["mw_wins"]);
            this.mSoloQueueWins = Convert.ToInt32(user["soloqueue_wins"]);
            this.mIsNoob = PlusEnvironment.EnumToBool(user["is_noob"].ToString());
            this.mNoobTimeLeft = Convert.ToInt32(user["noob_time_left"]);
            this.AnsweredPollQuestions = new ConcurrentDictionary<int, List<PollQuestion>>();
            this.mVIPBanned = Convert.ToInt32(user["vip_banned"]);
            this.mLastCoordinates = user["last_coordinates"].ToString();

            // Manages the timers for the user
            this.TimerManager = new TimerManager(Client);

            // Manages the cooldowns for the user
            this.CooldownManager = new CooldownManager(Client);
            this.SpecialCooldowns = this.LoadAndReturnCooldowns(cooldown);

            // Manages the offers for the user
            this.OfferManager = new OfferManager(Client);

            // Handles the users data
            this.UserDataHandler = new UserDataHandler(Client, this);

            // Handles bot friendships
            this.BotFriendShips = new ConcurrentDictionary<int, RoleplayBot>();

            // Fun stuff
            this.Invisible = false;

            // WebSocket
            if (user.Table.Columns.Contains("wchat_banned"))
                this.BannedFromChatting = PlusEnvironment.EnumToBool(Convert.ToString(user["wchat_banned"]));

            if (user.Table.Columns.Contains("wchat_making_banned"))
                this.BannedFromMakingChat = PlusEnvironment.EnumToBool(Convert.ToString(user["wchat_making_banned"]));
        }
        #endregion

        #region Methods

        /// <summary>
        /// Loads and returns the owned weapons
        /// </summary>
        /// <returns></returns>
        internal ConcurrentDictionary<string, Weapon> LoadAndReturnWeapons()
        {
            DataTable Weps = null;
            ConcurrentDictionary<string, Weapon> Weapons = new ConcurrentDictionary<string, Weapon>();

            Weapons.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_weapons_owned` WHERE `user_id` = '" + this.mId + "'");
                Weps = dbClient.getTable();
                uint id = 0;

                if (Weps != null)
                {
                    foreach (DataRow Row in Weps.Rows)
                    {
                        id++;

                        string basename = Convert.ToString(Row["base_weapon"]);
                        string name = Convert.ToString(Row["name"]);
                        int mindam = Convert.ToInt32(Row["min_damage"]);
                        int maxdam = Convert.ToInt32(Row["max_damage"]);
                        int range = Convert.ToInt32(Row["range"]);
                        bool canuse = Convert.ToBoolean(Row["can_use"]);

                        if (!Weapons.ContainsKey(basename))
                        {
                            Weapon BaseWeapon = WeaponManager.getWeapon(basename);

                            if (BaseWeapon != null)
                            {
                                Weapon Weapon = new Weapon(id, basename, name, BaseWeapon.FiringText, BaseWeapon.EquipText, BaseWeapon.UnEquipText, BaseWeapon.ReloadText, BaseWeapon.Energy, BaseWeapon.EffectID, BaseWeapon.HandItem, range, mindam, maxdam, BaseWeapon.ClipSize, BaseWeapon.ReloadTime, BaseWeapon.Cost, BaseWeapon.CostFine, BaseWeapon.Stock, BaseWeapon.LevelRequirement, canuse);

                                if (Weapon != null)
                                    Weapons.TryAdd(basename, Weapon);
                            }
                        }
                    }
                }
            }

            return Weapons;
        }

        /// <summary>
        /// Returns a true/false bool if friends with bot
        /// </summary>
        /// <param name="BotId"></param>
        /// <returns></returns>
        internal bool FriendsWithBot(int BotId)
        {
            return (BotFriendShips.ContainsKey(BotId));
        }

        /// <summary>
        /// Sends a message to a bot
        /// </summary>
        /// <param name="BotId"></param>
        /// <param name="Message"></param>
        internal void MessageBot(int BotId, string Message)
        {
            var serverMessage = new NewConsoleMessageComposer(Client.GetHabbo().Id, Message);
            serverMessage.WriteInteger(BotId); //userid
            serverMessage.WriteString(Client.GetHabbo().Username);
            serverMessage.WriteInteger(0);

            RoomUser Bot = RoleplayBotManager.GetDeployedBotById(BotId - RoleplayBotManager.BotFriendMultiplyer);
            Bot.GetBotRoleplayAI().OnMessaged(Client, Message);

        }

        /// <summary>
        /// Adds a bot friendship
        /// </summary>
        /// <param name="BotId"></param>
        internal void AddBotAsFriend(int BotId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO `rp_bots_friendships` (`bot_id`, `user_id`) VALUES ('" + BotId + "', '" + Client.GetHabbo().Id + "')");
            }

            LoadBotFriendships();
        }

        /// <summary>
        /// Removes a bot friendship
        /// </summary>
        /// <param name="BotId"></param>
        internal void RemoveBotAsFriend(int BotId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `rp_bots_friendships` WHERE `bot_id` = '" + BotId + "' AND `user_id` = '" + Client.GetHabbo().Id + "'");
            }

            Client.SendWhisper("Você removeu com sucesso " + RoleplayBotManager.GetCachedBotById(BotId).Name + " da sua lista de contatos do celular!", 1);
        }

        /// <summary>
        /// Loads bot friendships
        /// </summary>
        internal void LoadBotFriendships()
        {
            BotFriendShips = new ConcurrentDictionary<int, RoleplayBot>();

            DataTable BotFriends = null;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {

                dbClient.SetQuery("SELECT `bot_id` FROM `rp_bots_friendships` WHERE `user_id` = '" + Client.GetHabbo().Id + "'");
                BotFriends = dbClient.getTable();

                if (BotFriends == null)
                    return;

                foreach (DataRow BotUser in BotFriends.Rows)
                {
                    int BotId = Convert.ToInt32(BotUser["bot_id"]);

                    RoleplayBot Botfriend = RoleplayBotManager.GetCachedBotById(BotId);

                    if (Botfriend == null)
                        return;

                    MessengerBuddy newfriend = new MessengerBuddy(Botfriend.Id + RoleplayBotManager.BotFriendMultiplyer,
                      Botfriend.Name,
                      Botfriend.Figure,
                      Botfriend.Motto, 0, false, true, true);

                    int addid = Botfriend.Id + RoleplayBotManager.BotFriendMultiplyer;

                    if (Client.GetHabbo() == null)
                        return;

                    if (Client.GetHabbo().GetMessenger() == null)
                        return;

                    if (Client.GetHabbo().GetMessenger()._friends == null)
                        return;

                    if (!Client.GetHabbo().GetMessenger()._friends.ContainsKey(addid))
                        Client.GetHabbo().GetMessenger()._friends.Add(addid, newfriend);

                    if (!BotFriendShips.ContainsKey(Botfriend.Id))
                        BotFriendShips.TryAdd(Botfriend.Id, Botfriend);

                    Client.SendMessage(Client.GetHabbo().GetMessenger().SerializeUpdate(newfriend));
                }
            }
        }

        /// <summary>
        /// Loads and returns special cooldowns
        /// </summary>
        /// <param name="Row"></param>
        /// <returns></returns>
        internal ConcurrentDictionary<string, int> LoadAndReturnCooldowns(DataRow Row)
        {
            ConcurrentDictionary<string, int> specialCooldowns = new ConcurrentDictionary<string, int>();

            int RobberyCooldown = Convert.ToInt32(Row["robbery"]);
            specialCooldowns.TryAdd("robbery", RobberyCooldown);

            foreach (var cooldown in specialCooldowns)
            {
                if (cooldown.Value > 0)
                {
                    this.CooldownManager.CreateCooldown(cooldown.Key, 1000, cooldown.Value);
                }
            }

            return specialCooldowns;
        }

        /// <summary>
        /// Ends all timers/cooldowns and removes all offers
        /// </summary>
        public void EndCycle()
        {
            if (TimerManager != null)
            {
                TimerManager.EndAllTimers();
                TimerManager = null;
            }
            if (OfferManager != null)
            {
                OfferManager.EndAllOffers();
                OfferManager = null;
            }
            if (CooldownManager != null)
            {
                CooldownManager.EndAllCooldowns();
                CooldownManager = null;
            }
        }

        /// <summary>
        /// Sets stats to max
        /// </summary>
        public void ReplenishStats(bool Bool = false)
        {
            
            if (CurHealth < MaxHealth)
                CurHealth = MaxHealth;

            if (Hunger > 0)
                Hunger = 0;

            if (Hygiene < 100)
                Hygiene = 100;

            if (!Bool)
            {
                if (CurEnergy < MaxEnergy)
                    CurEnergy = MaxEnergy;
            }
        }

        /// <summary>
        /// Opens statistic dialogue for target user
        /// </summary>
        /// <param name="Target">User targetting</param>
        public void OpenUsersDialogue(GameClient Target)
        {
            if (Target == null)
                return;

            if (Target != Client)
            {
                if (UserViewing != Target.GetHabbo().Id)
                    UserViewing = Target.GetHabbo().Id;
            }

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_characterbar", "" + Target.GetHabbo().Id);
        }

        /// <summary>
        /// Closes the statistic dialogue of anybody viewing the users one
        /// </summary>
        public void CloseInteractingUserDialogues()
        {
            foreach (GameClient iClient in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (iClient == null)
                    continue;

                if (iClient.GetRoleplay() == null)
                    continue;

                if (iClient.GetRoleplay().UserViewing != Client.GetHabbo().Id)
                    continue;

                if (iClient.LoggingOut)
                    continue;

                if (iClient.GetRoleplay().WebSocketConnection == null)
                    continue;

                iClient.GetRoleplay().ClearWebSocketDialogue();
            }
        }

        /// <summary>
        /// Refreshes statistic dialogue of anybody viewing users one
        /// </summary>
        public void UpdateInteractingUserDialogues()
        {
            foreach (GameClient iClient in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (iClient == null)
                    continue;

                if (iClient.GetRoleplay() == null)
                    continue;

                if (iClient.GetRoleplay().UserViewing != Client.GetHabbo().Id)
                    continue;

                if (iClient.LoggingOut)
                    continue;

                if (iClient.GetRoleplay().WebSocketConnection == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(iClient, "event_characterbar", "" + Client.GetHabbo().Id);
            }
        }

        /// <summary>
        /// Refreshes users statistic dialogue
        /// </summary>
        public void RefreshStatDialogue()
        {
            if (WebSocketConnection == null)
                return;

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_retrieveconnectingstatistics", "");
        }

        /// <summary>
        /// Clear websocket dialogues
        /// </summary>
        public void ClearWebSocketDialogue(bool Force = false)
        {
            UserViewing = 0;
            GetUserComponent.ClearStatisticsDialogue(Client);
        }

        public void UpdateTimerDialogue(string timer, string action, int val1, int val2)
        {
            if (WebSocketConnection != null)
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_timerdialogue", "action:" + action + ",timer:" + timer + ",value:" + val1 + "/" + val2 + ",bypass:true");
        }

        /// <summary>
        /// Opens the CAPTCHA box with a generated random string to the user
        /// </summary>
        /// <param name="Title"></param>
        public void CreateCaptcha(string Title)
        {
            if (this.WebSocketConnection == null)
                return;

            string Action = "create";
            string CaptchaTitle = Title;

            Random Random = new Random();
            const string AvailableCharacters = "ABCDEFGHJKMNOPQRSTUVWXYZ0123456789";
            string GeneratedString = new string(Enumerable.Repeat(AvailableCharacters, 6).Select(s => s[Random.Next(s.Length)]).ToArray());

            string Data =  Action + "," + CaptchaTitle + "," + GeneratedString;

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_captcha", Data);

            if (this.Client != null)
                this.Client.SendWhisper(Title, 1);
        }

        /// <summary>
        /// Returns the cooldown time if cooldown exists
        /// </summary>
        public bool TryGetCooldown(string cooldown, bool sendwhisper = true)
        {
            if (this.CooldownManager != null && this.CooldownManager.ActiveCooldowns != null)
            {
                if (this.CooldownManager.ActiveCooldowns.ContainsKey(cooldown.ToLower()))
                {
                    var CoolDown = this.CooldownManager.ActiveCooldowns[cooldown.ToLower()];

                    if (CoolDown == null)
                        return false;

                    if (this.Client.GetHabbo().VIPRank == 2 && CoolDown.Type.ToLower() != "fist" && CoolDown.Type.ToLower() != "reload")
                        return false;

                    if (sendwhisper)
                        this.Client.SendWhisper("Você tem que esperar [" + (CoolDown.TimeLeft / 1000) + "/" + CoolDown.Amount + "]!", 1);
                    return true;
                }
            }
            return false;
        }

        public void UpdateEventWins(string Event, int IncrementalValue)
        {
            switch (Event)
            {
                case "brawl":
				
                    BrawlWins += IncrementalValue;
                    break;

                case "cw":
                case "colorwars":
				case "guerradecores":
				case "guerracores":
				case "gc":
				case "guerrac":
                    CwWins += IncrementalValue;
                    break;

                case "soloqueue":
                case "sq":
				case "solo":
				case "qsolo":
                    SoloQueueWins += IncrementalValue;
                    break;

                case "mw":
                case "mafia":
                case "mafiawars":
				case "guerrademafias":
				case "guerrademafia":
				case "guerramafia":
				case "gm":
                    MwWins += IncrementalValue;
                    break;
            }
        }

        public void SendTopAlert(string Message)
        {
            if (this.WebSocketConnection == null)
            {
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.Client, JsonConvert.SerializeObject(new Dictionary<object, object>()
             {
                { "event", "chatManager" },
                { "chatname", "getchats" },
                { "action", "newnotifyuser" },
                { "chatmessage", Message }
             }));
        }

        #endregion
    }
}