using System;
using System.Linq;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Quests;
using System.Collections.Generic;
using System.Drawing;
using Plus.HabboHotel.Pathfinding;
using Plus.Utilities;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Events.Methods
{
    /// <summary>
    /// Triggered when the user's health changes
    /// </summary>
    public class OnHealthChange : IEvent
    {
        /// <summary>
        /// Responds to the event
        /// </summary>
        public void Execute(object Source, object[] Params)
        {
            GameClient Client = (GameClient)Source;
            if (Client == null || Client.GetRoleplay() == null || Client.GetHabbo() == null)
                return;

            if (Client.GetRoleplay().CurHealth <= 0 && !Client.GetRoleplay().IsJailed && !Client.GetRoleplay().IsDead)
            {
                Client.GetRoleplay().BeingHealed = false;
                Client.GetRoleplay().CloseInteractingUserDialogues();
                Client.GetRoleplay().ClearWebSocketDialogue(true);

                if (Client.GetRoleplay().Game != null)
                    EventDeath(Client);
                else
                    NormalDeath(Client);
                return;
            }
            else
                Client.GetRoleplay().UpdateInteractingUserDialogues();

            Client.GetRoleplay().RefreshStatDialogue();
                
            if (Client.GetRoleplay().BeingHealed || Client.GetRoleplay().CurHealth <= 0 || Client.GetRoleplay().CurHealth >= Client.GetRoleplay().MaxHealth)
                return;

            if (Client.GetRoleplay().Hunger >= 100 && Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("hunger"))
            {
                int TimeCount = Client.GetRoleplay().TimerManager.ActiveTimers["hunger"].TimeCount;

                if (TimeCount == 0)
                    Client.SendWhisper("Sua vida agora está [" + Client.GetRoleplay().CurHealth + "/" + Client.GetRoleplay().MaxHealth + "]! Coma alguma coisa antes de morrer de fome!", 1);
                else
                    RoleplayManager.Shout(Client, "*[" + Client.GetRoleplay().CurHealth + "/" + Client.GetRoleplay().MaxHealth + "]*", 3);
            }
            else
                RoleplayManager.Shout(Client, "*[" + Client.GetRoleplay().CurHealth + "/" + Client.GetRoleplay().MaxHealth + "]*", 3);
        }

        /// <summary>
        /// Kills the user normally, sends them to the hospital
        /// </summary>
        /// <param name="Client"></param>
        private void NormalDeath(GameClient Client)
        {
            RoleplayManager.Shout(Client, "*Cai no chão desmaiado e morre*", 32);

            if (Client.GetRoomUser() != null)
                Client.GetRoomUser().ApplyEffect(0);

            #region Lays User Down
            if (Client.GetRoomUser() != null)
            {
                var User = Client.GetRoomUser();

                if (User.isLying)
                {
                    User.RemoveStatus("lay");
                    User.isLying = false;
                }

                if (User.isSitting)
                {
                    User.RemoveStatus("sit");
                    User.isSitting = false;
                }

                if ((User.RotBody % 2) == 0)
                {
                    if (User == null)
                        return;

                    try
                    {
                        User.Statusses.Add("lay", "1.0 null");
                        User.Z -= 0.35;
                        User.isLying = true;
                        User.UpdateNeeded = true;
                    }
                    catch { }
                }
                else
                {
                    User.RotBody--;
                    User.Statusses.Add("lay", "1.0 null");
                    User.Z -= 0.35;
                    User.isLying = true;
                    User.UpdateNeeded = true;
                }
            }
            #endregion

            if (Client.GetRoleplay().IsWorking)
            {
                WorkManager.RemoveWorkerFromList(Client);
                Client.GetRoleplay().IsWorking = false;
            }

            Client.GetRoleplay().IsDead = true;
            Client.GetRoleplay().DeadTimeLeft = RoleplayManager.DeathTime;

            if (Client.GetRoleplay() != null && Client.GetRoleplay().TimerManager != null && Client.GetRoleplay().TimerManager.ActiveTimers != null)
            {
                if (Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("death"))
                    Client.GetRoleplay().TimerManager.ActiveTimers["death"].EndTimer();
                Client.GetRoleplay().TimerManager.CreateTimer("death", 1000, true);
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.DEATH);

            int HospitalRID = Convert.ToInt32(RoleplayData.GetData("hospital", "insideroomid"));
            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(HospitalRID);

            if (Client != null && Client.GetHabbo() != null)
            {
                if (Client.GetHabbo().CurrentRoomId == HospitalRID)
                {
                    RoleplayManager.GetLookAndMotto(Client);
                    RoleplayManager.SpawnBeds(Client, "hosptl_bed");
                    Client.SendMessage(new RoomNotificationComposer("room_death_axe", "message", "Você morreu! Você está sendo levado ao Hospital."));
                }
                else
                {
                    Client.SendMessage(new RoomNotificationComposer("room_death_axe", "message", "Você morreu! Você está sendo levado ao Hospital."));
                    RoleplayManager.SendUser(Client, HospitalRID);
                }
            }
        }

        /// <summary>
        /// Kills the user normally, depends on the event mode
        /// </summary>
        /// <param name="Client"></param>
        private void EventDeath(GameClient Client)
        {
            if (Client.GetRoleplay().Game == null)
                NormalDeath(Client);

            #region Brawl
            else if (Client.GetRoleplay().Game == Games.RoleplayGameManager.GetGame(Games.GameMode.Brawl))
            {
                Client.GetRoleplay().ReplenishStats();

                RoleplayManager.Shout(Client, "*Nocauteado e expulso da briga*", 32);
                RoleplayManager.SpawnChairs(Client, "es_bench");

                Client.SendMessage(new RoomNotificationComposer("room_kick_cannonball", "message", "Você foi nocauteado e perdeu a briga!"));
                Client.GetRoleplay().Game.RemovePlayerFromGame(Client);
            }
            #endregion

            #region Team Brawl
            else if (Client.GetRoleplay().Game == Games.RoleplayGameManager.GetGame(Games.GameMode.TeamBrawl))
            {
                Client.GetRoleplay().ReplenishStats();
                RoleplayManager.Shout(Client, "*Sai da briga por ser nocauteado*", 32);

                #region Graveyard Spawn
                if (Client.GetRoomUser() != null)
                {
                    int ArenaStartX = Convert.ToInt32(RoleplayData.GetData("teambrawl", "graveyardstartx"));
                    int ArenaStartY = Convert.ToInt32(RoleplayData.GetData("teambrawl", "graveyardstarty"));
                    int ArenaFinishX = Convert.ToInt32(RoleplayData.GetData("teambrawl", "graveyardfinishx"));
                    int ArenaFinishY = Convert.ToInt32(RoleplayData.GetData("teambrawl", "graveyardfinishy"));

                    CryptoRandom Random = new CryptoRandom();
                    List<ThreeDCoord> Squares = RoleplayManager.GenerateMap(ArenaStartX, ArenaStartY, ArenaFinishX, ArenaFinishY);
                    ThreeDCoord RandomSquare = Squares[Random.Next(0, Squares.Count)] == null ? Squares.FirstOrDefault() : Squares[Random.Next(0, Squares.Count)];

                    Client.GetRoomUser().ClearMovement(true);
                    var Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                    if (Room != null)
                        Room.GetGameMap().UpdateUserMovement(new Point(Client.GetRoomUser().X, Client.GetRoomUser().Y), new Point(RandomSquare.X, RandomSquare.Y), Client.GetRoomUser());

                    Client.GetRoomUser().X = RandomSquare.X;
                    Client.GetRoomUser().Y = RandomSquare.Y;
                    Client.GetRoomUser().UpdateNeeded = true;
                }
                #endregion

                Client.SendMessage(new RoomNotificationComposer("room_kick_cannonball", "message", "Você foi nocauteado da Briga de Times!"));

                if (!Client.GetRoleplay().Team.LostMembers.Contains(Client.GetHabbo().Id))
                    Client.GetRoleplay().Team.LostMembers.Add(Client.GetHabbo().Id);

                if (Client.GetRoleplay().Team.LostMembers.Count == Client.GetRoleplay().Team.Members.Count)
                    Client.GetRoleplay().Team.InGame = false;
            }
            #endregion

            #region SoloQueue
            else if (Client.GetRoleplay().Game.GetGameMode() == Games.GameMode.SoloQueue || Client.GetRoleplay().Game.GetGameMode() == Games.GameMode.SoloQueueGuns)
            {
                Client.GetRoleplay().ReplenishStats();

                RoleplayManager.Shout(Client, "*Sai do Soloqueue por ser nocauteado*", 32);
                RoleplayManager.SpawnChairs(Client, "es_bench");

                Client.SendMessage(new RoomNotificationComposer("room_kick_cannonball", "message", "Você foi nocauteado do Soloqueue!"));
                Client.GetRoleplay().Game.RemovePlayerFromGame(Client);
            }
            #endregion

            #region Colour Wars
            else if (Client.GetRoleplay().Game == Games.RoleplayGameManager.GetGame(Games.GameMode.ColourWars))
            {
                Client.GetRoleplay().ReplenishStats();
                Client.GetRoleplay().NeedsRespawn = true;

                RoleplayManager.Shout(Client, "*Nocauteado! Você está sendo levado para sala de revivência*", 32);
                RoleplayManager.SendUser(Client, Client.GetRoleplay().Team.SpawnRoom);
                Client.SendMessage(new RoomNotificationComposer("room_kick_cannonball", "message", "Você foi nocauteado. Vai demorar dois minutos para recuperar."));

                new Thread(() =>
                {
                    Thread.Sleep(2000);

                    int Counter = 0;
                    while (Counter < 200)
                    {
                        if (Client == null || Client.GetRoleplay() == null || Client.GetRoleplay().Game == null || Client.GetRoleplay().Team == null || Client.GetRoleplay().Game.GetGameMode() != Games.GameMode.ColourWars)
                        {
                            if (Client.GetRoomUser() != null)
                                Client.GetRoomUser().CanWalk = true;

                            if (Client.GetRoleplay() != null)
                                Client.GetRoleplay().NeedsRespawn = false;
                            break;
                        }

                        Counter++;
                        Thread.Sleep(1000);

                        if (Counter == 30)
                            Client.SendWhisper("Você tem 1 minuto, 30 segundos restantes até que você possa se mover novamente!", 1);
                        else if (Counter == 60)
                            Client.SendWhisper("Você tem 1 minuto restante até que você possa se mover novamente!", 1);
                        else if (Counter == 90)
                            Client.SendWhisper("Você tem 30 segundos restantes até que você possa se mover novamente!", 1);
                        else if (Counter == 110)
                            Client.SendWhisper("Você tem 10 segundos restantes até que você possa se mover novament!", 1);
                        else if (Counter == 120)
                            Client.SendWhisper("Você agora está consciente e pode se mover!", 1);
                        else if (Counter >= 121)
                        {
                            if (Client.GetRoomUser() != null)
                                Client.GetRoomUser().CanWalk = true;

                            if (Client.GetRoleplay() != null)
                                Client.GetRoleplay().NeedsRespawn = false;
                            break;
                        }
                    }
                }).Start();
            }
            #endregion

            #region Mafia Wars
            else if (Client.GetRoleplay().Game == Games.RoleplayGameManager.GetGame(Games.GameMode.MafiaWars))
            {
                Client.GetRoleplay().ReplenishStats();
                Client.GetRoleplay().NeedsRespawn = true;

                RoleplayManager.Shout(Client, "*Nocauteado! Você está sendo levado para sala de revivência*", 32);
                RoleplayManager.SendUser(Client, Client.GetRoleplay().Team.SpawnRoom);
                Client.SendMessage(new RoomNotificationComposer("room_kick_cannonball", "message", "Você foi nocauteado! Levará 35 segundos para recuperar."));

                new Thread(() =>
                {
                    Thread.Sleep(2000);


                    if (Client.GetRoomUser() != null)
                    {
                        Client.GetRoomUser().ApplyEffect(EffectsList.Ghost);
                        Client.GetRoomUser().Frozen = true;
                    }

                    Thread.Sleep(4000);

                    int Counter = 0;
                    while (Counter < 30)
                    {

                        if (Client == null)
                            break;

                        if (Client.GetRoomUser() == null)
                            break;

                        Counter++;
                        Thread.Sleep(1000);
                        
                        if (Counter == 30)
                        {
                            Client.SendWhisper("Agora você pode se mover novamente!", 1);

                            if (Client.GetRoomUser() != null)
                            {
                                Client.GetRoomUser().ApplyEffect(EffectsList.None);
                                Client.GetRoomUser().CanWalk = true;
                                Client.GetRoomUser().Frozen = false;
                            }

                            if (Client.GetRoleplay() != null)
                                Client.GetRoleplay().NeedsRespawn = false;
                            break;
                        }
                    }
                }).Start();
            }
            #endregion
        }
    }
}