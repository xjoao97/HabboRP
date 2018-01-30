using System;
using System.Linq;
using System.Drawing;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.HabboRoleplay.Bots;
using Plus.Utilities;

using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Minigames.Modes.MafiaWars;
using System.Threading;
using Plus.HabboRoleplay.Bots.Manager.TimerHandlers;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using static Plus.HabboRoleplay.Bots.Manager.TimerHandlers.TimerHandlerManager;

namespace Plus.HabboRoleplay.Combat.Types
{
    public class Fist : ICombat
    {
        /// <summary>
        /// Executes this type of combat
        /// </summary>
        public void Execute(GameClient Client, GameClient TargetClient, bool HitClosest = false)
        {
            RoomUser User;
            if (HitClosest)
            {
                if (!this.TryGetClosestTarget(Client, out User))
                    return;

                if (User == null)
                    return;

                if (User.IsBot && User.GetBotRoleplay() != null)
                {
                    this.ExecuteBot(Client, User.GetBotRoleplay());
                    return;
                }
                else
                    TargetClient = User.GetClient();
            }

            if (!this.CanCombat(Client, TargetClient))
                return;

            int Damage = this.GetDamage(Client, TargetClient);

            // If the user is about to die and the user attacked themself
            if ((TargetClient.GetRoleplay().CurHealth - Damage) <= 0 && TargetClient == Client)
            {
                TargetClient.SendWhisper("Você não pode se matar!", 1);
                return;
            }

            // If about to die
            if (TargetClient.GetRoleplay().CurHealth - Damage <= 0)
            {
                if (TargetClient.GetRoleplay().Game != null)
                {
                    if (TargetClient.GetRoleplay().Game.GetGameMode() == GameMode.Brawl)
                    {
                        RoleplayManager.Shout(Client, "*Dá um soco em " + TargetClient.GetHabbo().Username + ", causando " + Damage + " damage*", 6);
                        RoleplayManager.Shout(Client, "*Dá um soco em " + TargetClient.GetHabbo().Username + ", matando-o e vencendo o Evento de Brigas! [+1 Ponto de Evento]*", 6);
                        Client.GetHabbo().EventPoints++;
                        Client.GetHabbo().UpdateEventPointsBalance();
                    }
                    else if (TargetClient.GetRoleplay().Game.GetGameMode() == GameMode.SoloQueue || TargetClient.GetRoleplay().Game.GetGameMode() == GameMode.SoloQueueGuns)
                    {
                        RoleplayManager.Shout(Client, "*Dá um soco em  " + TargetClient.GetHabbo().Username + ", causando " + Damage + " damage*", 6);
                        RoleplayManager.Shout(Client, "*Dá um soco em  " + TargetClient.GetHabbo().Username + ", matando-o e vencendo o Evento Soloqueue!*", 6);
                    }
                }
                else
                {
                    Client.GetRoleplay().ClearWebSocketDialogue();

                    int Amount = this.GetCoins(TargetClient);
                    this.GetRewards(Client, TargetClient, null);

                    Client.GetHabbo().Credits += Amount;
                    Client.GetHabbo().UpdateCreditsBalance();

                    TargetClient.GetHabbo().Credits -= Amount;
                    TargetClient.GetHabbo().UpdateCreditsBalance();

                    if (Amount > 0)
                    {
                        RoleplayManager.Shout(Client, "*Dá um soco em  " + TargetClient.GetHabbo().Username + ", causando " + Damage + " damage*", 6);
                        RoleplayManager.Shout(Client, "*Dá um soco em  " + TargetClient.GetHabbo().Username + ", matando-o e roubando R$" + String.Format("{0:N0}", Amount) + " da carteira dele*", 6);
						
                        lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                        foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (client == null || client.GetHabbo() == null)
                        continue;
                
                        client.SendMessage(new RoomNotificationComposer("staff_notice", "message", "[Notícia Urgente] " + Client.GetHabbo().Username + " matou com socos o cidadão " + TargetClient.GetHabbo().Username + " e roubou  R$" + String.Format("{0:N0}", Amount) + " da carteira dele, tome cuidado pelas ruas!"));
                    }
            }
                    }
                    else
                    {
                        RoleplayManager.Shout(Client, "*Dá um soco em  " + TargetClient.GetHabbo().Username + ", causando " + Damage + " damage*", 6);
                        RoleplayManager.Shout(Client, "*Dá um soco em  " + TargetClient.GetHabbo().Username + ", matando-o*", 6);
						
                        lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                        foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (client == null || client.GetHabbo() == null)
                        continue;
                
                        client.SendMessage(new RoomNotificationComposer("staff_notice", "message", "[Notícia Urgente] " + Client.GetHabbo().Username + " matou com socos o cidadão " + TargetClient.GetHabbo().Username + ", tome cuidado pelas ruas!"));
                    }
            }
                    }

                    BountyManager.CheckBounty(Client, TargetClient.GetHabbo().Id);

                    if ((TargetClient.GetRoleplay().CurHealth - Damage) <= 0)
                        TargetClient.GetRoleplay().CurHealth = 0;
                }
            }
            else
            {
                RoleplayManager.Shout(Client, "*Dá um soco em " + TargetClient.GetHabbo().Username + ", causando " + Damage + " damage*", 6);
                Client.GetRoleplay().OpenUsersDialogue(TargetClient);
                TargetClient.GetRoleplay().OpenUsersDialogue(Client);
            }

            if (TargetClient != Client)
            {
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Punching", 1);
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.PUNCH_USER, 1);
            }

            if (Client.GetRoleplay().Game == null)
            {
                if ((Client.GetRoleplay().CurEnergy - 2) <= 0)
                    Client.GetRoleplay().CurEnergy = 0;
                else
                    Client.GetRoleplay().CurEnergy -= 2;
            }

            if (TargetClient.GetRoleplay().CurHealth - Damage <= 0)
                TargetClient.GetRoleplay().CurHealth = 0;
            else
                TargetClient.GetRoleplay().CurHealth -= Damage;

            Client.GetRoleplay().Punches++;

            if (!Client.GetRoleplay().WantedFor.Contains("tentativa de assalto + tentativa/assassinato"))
                Client.GetRoleplay().WantedFor = Client.GetRoleplay().WantedFor +"tentativa de assalto + tentativa/assassinato, ";

            Client.GetRoleplay().CooldownManager.CreateCooldown("fist", 1000, (Client.GetRoleplay().Game == null ? (Client.GetRoleplay().Class.ToLower() == "fighter" ? RoleplayManager.HitCooldown : RoleplayManager.DefaultHitCooldown) : RoleplayManager.HitCooldownInEvent));
        }

        /// <summary>
        /// Executes this type of combat on a Bot
        /// </summary>
        public void ExecuteBot(GameClient Client, RoleplayBot Bot = null)
        {
            if (!this.CanCombat(Client, null, Bot))
                return;

            int Damage = this.GetDamage(Client, null, Bot);

            RoomUser BotUser = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetBotByName(Bot.Name);
            
            if (BotUser.GetRoom() == null)
            {
                Client.SendWhisper("Este usuário não foi encontrado nesta sala!", 1);
                return;
            }

            if (BotUser == null)
            {
                Client.SendWhisper("Este usuário não foi encontrado nesta sala!", 1);
                return;
            }

            if (BotUser.GetRoom() != Client.GetRoomUser().GetRoom())
            {
                Client.SendWhisper("Este usuário não foi encontrado nesta sala!", 1);
                return;
            }

            // If about to die
            bool Died = false;

            if (Bot.CurHealth - Damage <= 0)
                Died = true;
            else
                RoleplayManager.Shout(Client, "*Dá um soco em " + Bot.Name + ", causando " + Damage + " damage*", 6);

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Punching", 1);
            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.PUNCH_USER, 1);

            if ((Client.GetRoleplay().CurEnergy - 2) <= 0)
                Client.GetRoleplay().CurEnergy = 0;
            else
                Client.GetRoleplay().CurEnergy -= 2;

            Client.GetRoleplay().Punches++;

            if (Bot.CurHealth - Damage <= 0)
                Bot.CurHealth = 0;
            else
                Bot.CurHealth -= Damage;

            if (!Died)
            {
                BotUser.Chat("*[" + Bot.CurHealth + "/" + Bot.MaxHealth + "]*", true, 3);
                BotUser.GetBotRoleplayAI().OnAttacked(Client);
            }
            else
                BotUser.GetBotRoleplayAI().OnDeath(Client);

            Client.GetRoleplay().CooldownManager.CreateCooldown("fist", 1000, (Client.GetRoleplay().Game == null ? (Client.GetRoleplay().Class.ToLower() == "fighter" ? RoleplayManager.HitCooldown : RoleplayManager.DefaultHitCooldown) : RoleplayManager.HitCooldownInEvent));
        }

        /// <summary>
        /// Checks if a client can complete this action
        /// </summary>
        public bool CanCombat(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null)
        {
            RoomUser RoomUser = Client.GetRoomUser();
            RoomUser TargetRoomUser = Bot == null ? TargetClient.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username) : Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetBotByName(Bot.Name);

            if (RoomUser == null || TargetRoomUser == null)
                return false;

            if (Bot != null)
            {
                if (!Bot.CanBeAttacked)
                {
                    Client.SendWhisper("Desculpe, mas este bot não pode ser atacado!", 1);
                    return false;
                }

                if (Bot.AIType == RoleplayBotAIType.MAFIAWARS)
                {

                    if (Client.GetRoleplay().Team == null)
                    {
                        Client.SendWhisper("Como você não está na Guerra de Máfias, você não pode fazer isso!", 1);
                        return false;
                    }
                    
                    if (Client.GetRoleplay().Game == null)
                    {
                        Client.SendWhisper("Como você não está na Guerra de Máfias, você não pode fazer isso!", 1);
                        return false;
                    }

                    if (Client.GetHabbo().VIPRank < 2 && Client.GetRoleplay().Game.GetGameMode() != GameMode.MafiaWars)
                    {
                        Client.SendWhisper("Como você não está na Guerra de Máfias, você não pode fazer iss!", 1);
                        return false;
                    }

                    string BotsTeam = Bot.Motto.ToLower().Split(' ')[0];

                    if (Client.GetRoleplay().Team.Name.ToLower() == BotsTeam)
                    {
                        Client.SendWhisper("Você não pode atacar os membros da sua própria equipe!", 1);
                        return false;
                    }
                }
            }

            Point ClientPos = RoomUser.Coordinate;
            Point TargetClientPos = TargetRoomUser.Coordinate;

            #region Main Conditions
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            Room Room = null;

            if (Client.GetHabbo().CurrentRoomId > 0)
                Room = Client.GetHabbo().CurrentRoom;

            if (Room != null)
            {
                if (Room.SafeZoneEnabled)
                {
                    Client.SendWhisper("Você não pode bater nesta sala!", 1);
                    return false;
                }

                if (!RoleplayManager.PurgeStarted)
                {
                    if (!Room.HitEnabled && Client.GetRoleplay().Game == null)
                    {
                        Client.SendWhisper("Você não pode bater nesta sala!", 1);
                        return false;
                    }
                }
            }

            if (TargetRoomUser == null)
            {
                Client.SendWhisper("Esta pessoa não está na mesma sala que você!", 1);
                return false;
            }

            if (Client.GetRoleplay().Game != null)
            {
                if (!Client.GetRoleplay().Game.HasGameStarted())
                {
                    Client.SendWhisper("O evento ainda não começou!", 1);
                    return false;
                }
                if (TargetClient != null && TargetClient.GetRoleplay().Game != Client.GetRoleplay().Game)
                {
                    Client.SendWhisper("Seu alvo não faz parte desse evento!", 1);
                    return false;
                }
                if (RoomUser != null)
                {
                    if (RoomUser.Frozen)
                        return false;
                }

                if (TargetRoomUser != null)
                {
                    if (TargetRoomUser.Frozen)
                        return false;
                }
            }

            if (TargetClient != null && Client.GetRoleplay().Team != null && TargetClient.GetRoleplay().Team != null)
            {
                if (Client.GetRoleplay().Team == TargetClient.GetRoleplay().Team)
                {
                    Client.SendWhisper("Você não pode atacar seu companheiro de equipe!", 1);
                    return false;
                }
            }

            if (Client.GetRoleplay().Game == null)
            {
                if (RoleplayManager.LevelDifference)
                {
                    if (!Room.TurfEnabled)
                    {
                        int TargetLevel = Bot == null ? TargetClient.GetRoleplay().Level : Bot.Level;
                        int LevelDifference = Math.Abs(Client.GetRoleplay().Level - TargetLevel);

                        if (LevelDifference > 8)
                        {
                            Client.SendWhisper("Você não pode atingir essa pessoa, pois sua diferença de nível é maior do que 8!", 1);
                            return false;
                        }

                    }
                }
            }

            if (Client.GetRoleplay().Game == null)
            {
                if (Client.GetRoleplay().IsNoob)
                {
                    if (!Client.GetRoleplay().NoobWarned)
                    {
                        Client.SendWhisper("[OBSERVE] Se fizer isso mais vezes você perderá sua proteção de Deus e poderá ser atacado. (Avisos: 1/2)", 1);
                        Client.GetRoleplay().NoobWarned = true;
                        return false;
                    }
                    else if (!Client.GetRoleplay().NoobWarned2)
                    {
                        Client.SendWhisper("[OBSERVE] Se fizer isso mais vezes você perderá sua proteção de Deus e poderá ser atacado. (Avisos: 2/2)", 1);
                        Client.GetRoleplay().NoobWarned2 = true;
                        return false;
                    }
                    else
                    {
                        Client.SendWhisper("Você perdeu sua proteção de deus, você está sozinho agora.", 1);

                        if (Client.GetRoleplay().TimerManager != null && Client.GetRoleplay().TimerManager.ActiveTimers != null)
                        {
                            if (Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("noob"))
                                Client.GetRoleplay().TimerManager.ActiveTimers["noob"].EndTimer();
                        }

                        Client.GetRoleplay().IsNoob = false;
                        Client.GetRoleplay().NoobTimeLeft = 0;
                        return true;
                    }
                }
            }

            if (Client.GetRoleplay().TryGetCooldown("fist"))
                return false;

            #endregion

            #region Status Conditions
            if (RoomUser.Frozen)
            {
                Client.SendWhisper("Você não pode fazer isso enquanto você está atordoado!", 1);
                return false;
            }

            if (RoomUser.IsAsleep)
            {
                Client.SendWhisper("Você não pode fazer isso enquanto a pessoa está ausente!", 1);
                return false;
            }

            if (Client.GetRoleplay().IsDead)
            { 
              Client.SendWhisper("Você não pode bater em alguém enquanto está morto!", 1);
              return false;
            }

            if (Client.GetRoleplay().IsJailed)
            {
                Client.SendWhisper("Você não pode bater em alguém enquanto está preso!", 1);
                return false;
            }

            if (Client.GetRoleplay().IsWorking)
            {
                Client.SendWhisper("Você não pode bater em alguém enquanto trabalha!", 1);
                return false;
            }

            if (Client.GetRoleplay().StaffOnDuty || Client.GetRoleplay().AmbassadorOnDuty)
            {
                Client.SendWhisper("Você não pode bater em alguém enquanto está de plantão!", 1);
                return false;
            }

            if (Bot != null)
            {
                if (Bot.Dead)
                {
                    Client.SendWhisper("Você não pode bater em alguém que está morto!", 1);
                    return false;
                }

                if (Bot.Jailed)
                {
                    Client.SendWhisper("Você não pode bater em alguém preso!", 1);
                    return false;
                }
            }

            if (TargetClient != null)
            {
                if (TargetClient.GetRoleplay().IsDead)
                {
                    Client.SendWhisper("Você não pode bater em alguém que está morto!", 1);
                    return false;
                }

                if (TargetClient.GetRoleplay().IsJailed)
                {
                    Client.SendWhisper("Você não pode bater em alguém que está preso!", 1);
                    return false;
                }

                if (TargetClient.GetRoleplay().StaffOnDuty)
                {
                    Client.SendWhisper("Você não pode bater em alguém que está de plantão!", 1);
                    return false;
                }

                if (TargetClient.GetRoleplay().AmbassadorOnDuty)
                {
                    Client.SendWhisper("Você não pode bater em um embaixador que está de plantão!", 1);
                    return false;
                }

                if (TargetClient == Client)
                {
                    Client.SendWhisper("Você não pode bater em si mesmo!", 1);
                    return false;
                }

                if (TargetClient.MachineId == Client.MachineId)
                {
                    Client.SendWhisper("Você não pode bater em outra das suas contas!", 1);
                    return false;
                }
            }

            if (Client.GetRoleplay().CurEnergy <= 0 && Client.GetRoleplay().Game == null)
            {
                Client.SendWhisper("Você ficou sem energia para bater em alguém!", 1);
                return false;
            }

            if (Client.GetRoleplay().Cuffed)
            {
                Client.SendWhisper("Você não pode bater em um cidadão enquanto você está algemado!", 1);
                return false;
            }

            if (Client.GetRoleplay().DrivingCar)
            {
                Client.SendWhisper("Por favor, pare de dirigir seu veículo para bater em alguém!", 1);
                return false;
            }

            if (TargetRoomUser.IsAsleep)
            {
                Client.SendWhisper("Você não pode bater em alguém que está ausente", 1);
                return false;
            }
            #endregion

            #region Distance

            if (Distance > 1)
            {
                RoleplayManager.Shout(Client, "*Tenta dar um soco em " + (Bot == null ? TargetClient.GetHabbo().Username : Bot.Name) + ", mas erra*", 4);
                Client.GetRoleplay().CooldownManager.CreateCooldown("fist", 1000, (Client.GetRoleplay().Game == null ? (Client.GetRoleplay().Class.ToLower() == "fighter" ? RoleplayManager.HitCooldown : RoleplayManager.DefaultHitCooldown) : RoleplayManager.HitCooldownInEvent));
                return false;
            }

            #endregion

            return true;
        }

        /// <summary>
        /// Selects the closest person to the client
        /// </summary>
        public bool TryGetClosestTarget(GameClient Client, out RoomUser Target)
        {
            Target = null;

            if (Client.GetRoomUser() == null)
                return false;

            if (Client.GetRoomUser().RoomId <= 0)
                return false;

            var Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);

            if (Room == null)
                return false;

            if (Room.GetRoomUserManager().GetRoomUsers().Count <= 1)
            {
                Client.SendWhisper("Não há ninguém perto de você para atingir automaticamente!", 1);
                return false;
            }

            var Point = new Point(Client.GetRoomUser().Coordinate.X, Client.GetRoomUser().Coordinate.Y);

            ConcurrentDictionary<RoomUser, double> PossibleUsers = new ConcurrentDictionary<RoomUser, double>();
            lock (Room.GetRoomUserManager().GetRoomUsers())
            {
                foreach (var User in Room.GetRoomUserManager().GetRoomUsers())
                {
                    if (User.IsBot)
                    {
                        if (User.GetBotRoleplay() == null)
                            continue;

                        if (!User.GetBotRoleplay().CanBeAttacked)
                            continue;
                    }

                    if (User == Client.GetRoomUser())
                        continue;

                    Point TargetPoint = new Point(User.Coordinate.X, User.Coordinate.Y);
                    double Distance = RoleplayManager.GetDistanceBetweenPoints2D(Point, TargetPoint);

                    if (!User.IsBot && Client.GetRoleplay().Game != null && Client.GetRoleplay().Game.GetGameMode() == Games.GameMode.TeamBrawl)
                    {
                        if (Client.GetRoleplay().Team != null && User.GetClient().GetRoleplay().Team != null && User.GetClient().GetRoleplay().Team == Client.GetRoleplay().Team)
                            continue;
                    }

                    if (!PossibleUsers.ContainsKey(User))
                        PossibleUsers.TryAdd(User, Distance);
                }
            }

            var OrderedUsers = PossibleUsers.OrderBy(x => x.Value);

            if (OrderedUsers.ToList().Count < 1)
                return false;

            Target = OrderedUsers.FirstOrDefault().Key;

            if (Target != null)
                return true;

            return false;
        }

        /// <summary>
        /// Gets the damage
        /// </summary>
        private int GetDamage(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null)
        {
            CryptoRandom Randomizer = new CryptoRandom();

            if (Client.GetRoleplay().Game != null && Client.GetRoleplay().Game.GetGameMode() == GameMode.ColourWars)
                return Randomizer.Next(5, 13);

            int Strength = Client.GetRoleplay().Strength;
            int MinDamage = (Strength - 6) <= 0 ? 5 : (Strength - 6);
            int MaxDamage = Strength + 6;

            // Lucky shot?
            if (Randomizer.Next(0, 100) < 12)
            {
                MinDamage = Strength + 12;
                MaxDamage = MinDamage + 3;
            }

            int Damage = Randomizer.Next(MinDamage, MaxDamage);

            if (Client.GetRoleplay().Class.ToLower() == "fighter")
                Damage += Randomizer.Next(2, 4);

            if (Client.GetRoleplay().GangId > 1000 && Bot == null)
            {
                if (GroupManager.HasGangCommand(Client, "fighter"))
                {
                    if (RoleplayManager.GenerateRoom(Client.GetHabbo().CurrentRoomId, false).TurfEnabled || GroupManager.HasJobCommand(TargetClient, "guide"))
                        Damage += Randomizer.Next(1, 3);
                }
            }

            if (Client.GetRoleplay().HighOffWeed)
                Damage += Randomizer.Next(1, 3);

            return Damage;
        }

        /// <summary>
        /// Gets the coins from the users dead body
        /// </summary>
        public int GetCoins(GameClient TargetClient, RoleplayBot Bot = null)
        {
            if (TargetClient != null && TargetClient.GetHabbo() != null)
            {
                if (TargetClient.GetHabbo().VIPRank > 1)
                    return 0;

                if (TargetClient.GetHabbo().Credits < 3)
                    return 0;
            }

            if (Bot != null)
            {
                int MinMoney = Convert.ToInt32(RoleplayData.GetData("bots", "minmoney"));
                int MaxMoney = Convert.ToInt32(RoleplayData.GetData("bots", "maxmoney"));

                if (MaxMoney == 0)
                    return 0;

                CryptoRandom Random = new CryptoRandom();
                return Random.Next(MinMoney, (MaxMoney + 1));
            }

            return TargetClient.GetHabbo().Credits / 3;
        }

        /// <summary>
        /// calculates the amount of exp to give to the client
        /// </summary>
        public int GetEXP(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null)
        {
            int TargetLevel = TargetClient == null ? Bot.Level : TargetClient.GetRoleplay().Level;

            CryptoRandom Random = new CryptoRandom();
            int LevelDifference = Math.Abs(Client.GetRoleplay().Level - TargetLevel);
            int Amount;
            int Bonus;

            if (LevelDifference > 8)
            {
                Amount = 0;
                Bonus = 0;
            }
            else
            {
                if (TargetLevel > Client.GetRoleplay().Level)
                    Bonus = (10 * (LevelDifference + 1)) + LevelDifference * 2 + 5;
                else if (TargetLevel == Client.GetRoleplay().Level)
                    Bonus = (10 * 2) + 3 + 5;
                else if (TargetLevel < Client.GetRoleplay().Level)
                    Bonus = 10 + 5;
                else
                    Bonus = 2 * LevelDifference + 5;

                Amount = Random.Next(20, 20 + (LevelDifference + 9));
            }

            return (Amount + Bonus + 18);
        }

        /// <summary>
        /// Gets the rewards from the dead body
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="TargetClient"></param>
        /// <param name="Bot"></param>
        public void GetRewards(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null)
        {
            if (Bot == null)
            {
                if (Client.GetRoleplay().LastKilled != TargetClient.GetHabbo().Id)
                {
                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.KILL_USER);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Kills", 1);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(TargetClient, "ACH_Death", 1);

                    Client.GetRoleplay().LastKilled = TargetClient.GetHabbo().Id;
                    Client.GetRoleplay().Kills++;
                    Client.GetRoleplay().HitKills++;

                    if (GroupManager.HasJobCommand(TargetClient, "guide") && TargetClient.GetRoleplay().IsWorking)
                        TargetClient.GetRoleplay().CopDeaths++;
                    else
                        TargetClient.GetRoleplay().Deaths++;

                    if (!Client.GetRoleplay().WantedFor.Contains("murder"))
                        Client.GetRoleplay().WantedFor = Client.GetRoleplay().WantedFor + "murder, ";

                    CryptoRandom Random = new CryptoRandom();
                    int Multiplier = 1;

                    int Chance = Random.Next(1, 101);

                    if (Chance <= 16)
                    {
                        if (Chance <= 8)
                            Multiplier = 3;
                        else
                            Multiplier = 2;
                    }

                    LevelManager.AddLevelEXP(Client, GetEXP(Client, TargetClient) * Multiplier);

                    Group Gang = GroupManager.GetGang(Client.GetRoleplay().GangId);
                    Group TarGetGang = GroupManager.GetGang(TargetClient.GetRoleplay().GangId);

                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        if (Gang != null)
                        {
                            if (Gang.Id > 1000)
                            {
                                int ScoreIncrease = Random.Next(1, 11);

                                Gang.GangKills++;
                                Gang.GangScore += ScoreIncrease;

                                dbClient.RunQuery("UPDATE `rp_gangs` SET `gang_kills` = '" + Gang.GangKills + "', `gang_score` = '" + Gang.GangScore + "' WHERE `id` = '" + Gang.Id + "'");
                            }
                        }
                        if (TarGetGang != null)
                        {
                            if (TarGetGang.Id > 1000)
                            {
                                TarGetGang.GangDeaths++;

                                dbClient.RunQuery("UPDATE `rp_gangs` SET `gang_deaths` = '" + TarGetGang.GangDeaths + "' WHERE `id` = '" + TarGetGang.Id + "'");
                            }
                        }
                    }
                }
            }
            else
            {
                int Multiplier = 1;
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.KILL_USER);
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Kills", 1);

                Client.GetRoleplay().Kills++;
                Client.GetRoleplay().HitKills++;

                CryptoRandom Random = new CryptoRandom();


                int Chance = Random.Next(1, 101);

                if (Chance <= 16)
                {
                    if (Chance <= 8)
                        Multiplier = 3;
                    else
                        Multiplier = 2;
                }

                LevelManager.AddLevelEXP(Client, CombatManager.GetCombatType("fist").GetEXP(Client, null, Bot) * Multiplier);
            }
        }
    }
}
