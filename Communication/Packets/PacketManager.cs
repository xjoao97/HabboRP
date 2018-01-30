using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using log4net;

using Plus.Core;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;

using Plus.Communication.Packets.Incoming.Catalog;
using Plus.Communication.Packets.Incoming.Handshake;
using Plus.Communication.Packets.Incoming.Navigator;
using Plus.Communication.Packets.Incoming.Quests;
using Plus.Communication.Packets.Incoming.Rooms.Avatar;
using Plus.Communication.Packets.Incoming.Rooms.Chat;
using Plus.Communication.Packets.Incoming.Rooms.Connection;
using Plus.Communication.Packets.Incoming.Rooms.Engine;
using Plus.Communication.Packets.Incoming.Rooms.Action;
using Plus.Communication.Packets.Incoming.Users;
using Plus.Communication.Packets.Incoming.Inventory.AvatarEffects;
using Plus.Communication.Packets.Incoming.Inventory.Purse;
using Plus.Communication.Packets.Incoming.Sound;
using Plus.Communication.Packets.Incoming.Misc;
using Plus.Communication.Packets.Incoming.Inventory.Badges;
using Plus.Communication.Packets.Incoming.Avatar;
using Plus.Communication.Packets.Incoming.Inventory.Achievements;
using Plus.Communication.Packets.Incoming.Inventory.Bots;
using Plus.Communication.Packets.Incoming.Inventory.Pets;
using Plus.Communication.Packets.Incoming.LandingView;
using Plus.Communication.Packets.Incoming.Messenger;
using Plus.Communication.Packets.Incoming.Groups;
using Plus.Communication.Packets.Incoming.Rooms.Settings;
using Plus.Communication.Packets.Incoming.Rooms.AI.Pets;
using Plus.Communication.Packets.Incoming.Rooms.AI.Bots;
using Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse;
using Plus.Communication.Packets.Incoming.Rooms.Furni;
using Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces;
using Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions;
using Plus.Communication.Packets.Incoming.Rooms.Furni.Crafting;
using Plus.Communication.Packets.Incoming.Help;
using Plus.Communication.Packets.Incoming.Rooms.FloorPlan;
using Plus.Communication.Packets.Incoming.Rooms.Furni.Wired;
using Plus.Communication.Packets.Incoming.Moderation;
using Plus.Communication.Packets.Incoming.Inventory.Furni;
using Plus.Communication.Packets.Incoming.Rooms.Furni.Stickys;
using Plus.Communication.Packets.Incoming.Rooms.Furni.Moodlight;
using Plus.Communication.Packets.Incoming.Inventory.Trading;
using Plus.Communication.Packets.Incoming.GameCenter;
using Plus.Communication.Packets.Incoming.Marketplace;
using Plus.Communication.Packets.Incoming.Rooms.Furni.LoveLocks;
using Plus.Communication.Packets.Incoming.Talents;
using Plus.Communication.Packets.Incoming.Guides;
using Plus.Communication.Packets.Incoming.Polls;
using Plus.Communication.Packets.Incoming.Camera;

namespace Plus.Communication.Packets
{
    public sealed class PacketManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.Communication.Packets");

        /// <summary>
        ///     Testing the Task code
        /// </summary>
        private readonly bool IgnoreTasks = true;

        /// <summary>
        ///     The maximum time a task can run for before it is considered dead
        ///     (can be used for debugging any locking issues with certain areas of code)
        /// </summary>
        private readonly int MaximumRunTimeInSec = 300; // 5 minutes

        /// <summary>
        ///     Should the handler throw errors or log and continue.
        /// </summary>
        private readonly bool ThrowUserErrors = false;

        /// <summary>
        ///     The task factory which is used for running Asynchronous tasks, in this case we use it to execute packets.
        /// </summary>
        private readonly TaskFactory _eventDispatcher;

        private readonly ConcurrentDictionary<int, IPacketEvent> _incomingPackets;
        private readonly ConcurrentDictionary<int, string> _packetNames;

        /// <summary>
        ///     Currently running tasks to keep track of what the current load is
        /// </summary>
        private readonly ConcurrentDictionary<int, Task> _runningTasks;

        public PacketManager()
        {
            this._incomingPackets = new ConcurrentDictionary<int, IPacketEvent>();

            this._eventDispatcher = new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.None);
            this._runningTasks = new ConcurrentDictionary<int, Task>();
            this._packetNames = new ConcurrentDictionary<int, string>();

            this.RegisterHandshake();
            this.RegisterLandingView();
            this.RegisterCatalog();
            this.RegisterMarketplace();
            this.RegisterNavigator();
            this.RegisterNewNavigator();
            this.RegisterRoomAction();
            this.RegisterQuests();
            this.RegisterRoomConnection();
            this.RegisterRoomChat();
            this.RegisterRoomEngine();
            this.RegisterFurni();
            this.RegisterUsers();
            this.RegisterSound();
            this.RegisterMisc();
            this.RegisterInventory();
            this.RegisterTalents();
            this.RegisterPurse();
            this.RegisterRoomAvatar();
            this.RegisterAvatar();
            this.RegisterMessenger();
            this.RegisterGroups();
            this.RegisterRoomSettings();
            this.RegisterPets();
            this.RegisterBots();
            this.RegisterHelp();
            this.FloorPlanEditor();
            this.RegisterModeration();
            this.RegisterGameCenter();
            this.RegisterNames();
        }

        public void TryExecutePacket(GameClient Session, ClientPacket Packet)
        {
            IPacketEvent Pak = null;

            if (!_incomingPackets.TryGetValue(Packet.Id, out Pak))
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    log.Debug("Pacote não tratado: " + Packet.ToString());
                return;
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                if (_packetNames.ContainsKey(Packet.Id))
                    log.Debug("Pacote tratado: [" + Packet.Id + "] " + _packetNames[Packet.Id]);
                else
                    log.Debug("Pacote tratado: [" + Packet.Id + "] UnnamedPacketEvent");
            }

            if (!IgnoreTasks)
                ExecutePacketAsync(Session, Packet, Pak);
            else
                Pak.Parse(Session, Packet);
        }

        private void ExecutePacketAsync(GameClient Session, ClientPacket Packet, IPacketEvent Pak)
        {
            DateTime Start = DateTime.Now;

            var CancelSource = new CancellationTokenSource();
            CancellationToken Token = CancelSource.Token;

            Task t = _eventDispatcher.StartNew(() =>
            {
                Pak.Parse(Session, Packet);
                Token.ThrowIfCancellationRequested();
            }, Token);

            _runningTasks.TryAdd(t.Id, t);

            try
            {
                if (!t.Wait(MaximumRunTimeInSec * 1000, Token))
                {
                    CancelSource.Cancel();
                }
            }
            catch (AggregateException ex)
            {
                foreach (Exception e in ex.Flatten().InnerExceptions)
                {
                    if (ThrowUserErrors)
                    {
                        throw e;
                    }
                    else
                    {
                        //log.Fatal("Unhandled Error: " + e.Message + " - " + e.StackTrace);
                        Session.Disconnect(true);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Session.Disconnect(true);
            }
            finally
            {
                Task RemovedTask = null;
                _runningTasks.TryRemove(t.Id, out RemovedTask);

                CancelSource.Dispose();

                //log.Debug("Event took " + (DateTime.Now - Start).Milliseconds + "ms to complete.");
            }
        }

        public void WaitForAllToComplete()
        {
            foreach (Task t in this._runningTasks.Values.ToList())
            {
                t.Wait();
            }
        }

        public void UnregisterAll()
        {
            this._incomingPackets.Clear();
        }

        private void RegisterHandshake()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetClientVersionMessageEvent, new GetClientVersionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.InitCryptoMessageEvent, new InitCryptoEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GenerateSecretKeyMessageEvent, new GenerateSecretKeyEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UniqueIDMessageEvent, new UniqueIDEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SSOTicketMessageEvent, new SSOTicketEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.InfoRetrieveMessageEvent, new InfoRetrieveEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PingMessageEvent, new PingEvent());
        }

        private void RegisterLandingView()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.RefreshCampaignMessageEvent, new RefreshCampaignEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetPromoArticlesMessageEvent, new GetPromoArticlesEvent());
        }

        private void RegisterCatalog()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCatalogModeMessageEvent, new GetCatalogModeEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCatalogIndexMessageEvent, new GetCatalogIndexEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCatalogPageMessageEvent, new GetCatalogPageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCatalogOfferMessageEvent, new GetCatalogOfferEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PurchaseFromCatalogMessageEvent, new PurchaseFromCatalogEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PurchaseFromCatalogAsGiftMessageEvent, new PurchaseFromCatalogAsGiftEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PurchaseRoomPromotionMessageEvent, new PurchaseRoomPromotionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGiftWrappingConfigurationMessageEvent, new GetGiftWrappingConfigurationEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetMarketplaceConfigurationMessageEvent, new GetMarketplaceConfigurationEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRecyclerRewardsMessageEvent, new GetRecyclerRewardsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CheckPetNameMessageEvent, new CheckPetNameEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RedeemVoucherMessageEvent, new RedeemVoucherEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetSellablePetBreedsMessageEvent, new GetSellablePetBreedsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetPromotableRoomsMessageEvent, new GetPromotableRoomsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCatalogRoomPromotionMessageEvent, new GetCatalogRoomPromotionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGroupFurniConfigMessageEvent, new GetGroupFurniConfigEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CheckGnomeNameMessageEvent, new CheckGnomeNameEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetClubGiftsMessageEvent, new GetClubGiftsEvent());

        }

        private void RegisterMarketplace()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetOffersMessageEvent, new GetOffersEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetOwnOffersMessageEvent, new GetOwnOffersEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetMarketplaceCanMakeOfferMessageEvent, new GetMarketplaceCanMakeOfferEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetMarketplaceItemStatsMessageEvent, new GetMarketplaceItemStatsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.MakeOfferMessageEvent, new MakeOfferEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CancelOfferMessageEvent, new CancelOfferEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.BuyOfferMessageEvent, new BuyOfferEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RedeemOfferCreditsMessageEvent, new RedeemOfferCreditsEvent());
        }

        private void RegisterNavigator()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.AddFavouriteRoomMessageEvent, new AddFavouriteRoomEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetUserFlatCatsMessageEvent, new GetUserFlatCatsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DeleteFavouriteRoomMessageEvent, new RemoveFavouriteRoomEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GoToHotelViewMessageEvent, new GoToHotelViewEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateNavigatorSettingsMessageEvent, new UpdateNavigatorSettingsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CanCreateRoomMessageEvent, new CanCreateRoomEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CreateFlatMessageEvent, new CreateFlatEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGuestRoomMessageEvent, new GetGuestRoomEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.EditRoomPromotionMessageEvent, new EditRoomEventEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetEventCategoriesMessageEvent, new GetNavigatorFlatsEvent());
        }

        public void RegisterNewNavigator()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.InitializeNewNavigatorMessageEvent, new InitializeNewNavigatorEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.NewNavigatorSearchMessageEvent, new NewNavigatorSearchEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.FindRandomFriendingRoomMessageEvent, new FindRandomFriendingRoomEvent());
        }

        private void RegisterQuests()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetQuestListMessageEvent, new GetQuestListEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.StartQuestMessageEvent, new StartQuestEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CancelQuestMessageEvent, new CancelQuestEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCurrentQuestMessageEvent, new GetCurrentQuestEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetDailyQuestMessageEvent, new GetDailyQuestEvent());
        }

        private void RegisterHelp()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.OnBullyClickMessageEvent, new OnBullyClickEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SendBullyReportMessageEvent, new SendBullyReportEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SubmitBullyReportMessageEvent, new SubmitBullyReportEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetSanctionStatusMessageEvent, new GetSanctionStatusEvent());
        }

        private void RegisterRoomAction()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.LetUserInMessageEvent, new LetUserInEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.BanUserMessageEvent, new BanUserEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.KickUserMessageEvent, new KickUserEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AssignRightsMessageEvent, new AssignRightsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RemoveRightsMessageEvent, new RemoveRightsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RemoveAllRightsMessageEvent, new RemoveAllRightsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.MuteUserMessageEvent, new MuteUserEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GiveHandItemMessageEvent, new GiveHandItemEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RemoveMyRightsMessageEvent, new RemoveMyRightsEvent());
        }

        private void RegisterAvatar()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetWardrobeMessageEvent, new GetWardrobeEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveWardrobeOutfitMessageEvent, new SaveWardrobeOutfitEvent());
        }

        private void RegisterRoomAvatar()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.ActionMessageEvent, new ActionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ApplySignMessageEvent, new ApplySignEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DanceMessageEvent, new DanceEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SitMessageEvent, new SitEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ChangeMottoMessageEvent, new ChangeMottoEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.LookToMessageEvent, new LookToEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DropHandItemMessageEvent, new DropHandItemEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GiveRoomScoreMessageEvent, new GiveRoomScoreEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.IgnoreUserMessageEvent, new IgnoreUserEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UnIgnoreUserMessageEvent, new UnIgnoreUserEvent());
        }

        private void RegisterRoomConnection()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.OpenFlatConnectionMessageEvent, new OpenFlatConnectionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GoToFlatMessageEvent, new GoToFlatEvent());
        }

        private void RegisterRoomChat()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.ChatMessageEvent, new ChatEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ShoutMessageEvent, new ShoutEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.WhisperMessageEvent, new WhisperEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.StartTypingMessageEvent, new StartTypingEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CancelTypingMessageEvent, new CancelTypingEvent());
        }

        private void RegisterRoomEngine()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRoomEntryDataMessageEvent, new GetRoomEntryDataEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetFurnitureAliasesMessageEvent, new GetFurnitureAliasesEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.MoveAvatarMessageEvent, new MoveAvatarEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.MoveObjectMessageEvent, new MoveObjectEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PickupObjectMessageEvent, new PickupObjectEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.MoveWallItemMessageEvent, new MoveWallItemEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ApplyDecorationMessageEvent, new ApplyDecorationEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PlaceObjectMessageEvent, new PlaceObjectEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UseFurnitureMessageEvent, new UseFurnitureEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UseWallItemMessageEvent, new UseWallItemEvent());
        }

        private void RegisterInventory()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.InitTradeMessageEvent, new InitTradeEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TradingOfferItemMessageEvent, new TradingOfferItemEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TradingOfferItemsMessageEvent, new TradingOfferItemsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TradingRemoveItemMessageEvent, new TradingRemoveItemEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TradingAcceptMessageEvent, new TradingAcceptEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TradingCancelMessageEvent, new TradingCancelEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TradingConfirmMessageEvent, new TradingConfirmEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TradingModifyMessageEvent, new TradingModifyEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TradingCancelConfirmMessageEvent, new TradingCancelConfirmEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RequestFurniInventoryMessageEvent, new RequestFurniInventoryEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetBadgesMessageEvent, new GetBadgesEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetAchievementsMessageEvent, new GetAchievementsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetActivatedBadgesMessageEvent, new SetActivatedBadgesEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetBotInventoryMessageEvent, new GetBotInventoryEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetPetInventoryMessageEvent, new GetPetInventoryEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AvatarEffectActivatedMessageEvent, new AvatarEffectActivatedEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AvatarEffectSelectedMessageEvent, new AvatarEffectSelectedEvent());
        }

        private void RegisterTalents()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetTalentTrackMessageEvent, new GetTalentTrackEvent());
        }

        private void RegisterPurse()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCreditsInfoMessageEvent, new GetCreditsInfoEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetClubOffersMessageEvent, new GetHabboClubWindowEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetHabboClubCenterInfoMessageEvent, new GetHabboClubTimeEvent());
        }

        private void RegisterUsers()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.ScrGetUserInfoMessageEvent, new ScrGetUserInfoEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetChatPreferenceMessageEvent, new SetChatPreferenceEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetUserFocusPreferenceEvent, new SetUserFocusPreferenceEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetMessengerInviteStatusMessageEvent, new SetMessengerInviteStatusEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RespectUserMessageEvent, new RespectUserEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateFigureDataMessageEvent, new UpdateFigureDataEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.OpenPlayerProfileMessageEvent, new OpenPlayerProfileEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetSelectedBadgesMessageEvent, new GetSelectedBadgesEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRelationshipsMessageEvent, new GetRelationshipsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetRelationshipMessageEvent, new SetRelationshipEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CheckValidNameMessageEvent, new CheckValidNameEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ChangeNameMessageEvent, new ChangeNameEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetUsernameMessageEvent, new SetUsernameEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetHabboGroupBadgesMessageEvent, new GetHabboGroupBadgesEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetUserTagsMessageEvent, new GetUserTagsEvent());
        }

        private void RegisterSound()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.SetSoundSettingsMessageEvent, new SetSoundSettingsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetSongInfoMessageEvent, new GetSongInfoEvent());
        }

        private void RegisterMisc()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.EventTrackerMessageEvent, new EventTrackerEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ClientVariablesMessageEvent, new ClientVariablesEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DisconnectionMessageEvent, new DisconnectEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.LatencyTestMessageEvent, new LatencyTestEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.MemoryPerformanceMessageEvent, new MemoryPerformanceEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetFriendBarStateMessageEvent, new SetFriendBarStateEvent());

            this._incomingPackets.TryAdd(ClientPacketHeader.HabboCameraMessageEvent, new HabboCameraMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCameraRequest, new GetCameraRequest());
            //this._incomingPackets.TryAdd(ClientPacketHeader.HabboCameraPublishPhoto, new HabboCameraPublishPhoto());
            //this._incomingPackets.TryAdd(ClientPacketHeader.GetCameraPriceMessageEvent, new GetCameraPriceMessageEvent());
            //this._incomingPackets.TryAdd(ClientPacketHeader.SaveRoomThumbnailMessageEvent, new SaveRoomThumbnailMessageEvent());

            this._incomingPackets.TryAdd(ClientPacketHeader.GetCraftingListMessageEvent, new GetCraftingListMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetCraftingRecipesAvailableMessageEvent, new GetCraftingRecipesAvailableMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CraftSecretMessageEvent, new CraftSecretMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRecipeConfigMessageEvent, new GetRecipeConfigMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CraftedRecipeExecutedMessageEvent, new CraftedRecipeExecutedMessageEvent());
        }


        private void RegisterMessenger()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.MessengerInitMessageEvent, new MessengerInitEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetBuddyRequestsMessageEvent, new GetBuddyRequestsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.FollowFriendMessageEvent, new FollowFriendEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.FindNewFriendsMessageEvent, new FindNewFriendsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.FriendListUpdateMessageEvent, new FriendListUpdateEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RemoveBuddyMessageEvent, new RemoveBuddyEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RequestBuddyMessageEvent, new RequestBuddyEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SendMsgMessageEvent, new SendMsgEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SendRoomInviteMessageEvent, new SendRoomInviteEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.HabboSearchMessageEvent, new HabboSearchEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AcceptBuddyMessageEvent, new AcceptBuddyEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DeclineBuddyMessageEvent, new DeclineBuddyEvent());
        }

        private void RegisterGroups()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.JoinGroupMessageEvent, new JoinGroupEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RemoveGroupFavouriteMessageEvent, new RemoveGroupFavouriteEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetGroupFavouriteMessageEvent, new SetGroupFavouriteEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGroupInfoMessageEvent, new GetGroupInfoEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGroupMembersMessageEvent, new GetGroupMembersEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGroupCreationWindowMessageEvent, new GetGroupCreationWindowEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetBadgeEditorPartsMessageEvent, new GetBadgeEditorPartsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PurchaseGroupMessageEvent, new PurchaseGroupEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateGroupIdentityMessageEvent, new UpdateGroupIdentityEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateGroupBadgeMessageEvent, new UpdateGroupBadgeEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateGroupColoursMessageEvent, new UpdateGroupColoursEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateGroupSettingsMessageEvent, new UpdateGroupSettingsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ManageGroupMessageEvent, new ManageGroupEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GiveAdminRightsMessageEvent, new GiveAdminRightsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.TakeAdminRightsMessageEvent, new TakeAdminRightsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RemoveGroupMemberMessageEvent, new RemoveGroupMemberEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AcceptGroupMembershipMessageEvent, new AcceptGroupMembershipEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DeclineGroupMembershipMessageEvent, new DeclineGroupMembershipEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DeleteGroupMessageEvent, new DeleteGroupEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGroupForumsMessageEvent, new GetGroupForumsMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGroupForumDataMessageEvent, new GetGroupForumDataMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGroupForumThreadRootMessageEvent, new GetGroupForumThreadRootMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateThreadMessageEvent, new UpdateThreadMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateForumSettingsMessageEvent, new UpdateForumSettingsMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AlterForumThreadStateMessageEvent, new AlterForumThreadStateMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PublishForumThreadMessageEvent, new PublishForumThreadMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ReadForumThreadMessageEvent, new ReadForumThreadMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DeleteGroupPostMessageEvent, new DeleteGroupPostMessageEvent());
        }

        private void RegisterRoomSettings()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRoomSettingsMessageEvent, new GetRoomSettingsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveRoomSettingsMessageEvent, new SaveRoomSettingsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DeleteRoomMessageEvent, new DeleteRoomEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ToggleMuteToolMessageEvent, new ToggleMuteToolEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRoomFilterListMessageEvent, new GetRoomFilterListEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModifyRoomFilterListMessageEvent, new ModifyRoomFilterListEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRoomRightsMessageEvent, new GetRoomRightsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRoomBannedUsersMessageEvent, new GetRoomBannedUsersEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UnbanUserFromRoomMessageEvent, new UnbanUserFromRoomEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveEnforcedCategorySettingsMessageEvent, new SaveEnforcedCategorySettingsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AcceptPollMessageEvent, new AcceptPollMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RefusePollMessageEvent, new RefusePollMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AnswerPollQuestionMessageEvent, new AnswerPollQuestionMessageEvent());
        }

        private void RegisterPets()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.RespectPetMessageEvent, new RespectPetEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetPetInformationMessageEvent, new GetPetInformationEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PickUpPetMessageEvent, new PickUpPetEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PlacePetMessageEvent, new PlacePetEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RideHorseMessageEvent, new RideHorseEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ApplyHorseEffectMessageEvent, new ApplyHorseEffectEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.RemoveSaddleFromHorseMessageEvent, new RemoveSaddleFromHorseEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModifyWhoCanRideHorseMessageEvent, new ModifyWhoCanRideHorseEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetPetTrainingPanelMessageEvent, new GetPetTrainingPanelEvent());
        }

        private void RegisterBots()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.PlaceBotMessageEvent, new PlaceBotEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PickUpBotMessageEvent, new PickUpBotEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.OpenBotActionMessageEvent, new OpenBotActionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveBotActionMessageEvent, new SaveBotActionEvent());
        }

        private void RegisterFurni()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateMagicTileMessageEvent, new UpdateMagicTileEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetYouTubeTelevisionMessageEvent, new GetYouTubeTelevisionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetRentableSpaceMessageEvent, new GetRentableSpaceEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PurchaseRentableSpaceMessageEvent, new PurchaseRentableSpaceEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CancelRentableSpaceMessageEvent, new CancelRentableSpaceEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ToggleYouTubeVideoMessageEvent, new ToggleYouTubeVideoEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.YouTubeVideoInformationMessageEvent, new YouTubeVideoInformationEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.YouTubeGetNextVideo, new YouTubeGetNextVideo());
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveWiredTriggerConfigMessageEvent, new SaveWiredConfigEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveWiredEffectConfigMessageEvent, new SaveWiredConfigEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveWiredConditionConfigMessageEvent, new SaveWiredConfigEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveBrandingItemMessageEvent, new SaveBrandingItemEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetTonerMessageEvent, new SetTonerEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DiceOffMessageEvent, new DiceOffEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ThrowDiceMessageEvent, new ThrowDiceEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetMannequinNameMessageEvent, new SetMannequinNameEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SetMannequinFigureMessageEvent, new SetMannequinFigureEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CreditFurniRedeemMessageEvent, new CreditFurniRedeemEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetStickyNoteMessageEvent, new GetStickyNoteEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AddStickyNoteMessageEvent, new AddStickyNoteEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UpdateStickyNoteMessageEvent, new UpdateStickyNoteEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.DeleteStickyNoteMessageEvent, new DeleteStickyNoteEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetMoodlightConfigMessageEvent, new GetMoodlightConfigEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.MoodlightUpdateMessageEvent, new MoodlightUpdateEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ToggleMoodlightMessageEvent, new ToggleMoodlightEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UseOneWayGateMessageEvent, new UseFurnitureEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UseHabboWheelMessageEvent, new UseFurnitureEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.OpenGiftMessageEvent, new OpenGiftEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGroupFurniSettingsMessageEvent, new GetGroupFurniSettingsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.UseSellableClothingMessageEvent, new UseSellableClothingEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ConfirmLoveLockMessageEvent, new ConfirmLoveLockEvent());
        }

        private void FloorPlanEditor()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.SaveFloorPlanModelMessageEvent, new SaveFloorPlanModelEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.InitializeFloorPlanSessionMessageEvent, new InitializeFloorPlanSessionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.FloorPlanEditorRoomPropertiesMessageEvent, new FloorPlanEditorRoomPropertiesEvent());
        }

        private void RegisterModeration()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.SendHelpTicketMessageEvent, new SendHelpTicketEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.OpenHelpToolMessageEvent, new OpenHelpToolEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetModeratorRoomInfoMessageEvent, new GetModeratorRoomInfoEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetModeratorUserInfoMessageEvent, new GetModeratorUserInfoEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetModeratorUserRoomVisitsMessageEvent, new GetModeratorUserRoomVisitsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModerateRoomMessageEvent, new ModerateRoomEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModeratorActionMessageEvent, new ModeratorActionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.SubmitNewTicketMessageEvent, new SubmitNewTicketEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetModeratorRoomChatlogMessageEvent, new GetModeratorRoomChatlogEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetModeratorUserChatlogMessageEvent, new GetModeratorUserChatlogEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetModeratorTicketChatlogsMessageEvent, new GetModeratorTicketChatlogsEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.PickTicketMessageEvent, new PickTicketEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ReleaseTicketMessageEvent, new ReleaseTicketEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.CloseTicketMesageEvent, new CloseTicketEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModerationMuteMessageEvent, new ModerationMuteEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModerationKickMessageEvent, new ModerationKickEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModerationBanMessageEvent, new ModerationBanEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModerationMsgMessageEvent, new ModerationMsgEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModerationCautionMessageEvent, new ModerationCautionEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.ModerationTradeLockMessageEvent, new ModerationTradeLockEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetHelperToolConfigurationMessageEvent, new GetHelperToolConfigurationMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.OnGuideSessionDetachedMessageEvent, new OnGuideSessionDetachedMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GuideToolMessageNew, new GuideToolMessageNew());
            this._incomingPackets.TryAdd(ClientPacketHeader.GuideInviteToRoom, new GuideInviteToRoom());
            this._incomingPackets.TryAdd(ClientPacketHeader.VisitRoomGuides, new VisitRoomGuides());
            this._incomingPackets.TryAdd(ClientPacketHeader.GuideEndSession, new GuideEndSession());
            this._incomingPackets.TryAdd(ClientPacketHeader.OnGuideSessionTyping, new OnGuideSessionTyping());
            //this._incomingPackets.TryAdd(ClientPacketHeader.CancellInviteGuide, new CancellInviteGuide());
            this._incomingPackets.TryAdd(ClientPacketHeader.OnGuideMessageEvent, new OnGuideMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.OnGuideFeedbackMessageEvent, new OnGuideFeedbackMessageEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.AmbassadorWarningMessageEvent, new AmbassadorWarningEvent());
        }

        public void RegisterGameCenter()
        {
            this._incomingPackets.TryAdd(ClientPacketHeader.GetGameListingMessageEvent, new GetGameListingEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.InitializeGameCenterMessageEvent, new InitializeGameCenterEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.GetPlayableGamesMessageEvent, new GetPlayableGamesEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.JoinPlayerQueueMessageEvent, new JoinPlayerQueueEvent());
            this._incomingPackets.TryAdd(ClientPacketHeader.Game2GetWeeklyLeaderboardMessageEvent, new Game2GetWeeklyLeaderboardEvent());
        }

        public void RegisterNames()
        {
            this._packetNames.TryAdd(ClientPacketHeader.GetClientVersionMessageEvent, "GetClientVersionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.InitCryptoMessageEvent, "InitCryptoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GenerateSecretKeyMessageEvent, "GenerateSecretKeyEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UniqueIDMessageEvent, "UniqueIDEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SSOTicketMessageEvent, "SSOTicketEvent");
            this._packetNames.TryAdd(ClientPacketHeader.InfoRetrieveMessageEvent, "InfoRetrieveEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PingMessageEvent, "PingEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RefreshCampaignMessageEvent, "RefreshCampaignEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetPromoArticlesMessageEvent, "RefreshPromoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCatalogModeMessageEvent, "GetCatalogModeEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCatalogIndexMessageEvent, "GetCatalogIndexEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCatalogPageMessageEvent, "GetCatalogPageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCatalogOfferMessageEvent, "GetCatalogOfferEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PurchaseFromCatalogMessageEvent, "PurchaseFromCatalogEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PurchaseFromCatalogAsGiftMessageEvent, "PurchaseFromCatalogAsGiftEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PurchaseRoomPromotionMessageEvent, "PurchaseRoomPromotionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGiftWrappingConfigurationMessageEvent, "GetGiftWrappingConfigurationEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetMarketplaceConfigurationMessageEvent, "GetMarketplaceConfigurationEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRecyclerRewardsMessageEvent, "GetRecyclerRewardsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CheckPetNameMessageEvent, "CheckPetNameEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RedeemVoucherMessageEvent, "RedeemVoucherEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetSellablePetBreedsMessageEvent, "GetSellablePetBreedsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetPromotableRoomsMessageEvent, "GetPromotableRoomsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCatalogRoomPromotionMessageEvent, "GetCatalogRoomPromotionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGroupFurniConfigMessageEvent, "GetGroupFurniConfigEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CheckGnomeNameMessageEvent, "CheckGnomeNameEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetOffersMessageEvent, "GetOffersEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetOwnOffersMessageEvent, "GetOwnOffersEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetMarketplaceCanMakeOfferMessageEvent, "GetMarketplaceCanMakeOfferEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetMarketplaceItemStatsMessageEvent, "GetMarketplaceItemStatsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.MakeOfferMessageEvent, "MakeOfferEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CancelOfferMessageEvent, "CancelOfferEvent");
            this._packetNames.TryAdd(ClientPacketHeader.BuyOfferMessageEvent, "BuyOfferEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RedeemOfferCreditsMessageEvent, "RedeemOfferCreditsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AddFavouriteRoomMessageEvent, "AddFavouriteRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetUserFlatCatsMessageEvent, "GetUserFlatCatsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DeleteFavouriteRoomMessageEvent, "RemoveFavouriteRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GoToHotelViewMessageEvent, "GoToHotelViewEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateNavigatorSettingsMessageEvent, "UpdateNavigatorSettingsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CanCreateRoomMessageEvent, "CanCreateRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CreateFlatMessageEvent, "CreateFlatEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGuestRoomMessageEvent, "GetGuestRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.EditRoomPromotionMessageEvent, "EditRoomEventEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetEventCategoriesMessageEvent, "GetNavigatorFlatsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.InitializeNewNavigatorMessageEvent, "InitializeNewNavigatorEvent");
            this._packetNames.TryAdd(ClientPacketHeader.NewNavigatorSearchMessageEvent, "NewNavigatorSearchEvent");
            this._packetNames.TryAdd(ClientPacketHeader.FindRandomFriendingRoomMessageEvent, "FindRandomFriendingRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetQuestListMessageEvent, "GetQuestListEvent");
            this._packetNames.TryAdd(ClientPacketHeader.StartQuestMessageEvent, "StartQuestEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CancelQuestMessageEvent, "CancelQuestEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCurrentQuestMessageEvent, "GetCurrentQuestEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OnBullyClickMessageEvent, "OnBullyClickEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SendBullyReportMessageEvent, "SendBullyReportEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SubmitBullyReportMessageEvent, "SubmitBullyReportEvent");
            this._packetNames.TryAdd(ClientPacketHeader.LetUserInMessageEvent, "LetUserInEvent");
            this._packetNames.TryAdd(ClientPacketHeader.BanUserMessageEvent, "BanUserEvent");
            this._packetNames.TryAdd(ClientPacketHeader.KickUserMessageEvent, "KickUserEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AssignRightsMessageEvent, "AssignRightsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RemoveRightsMessageEvent, "RemoveRightsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RemoveAllRightsMessageEvent, "RemoveAllRightsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.MuteUserMessageEvent, "MuteUserEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GiveHandItemMessageEvent, "GiveHandItemEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetWardrobeMessageEvent, "GetWardrobeEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SaveWardrobeOutfitMessageEvent, "SaveWardrobeOutfitEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ActionMessageEvent, "ActionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ApplySignMessageEvent, "ApplySignEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DanceMessageEvent, "DanceEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SitMessageEvent, "SitEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ChangeMottoMessageEvent, "ChangeMottoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.LookToMessageEvent, "LookToEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DropHandItemMessageEvent, "DropHandItemEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GiveRoomScoreMessageEvent, "GiveRoomScoreEvent");
            this._packetNames.TryAdd(ClientPacketHeader.IgnoreUserMessageEvent, "IgnoreUserEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UnIgnoreUserMessageEvent, "UnIgnoreUserEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OpenFlatConnectionMessageEvent, "OpenFlatConnectionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GoToFlatMessageEvent, "GoToFlatEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ChatMessageEvent, "ChatEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ShoutMessageEvent, "ShoutEvent");
            this._packetNames.TryAdd(ClientPacketHeader.WhisperMessageEvent, "WhisperEvent");
            this._packetNames.TryAdd(ClientPacketHeader.StartTypingMessageEvent, "StartTypingEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CancelTypingMessageEvent, "CancelTypingEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRoomEntryDataMessageEvent, "GetRoomEntryDataEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetFurnitureAliasesMessageEvent, "GetFurnitureAliasesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.MoveAvatarMessageEvent, "MoveAvatarEvent");
            this._packetNames.TryAdd(ClientPacketHeader.MoveObjectMessageEvent, "MoveObjectEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PickupObjectMessageEvent, "PickupObjectEvent");
            this._packetNames.TryAdd(ClientPacketHeader.MoveWallItemMessageEvent, "MoveWallItemEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ApplyDecorationMessageEvent, "ApplyDecorationEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PlaceObjectMessageEvent, "PlaceObjectEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UseFurnitureMessageEvent, "UseFurnitureEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UseWallItemMessageEvent, "UseWallItemEvent");
            this._packetNames.TryAdd(ClientPacketHeader.InitTradeMessageEvent, "InitTradeEvent");
            this._packetNames.TryAdd(ClientPacketHeader.TradingOfferItemMessageEvent, "TradingOfferItemEvent");
            this._packetNames.TryAdd(ClientPacketHeader.TradingRemoveItemMessageEvent, "TradingRemoveItemEvent");
            this._packetNames.TryAdd(ClientPacketHeader.TradingAcceptMessageEvent, "TradingAcceptEvent");
            this._packetNames.TryAdd(ClientPacketHeader.TradingCancelMessageEvent, "TradingCancelEvent");
            this._packetNames.TryAdd(ClientPacketHeader.TradingConfirmMessageEvent, "TradingConfirmEvent");
            this._packetNames.TryAdd(ClientPacketHeader.TradingModifyMessageEvent, "TradingModifyEvent");
            this._packetNames.TryAdd(ClientPacketHeader.TradingCancelConfirmMessageEvent, "TradingCancelConfirmEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RequestFurniInventoryMessageEvent, "RequestFurniInventoryEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetBadgesMessageEvent, "GetBadgesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetAchievementsMessageEvent, "GetAchievementsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetActivatedBadgesMessageEvent, "SetActivatedBadgesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetBotInventoryMessageEvent, "GetBotInventoryEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetPetInventoryMessageEvent, "GetPetInventoryEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AvatarEffectActivatedMessageEvent, "AvatarEffectActivatedEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AvatarEffectSelectedMessageEvent, "AvatarEffectSelectedEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetTalentTrackMessageEvent, "GetTalentTrackEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCreditsInfoMessageEvent, "GetCreditsInfoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetClubOffersMessageEvent, "GetHabboClubWindowEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetHabboClubCenterInfoMessageEvent, "GetHabboClubTimeEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ScrGetUserInfoMessageEvent, "ScrGetUserInfoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetChatPreferenceMessageEvent, "SetChatPreferenceEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetUserFocusPreferenceEvent, "SetUserFocusPreferenceEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetMessengerInviteStatusMessageEvent, "SetMessengerInviteStatusEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RespectUserMessageEvent, "RespectUserEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateFigureDataMessageEvent, "UpdateFigureDataEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OpenPlayerProfileMessageEvent, "OpenPlayerProfileEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetSelectedBadgesMessageEvent, "GetSelectedBadgesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRelationshipsMessageEvent, "GetRelationshipsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetRelationshipMessageEvent, "SetRelationshipEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CheckValidNameMessageEvent, "CheckValidNameEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ChangeNameMessageEvent, "ChangeNameEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetUsernameMessageEvent, "SetUsernameEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetHabboGroupBadgesMessageEvent, "GetHabboGroupBadgesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetUserTagsMessageEvent, "GetUserTagsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetSoundSettingsMessageEvent, "SetSoundSettingsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetSongInfoMessageEvent, "GetSongInfoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.EventTrackerMessageEvent, "EventTrackerEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ClientVariablesMessageEvent, "ClientVariablesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DisconnectionMessageEvent, "DisconnectEvent");
            this._packetNames.TryAdd(ClientPacketHeader.LatencyTestMessageEvent, "LatencyTestEvent");
            this._packetNames.TryAdd(ClientPacketHeader.MemoryPerformanceMessageEvent, "MemoryPerformanceEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetFriendBarStateMessageEvent, "SetFriendBarStateEvent");
            this._packetNames.TryAdd(ClientPacketHeader.MessengerInitMessageEvent, "MessengerInitEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetBuddyRequestsMessageEvent, "GetBuddyRequestsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.FollowFriendMessageEvent, "FollowFriendEvent");
            this._packetNames.TryAdd(ClientPacketHeader.FindNewFriendsMessageEvent, "FindNewFriendsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.FriendListUpdateMessageEvent, "FriendListUpdateEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RemoveBuddyMessageEvent, "RemoveBuddyEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RequestBuddyMessageEvent, "RequestBuddyEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SendMsgMessageEvent, "SendMsgEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SendRoomInviteMessageEvent, "SendRoomInviteEvent");
            this._packetNames.TryAdd(ClientPacketHeader.HabboSearchMessageEvent, "HabboSearchEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AcceptBuddyMessageEvent, "AcceptBuddyEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DeclineBuddyMessageEvent, "DeclineBuddyEvent");
            this._packetNames.TryAdd(ClientPacketHeader.JoinGroupMessageEvent, "JoinGroupEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RemoveGroupFavouriteMessageEvent, "RemoveGroupFavouriteEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetGroupFavouriteMessageEvent, "SetGroupFavouriteEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGroupInfoMessageEvent, "GetGroupInfoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGroupMembersMessageEvent, "GetGroupMembersEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGroupCreationWindowMessageEvent, "GetGroupCreationWindowEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetBadgeEditorPartsMessageEvent, "GetBadgeEditorPartsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PurchaseGroupMessageEvent, "PurchaseGroupEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateGroupIdentityMessageEvent, "UpdateGroupIdentityEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateGroupBadgeMessageEvent, "UpdateGroupBadgeEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateGroupColoursMessageEvent, "UpdateGroupColoursEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateGroupSettingsMessageEvent, "UpdateGroupSettingsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ManageGroupMessageEvent, "ManageGroupEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GiveAdminRightsMessageEvent, "GiveAdminRightsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.TakeAdminRightsMessageEvent, "TakeAdminRightsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RemoveGroupMemberMessageEvent, "RemoveGroupMemberEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AcceptGroupMembershipMessageEvent, "AcceptGroupMembershipEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DeclineGroupMembershipMessageEvent, "DeclineGroupMembershipEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DeleteGroupMessageEvent, "DeleteGroupEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGroupForumsMessageEvent, "GetGroupForumsMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGroupForumDataMessageEvent, "GetGroupForumDataMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGroupForumThreadRootMessageEvent, "GetGroupForumThreadRootMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateThreadMessageEvent, "UpdateThreadMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateForumSettingsMessageEvent, "UpdateForumSettingsMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AlterForumThreadStateMessageEvent, "AlterForumThreadStateMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PublishForumThreadMessageEvent, "PublishForumThreadMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ReadForumThreadMessageEvent, "ReadForumThreadMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DeleteGroupPostMessageEvent, "DeleteGroupPostMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRoomSettingsMessageEvent, "GetRoomSettingsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SaveRoomSettingsMessageEvent, "SaveRoomSettingsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DeleteRoomMessageEvent, "DeleteRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ToggleMuteToolMessageEvent, "ToggleMuteToolEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRoomFilterListMessageEvent, "GetRoomFilterListEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModifyRoomFilterListMessageEvent, "ModifyRoomFilterListEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRoomRightsMessageEvent, "GetRoomRightsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRoomBannedUsersMessageEvent, "GetRoomBannedUsersEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UnbanUserFromRoomMessageEvent, "UnbanUserFromRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SaveEnforcedCategorySettingsMessageEvent, "SaveEnforcedCategorySettingsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RespectPetMessageEvent, "RespectPetEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetPetInformationMessageEvent, "GetPetInformationEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PickUpPetMessageEvent, "PickUpPetEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PlacePetMessageEvent, "PlacePetEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RideHorseMessageEvent, "RideHorseEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ApplyHorseEffectMessageEvent, "ApplyHorseEffectEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RemoveSaddleFromHorseMessageEvent, "RemoveSaddleFromHorseEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModifyWhoCanRideHorseMessageEvent, "ModifyWhoCanRideHorseEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetPetTrainingPanelMessageEvent, "GetPetTrainingPanelEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PlaceBotMessageEvent, "PlaceBotEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PickUpBotMessageEvent, "PickUpBotEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OpenBotActionMessageEvent, "OpenBotActionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SaveBotActionMessageEvent, "SaveBotActionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateMagicTileMessageEvent, "UpdateMagicTileEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetYouTubeTelevisionMessageEvent, "GetYouTubeTelevisionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRentableSpaceMessageEvent, "GetRentableSpaceEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PurchaseRentableSpaceMessageEvent, "PurchaseRentableSpaceEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CancelRentableSpaceMessageEvent, "CancelRentableSpaceEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ToggleYouTubeVideoMessageEvent, "ToggleYouTubeVideoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.YouTubeVideoInformationMessageEvent, "YouTubeVideoInformationEvent");
            this._packetNames.TryAdd(ClientPacketHeader.YouTubeGetNextVideo, "YouTubeGetNextVideo");
            this._packetNames.TryAdd(ClientPacketHeader.SaveWiredTriggerConfigMessageEvent, "SaveWiredConfigEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SaveWiredEffectConfigMessageEvent, "SaveWiredConfigEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SaveWiredConditionConfigMessageEvent, "SaveWiredConfigEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SaveBrandingItemMessageEvent, "SaveBrandingItemEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetTonerMessageEvent, "SetTonerEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DiceOffMessageEvent, "DiceOffEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ThrowDiceMessageEvent, "ThrowDiceEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetMannequinNameMessageEvent, "SetMannequinNameEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SetMannequinFigureMessageEvent, "SetMannequinFigureEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CreditFurniRedeemMessageEvent, "CreditFurniRedeemEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetStickyNoteMessageEvent, "GetStickyNoteEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AddStickyNoteMessageEvent, "AddStickyNoteEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UpdateStickyNoteMessageEvent, "UpdateStickyNoteEvent");
            this._packetNames.TryAdd(ClientPacketHeader.DeleteStickyNoteMessageEvent, "DeleteStickyNoteEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetMoodlightConfigMessageEvent, "GetMoodlightConfigEvent");
            this._packetNames.TryAdd(ClientPacketHeader.MoodlightUpdateMessageEvent, "MoodlightUpdateEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ToggleMoodlightMessageEvent, "ToggleMoodlightEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UseOneWayGateMessageEvent, "UseFurnitureEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UseHabboWheelMessageEvent, "UseFurnitureEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OpenGiftMessageEvent, "OpenGiftEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGroupFurniSettingsMessageEvent, "GetGroupFurniSettingsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.UseSellableClothingMessageEvent, "UseSellableClothingEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ConfirmLoveLockMessageEvent, "ConfirmLoveLockEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SaveFloorPlanModelMessageEvent, "SaveFloorPlanModelEvent");
            this._packetNames.TryAdd(ClientPacketHeader.InitializeFloorPlanSessionMessageEvent, "InitializeFloorPlanSessionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.FloorPlanEditorRoomPropertiesMessageEvent, "FloorPlanEditorRoomPropertiesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OpenHelpToolMessageEvent, "OpenHelpToolEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetModeratorRoomInfoMessageEvent, "GetModeratorRoomInfoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetModeratorUserInfoMessageEvent, "GetModeratorUserInfoEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetModeratorUserRoomVisitsMessageEvent, "GetModeratorUserRoomVisitsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModerateRoomMessageEvent, "ModerateRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModeratorActionMessageEvent, "ModeratorActionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SubmitNewTicketMessageEvent, "SubmitNewTicketEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetModeratorRoomChatlogMessageEvent, "GetModeratorRoomChatlogEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetModeratorUserChatlogMessageEvent, "GetModeratorUserChatlogEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetModeratorTicketChatlogsMessageEvent, "GetModeratorTicketChatlogsEvent");
            this._packetNames.TryAdd(ClientPacketHeader.PickTicketMessageEvent, "PickTicketEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ReleaseTicketMessageEvent, "ReleaseTicketEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CloseTicketMesageEvent, "CloseTicketEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModerationMuteMessageEvent, "ModerationMuteEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModerationKickMessageEvent, "ModerationKickEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModerationBanMessageEvent, "ModerationBanEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModerationMsgMessageEvent, "ModerationMsgEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModerationCautionMessageEvent, "ModerationCautionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.ModerationTradeLockMessageEvent, "ModerationTradeLockEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetGameListingMessageEvent, "GetGameListingEvent");
            this._packetNames.TryAdd(ClientPacketHeader.InitializeGameCenterMessageEvent, "InitializeGameCenterEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetPlayableGamesMessageEvent, "GetPlayableGamesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.JoinPlayerQueueMessageEvent, "JoinPlayerQueueEvent");
            this._packetNames.TryAdd(ClientPacketHeader.Game2GetWeeklyLeaderboardMessageEvent, "Game2GetWeeklyLeaderboardEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetClubGiftsMessageEvent, "GetClubGiftsEvent");

            this._packetNames.TryAdd(ClientPacketHeader.GetHelperToolConfigurationMessageEvent, "GetHelperToolConfigurationEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OnGuideSessionDetachedMessageEvent, "OnGuideSessionDetachedEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GuideToolMessageNew, "GuideToolMessageNewEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GuideInviteToRoom, "GuideInviteToRoomEvent");
            this._packetNames.TryAdd(ClientPacketHeader.VisitRoomGuides, "VisitRoomGuidesEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GuideEndSession, "GuideEndSessionEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OnGuideSessionTyping, "OnGuideSessionTypingEvent");
            //this._packetNames.TryAdd(ClientPacketHeader.CancellInviteGuide, "CancellInviteGuideEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OnGuideMessageEvent, "OnGuideMessageEvent");
            this._packetNames.TryAdd(ClientPacketHeader.OnGuideFeedbackMessageEvent, "OnGuideFeedbackEvent");

            this._packetNames.TryAdd(ClientPacketHeader.AcceptPollMessageEvent, "AcceptPollEvent");
            this._packetNames.TryAdd(ClientPacketHeader.RefusePollMessageEvent, "RefusePollEvent");
            this._packetNames.TryAdd(ClientPacketHeader.AnswerPollQuestionMessageEvent, "AnswerPollQuestionEvent");

            this._packetNames.TryAdd(ClientPacketHeader.HabboCameraMessageEvent, "HabboCameraEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCameraRequest, "GetCameraRequestEvent");
            //this._packetNames.TryAdd(ClientPacketHeader.HabboCameraPublishPhoto, "HabboCameraPublishPhotoEvent");
            //this._packetNames.TryAdd(ClientPacketHeader.GetCameraPriceMessageEvent, "GetCameraPriceEvent");
            //this._packetNames.TryAdd(ClientPacketHeader.SaveRoomThumbnailMessageEvent, "SaveRoomThumbnailEvent");

            this._packetNames.TryAdd(ClientPacketHeader.GetCraftingListMessageEvent, "GetCraftingListEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetCraftingRecipesAvailableMessageEvent, "GetCraftingRecipesAvailableEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CraftSecretMessageEvent, "CraftSecretEvent");
            this._packetNames.TryAdd(ClientPacketHeader.GetRecipeConfigMessageEvent, "GetRecipeConfigEvent");
            this._packetNames.TryAdd(ClientPacketHeader.CraftedRecipeExecutedMessageEvent, "CraftedRecipeExecutedEvent");

            this._packetNames.TryAdd(ClientPacketHeader.GetSanctionStatusMessageEvent, "GetSanctionStatusEvent");
            this._packetNames.TryAdd(ClientPacketHeader.SendHelpTicketMessageEvent, "SendHelpTicketEvent");

            this._packetNames.TryAdd(ClientPacketHeader.AmbassadorWarningMessageEvent, "AmbassadorWarningEvent");
        }
    }
}