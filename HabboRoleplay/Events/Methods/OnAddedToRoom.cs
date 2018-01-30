using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Guides;
using Plus.HabboRoleplay.Houses;
using Plus.Communication.Packets.Outgoing.Guides;
using System.Drawing;
using Plus.HabboRoleplay.Farming;
using Plus.HabboRoleplay.Gambling;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Events.Methods
{
    /// <summary>
    /// Triggered when the user is added to the room
    /// </summary>
    public class OnAddedToRoom : IEvent
    {
        #region Execute Event
        /// <summary>
        /// Responds to the event
        /// </summary>
        public void Execute(object Source, object[] Params)
        {
            GameClient Client = (GameClient)Source;
            if (Client == null || Client.GetRoleplay() == null || Client.GetHabbo() == null)
                return;

            Room Room = (Room)Params[0];

            TutorialBadge(Client, Params);

            if (Client.GetRoleplay().PoliceTrial)
                Client.GetRoleplay().PoliceTrial = false;

            #region WebSocket Dialogue Check
            Client.GetRoleplay().ClearWebSocketDialogue();
            #endregion

            #region Police Car Enable Check
            if (Client.GetRoomUser() != null)
            {
                if (Client.GetRoomUser().CurrentEffect == EffectsList.CarPolice)
                    Client.GetRoomUser().ApplyEffect(EffectsList.None);
            }
            #endregion

            #region Spawn/Update Texas Hold 'Em Furni
            if (TexasHoldEmManager.GetGamesByRoomId(Room.RoomId).Count > 0)
            {
                List<TexasHoldEm> Games = TexasHoldEmManager.GetGamesByRoomId(Room.RoomId);

                foreach (TexasHoldEm Game in Games)
                {
                    if (Game != null)
                    {
                        #region PotSquare Check
                        if (Game.PotSquare.Furni != null)
                        {
                            if (Game.PotSquare.Furni.GetX != Game.PotSquare.X && Game.PotSquare.Furni.GetY != Game.PotSquare.Y && Game.PotSquare.Furni.GetZ != Game.PotSquare.Z && Game.PotSquare.Furni.Rotation != Game.PotSquare.Rotation)
                            {
                                if (Room.GetRoomItemHandler().GetFloor.Contains(Game.PotSquare.Furni))
                                    Room.GetRoomItemHandler().RemoveFurniture(null, Game.PotSquare.Furni.Id);
                                Game.PotSquare.SpawnDice();
                            }
                        }
                        else
                            Game.PotSquare.SpawnDice();
                        #endregion

                        #region JoinGate Check
                        if (Game.JoinGate.Furni != null)
                        {
                            if (Game.JoinGate.Furni.GetX != Game.JoinGate.X && Game.JoinGate.Furni.GetY != Game.JoinGate.Y && Game.JoinGate.Furni.GetZ != Game.JoinGate.Z && Game.JoinGate.Furni.Rotation != Game.JoinGate.Rotation)
                            {
                                if (Room.GetRoomItemHandler().GetFloor.Contains(Game.JoinGate.Furni))
                                    Room.GetRoomItemHandler().RemoveFurniture(null, Game.JoinGate.Furni.Id);
                                Game.JoinGate.SpawnDice();
                            }
                        }
                        else
                            Game.JoinGate.SpawnDice();
                        #endregion

                        #region Player1 Check
                        foreach (TexasHoldEmItem Item in Game.Player1.Values)
                        {
                            if (Item.Furni != null)
                            {
                                if (Item.Furni.GetX != Item.X && Item.Furni.GetY != Item.Y && Item.Furni.GetZ != Item.Z && Item.Furni.Rotation != Item.Rotation)
                                {
                                    if (Room.GetRoomItemHandler().GetFloor.Contains(Item.Furni))
                                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Furni.Id);
                                    Item.SpawnDice();
                                }
                            }
                            else
                                Item.SpawnDice();
                        }
                        #endregion

                        #region Player2 Check
                        foreach (TexasHoldEmItem Item in Game.Player2.Values)
                        {
                            if (Item.Furni != null)
                            {
                                if (Item.Furni.GetX != Item.X && Item.Furni.GetY != Item.Y && Item.Furni.GetZ != Item.Z && Item.Furni.Rotation != Item.Rotation)
                                {
                                    if (Room.GetRoomItemHandler().GetFloor.Contains(Item.Furni))
                                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Furni.Id);
                                    Item.SpawnDice();
                                }
                            }
                            else
                                Item.SpawnDice();
                        }
                        #endregion

                        #region Player3 Check
                        foreach (TexasHoldEmItem Item in Game.Player3.Values)
                        {
                            if (Item.Furni != null)
                            {
                                if (Item.Furni.GetX != Item.X && Item.Furni.GetY != Item.Y && Item.Furni.GetZ != Item.Z && Item.Furni.Rotation != Item.Rotation)
                                {
                                    if (Room.GetRoomItemHandler().GetFloor.Contains(Item.Furni))
                                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Furni.Id);
                                    Item.SpawnDice();
                                }
                            }
                            else
                                Item.SpawnDice();
                        }
                        #endregion

                        #region Banker Check
                        foreach (TexasHoldEmItem Item in Game.Banker.Values)
                        {
                            if (Item.Furni != null)
                            {
                                if (Item.Furni.GetX != Item.X && Item.Furni.GetY != Item.Y && Item.Furni.GetZ != Item.Z && Item.Furni.Rotation != Item.Rotation)
                                {
                                    if (Room.GetRoomItemHandler().GetFloor.Contains(Item.Furni))
                                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Furni.Id);
                                    Item.SpawnDice();
                                }
                            }
                            else
                                Item.SpawnDice();
                        }
                        #endregion
                    }
                }
            }
            #endregion

            #region Spawn/Update Turf Flag Furni
            if (Room.TurfEnabled)
            {
                Turf Turf = TurfManager.GetTurf(Room.RoomId);

                if (Turf != null)
                {
                    if (Turf.FlagSpawned)
                    {
                        if (Turf.Flag.GroupId != Turf.GangId || (Turf.Flag.GetX != Turf.FlagX && Turf.Flag.GetY != Turf.FlagY))
                            Turf.SpawnFlag();
                    }
                    else
                        Turf.SpawnFlag();
                }
            }
            #endregion

            #region Spawn/Update House Signs
            List<House> Houses = PlusEnvironment.GetGame().GetHouseManager().GetHousesBySignRoomId(Room.Id);
            if (Houses.Count > 0)
            {
                foreach (House House in Houses)
                {
                    if (House.Sign.Spawned)
                    {
                        if (House.Sign.Item.GetX != House.Sign.X && House.Sign.Item.GetY != House.Sign.Y && House.Sign.Item.GetZ != House.Sign.Z)
                            House.SpawnSign();
                    }
                    else
                        House.SpawnSign();
                }
            }
            #endregion

            #region Spawn/Update Farming Spaces
            List<FarmingSpace> FarmingSpaces = FarmingManager.GetFarmingSpacesByRoomId(Room.Id);
            if (FarmingSpaces.Count > 0)
            {
                foreach (FarmingSpace Space in FarmingSpaces)
                {
                    if (Space.Spawned)
                    {
                        if (Space.Item.GetX != Space.X && Space.Item.GetY != Space.Y && Space.Item.GetZ != Space.Z)
                            Space.SpawnSign();
                    }
                    else
                        Space.SpawnSign();
                }
            }
            #endregion

            #region Spawn Jailbreak Fence
            if (Room.RoomId == Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")) && !JailbreakManager.JailbreakActivated)
            {
                int X = Convert.ToInt32(RoleplayData.GetData("jailbreak", "fencex"));
                int Y = Convert.ToInt32(RoleplayData.GetData("jailbreak", "fencey"));
                int Rot = Convert.ToInt32(RoleplayData.GetData("jailbreak", "fencerotation"));

                if (Room.GetRoomItemHandler().GetFloor.Where(x => x.BaseItem == 8049 && x.GetX == X && x.GetY == Y).ToList().Count <= 0)
                {
                    double MaxHeight = 0.0;
                    Item ItemInFront;
                    if (Room.GetGameMap().GetHighestItemForSquare(new Point(X, Y), out ItemInFront))
                    {
                        if (ItemInFront != null)
                            MaxHeight = ItemInFront.TotalHeight;
                    }

                    RoleplayManager.PlaceItemToRoom(null, 8049, 0, X, Y, MaxHeight, Rot, false, Room.RoomId, false);
                }
            }
            #endregion

            #region Taxi Message
            if (Client.GetRoleplay().AntiArrowCheck)
                Client.GetRoleplay().AntiArrowCheck = false;

            if (Client.GetRoleplay().InsideTaxi)
            {
                int Bubble = (Client.GetHabbo().GetPermissions().HasRight("mod_tool") && Client.GetRoleplay().StaffOnDuty) ? 23 : 4;
                Client.GetRoleplay().InsideTaxi = false;

                new Thread(() =>
                {
                    Thread.Sleep(500);
                    RoleplayManager.Shout(Client, "*Chega ao meu destino*", Bubble);
                }).Start();
            }
            else
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.SOCIAL_VISIT);
            #endregion

            #region Room Entrance Message
            if (Room.EnterRoomMessage != "none")
            {
                new Thread(() =>
                {
                    Thread.Sleep(500);
                    Client.SendWhisper(Room.EnterRoomMessage, 34);
                }).Start();
            }
            #endregion

            #region Main checks

            BotInteractionCheck(Client, Params);

            if (Client.GetRoleplay().Game == null && Client.GetRoleplay().Team == null)
            {
                HomeRoomCheck(Client, Params);
                JobCheck(Client, Params);
                SendhomeCheck(Client, Params);
                DeathCheck(Client, Params);
                JailCheck(Client, Params);
                WantedCheck(Client, Params);
                ProbationCheck(Client, Params);
               
                #region AFK check

                if (Client.GetRoomUser() != null)
                    Client.GetHabbo().Poof(true);

                #endregion
            }
            #endregion

            #region Event/Game Checks
            else
            {
                if (Client.GetRoleplay().Game != null && Client.GetRoleplay().Team != null)
                {
                    if (!Client.GetRoleplay().GameSpawned || Client.GetRoleplay().NeedsRespawn)
                    {
                        if (Client.GetRoleplay().Game != Games.RoleplayGameManager.GetGame(Games.GameMode.Brawl))
                        {
                            var OldCoord = new Point(Client.GetRoomUser().Coordinate.X, Client.GetRoomUser().Coordinate.Y);
                            var NewCoord = new Point(Client.GetRoleplay().Team.SpawnPoint.X, Client.GetRoleplay().Team.SpawnPoint.Y);

                            Client.GetHabbo().CurrentRoom.GetGameMap().UpdateUserMovement(OldCoord, NewCoord, Client.GetRoomUser());
                            Client.GetRoomUser().SetPos(NewCoord.X, NewCoord.Y, Client.GetHabbo().CurrentRoom.GetGameMap().GetHeightForSquare(NewCoord));

                            if (Client.GetRoleplay().NeedsRespawn)
                            {
                                if (Client.GetRoomUser() != null)
                                    Client.GetRoomUser().CanWalk = false;
                            }

                            if (!Client.GetRoleplay().GameSpawned)
                            {
                                Client.GetRoleplay().GameSpawned = true;
                                Client.SendNotification(Client.GetRoleplay().Game.GetName() + " Começou! Boa sorte!");
                            }
                        }
                    }
                }
            }
            #endregion           
        }
        #endregion

        #region Give Tutorial Badge
        public void TutorialBadge(GameClient Client, object[] Params)
        {
            Room Room = (Room)Params[0];

            int FinalTutorialRoom = Convert.ToInt32(RoleplayData.GetData("tutorial", "finishroomid"));

            if (Room == null)
                return;

            if (Room.Id != FinalTutorialRoom)
                return;

            if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().GetBadgeComponent() == null)
                return;

            Client.GetHabbo().GetBadgeComponent().GiveBadge("BR248", true, Client);
            Client.SendNotification("You have just been awarded the 'Tutorial Master' badge for completing the tutorial!\n\nCongratulations!");
        }
        #endregion

        #region HomeRoomCheck
        /// <summary>
        /// Checks if the users homeroom is the correct one
        /// </summary>
        private void HomeRoomCheck(GameClient Client, object[] Params)
        {
            Room Room = (Room)Params[0];
            if (Room.Id == 0)
                return;

            if (Client.GetHabbo().HomeRoom != Room.Id)
                Client.GetHabbo().HomeRoom = Room.Id;
        }
        #endregion

        #region JobCheck
        /// <summary>
        /// Checks if the user is in the correct working room
        /// </summary>
        private void JobCheck(GameClient Client, object[] Params)
        {
            if (Client.GetHabbo().CurrentRoom == null)
                Client.GetRoleplay().IsWorking = false;

            if (Client.GetRoleplay().JobId != 1 && Client.GetRoleplay().IsWorking)
            {
                Room Room = (Room)Params[0];
                int JobId = Client.GetRoleplay().JobId;
                int JobRank = Client.GetRoleplay().JobRank;

                if (!GroupManager.GetJobRank(JobId, JobRank).CanWorkHere(Room.Id))
                {
                    if (GroupManager.HasJobCommand(Client, "guide"))
                    {
                        GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
                        guideManager.RemoveGuide(Client);

                        #region End Existing Calls

                        if (Client.GetRoleplay().GuideOtherUser != null)
                        {
                            Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                            Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                            if (Client.GetRoleplay().GuideOtherUser.GetRoleplay() != null)
                            {
                                Client.GetRoleplay().GuideOtherUser.GetRoleplay().Sent911Call = false;
                                Client.GetRoleplay().GuideOtherUser.GetRoleplay().GuideOtherUser = null;
                            }

                            Client.GetRoleplay().GuideOtherUser = null;
                            Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                            Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                        }
                        #endregion
                        else
                            Client.SendMessage(new HelperToolConfigurationComposer(Client));
                    }
                    WorkManager.RemoveWorkerFromList(Client);
                    Client.GetRoleplay().IsWorking = false;
                    Client.GetHabbo().Poof();
                }
            }
            else
                return;
        }
        #endregion

        #region Sendhome Check
        /// <summary>
        /// Checks if the user has been senthome
        /// </summary>
        /// <param name="Client"></param>
        public void SendhomeCheck(GameClient Client, object[] Params)
        {
            if (Client.GetRoleplay().SendHomeTimeLeft <= 0)
                return;

            if (Client.GetRoleplay().SendHomeTimeLeft > 30)
                Client.GetRoleplay().SendHomeTimeLeft = 30;

            if (!Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("sendhome"))
                Client.GetRoleplay().TimerManager.CreateTimer("sendhome", 1000, false);
            else
                return;
        }
        #endregion

        #region DeathCheck
        /// <summary>
        /// Checks to see if the client is dead, if true send back to hospital if not already in one
        /// </summary>
        private void DeathCheck(GameClient Client, object[] Params)
        {
            if (Client.GetRoleplay().IsDead)
            {
                int HospitalRID = Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2"));
                Room Room = (Room)Params[0];

                if (Room.Id != HospitalRID)
                {
                    RoleplayManager.SendUser(Client, HospitalRID);
                    Client.SendNotification("You cannot leave the hospital while you are dead!");
                }

                RoleplayManager.GetLookAndMotto(Client);
                RoleplayManager.SpawnBeds(Client, "hosptl_bed");
            }
            else
                return;
        }
        #endregion

        #region JailCheck
        /// <summary>
        /// Checks to see if the client is jailed, if true send back to jail if not already in one
        /// </summary>
        private void JailCheck(GameClient Client, object[] Params)
        {
            if (Client.GetRoleplay().IsJailed)
            {
                if (Client.GetRoleplay().Jailbroken)
                {
                    RoleplayManager.GetLookAndMotto(Client);
                    return;
                }

                int JailRID = Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid"));
                int JailRID2 = Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid"));
                int CourtRID = Convert.ToInt32(RoleplayData.GetData("court", "roomid"));
                Room Room = (Room)Params[0];

                if (RoleplayManager.Defendant == Client && Room.Id == CourtRID)
                {
                    RoleplayManager.GetLookAndMotto(Client);
                    RoleplayManager.SpawnChairs(Client, "uni_lectern");

                    new Thread(() =>
                    {
                        Thread.Sleep(500);
                        if (Client.GetRoomUser() != null)
                            Client.GetRoomUser().Frozen = true;
                    }).Start();
                    return;
                }

                if (Room.Id != JailRID && Room.Id != JailRID2)
                {
                    RoleplayManager.SendUser(Client, JailRID);
                    Client.SendNotification("You cannot leave jail until your sentence has expired!");
                }

                if (Room.Id == JailRID)
                {
                    RoleplayManager.GetLookAndMotto(Client);
                    RoleplayManager.SpawnBeds(Client, "bed_silo_one");
                }
            }
            else
                return;
        }
        #endregion

        #region Wanted Check
        /// <summary>
        /// Checks if the user is wanted
        /// </summary>
        /// <param name="Client"></param>
        public void WantedCheck(GameClient Client, object[] Params)
        {
            if (!Client.GetRoleplay().IsWanted)
                return;

            if (RoleplayManager.WantedList.ContainsKey(Client.GetHabbo().Id))
                return;

            Room Room = (Room)Params[0];
            string RoomId = Room.Id.ToString() != "0" ? Room.Id.ToString() : "Unknown";

            if (!RoleplayManager.WantedList.ContainsKey(Client.GetHabbo().Id))
            {
                Wanted Wanted = new Wanted(Convert.ToUInt32(Client.GetHabbo().Id), RoomId, Client.GetRoleplay().WantedLevel);
                RoleplayManager.WantedList.TryAdd(Client.GetHabbo().Id, Wanted);
            }

            if (!Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("wanted"))
                Client.GetRoleplay().TimerManager.CreateTimer("wanted", 1000, false);
        }
        #endregion

        #region Probation Check
        /// <summary>
        /// Checks if the user is on probation
        /// </summary>
        /// <param name="Client"></param>
        public void ProbationCheck(GameClient Client, object[] Params)
        {
            if (!Client.GetRoleplay().OnProbation)
                return;

            if (!Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("probation"))
                Client.GetRoleplay().TimerManager.CreateTimer("probation", 1000, false);
            else
                return;
        }
        #endregion

        #region Bot Interaction Check
        /// <summary>
        /// Checks for any possible interactions with bots in room
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Params"></param>
        public void BotInteractionCheck(GameClient Client, object[] Params)
        {
            Room Room = Client.GetHabbo().CurrentRoom;
            if (Room == null) return;

            List<RoomUser> Bots = Room.GetRoomUserManager().GetBotList().ToList();

            foreach (RoomUser Bot in Bots)
            {
                if (Bot == null)
                    continue;

                if (!Bot.IsBot)
                    continue;

                if (!Bot.IsRoleplayBot)
                    continue;

                if (!Bot.GetBotRoleplay().Deployed)
                    continue;

                Bot.GetBotRoleplayAI().OnUserEnterRoom(Client);
            }
        }
        #endregion

    }
}