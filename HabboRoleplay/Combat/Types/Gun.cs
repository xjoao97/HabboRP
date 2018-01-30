using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using Plus.Utilities;

using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

/*
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
*/

namespace Plus.HabboRoleplay.Combat.Types
{
    public class Gun : ICombat
    {
        /// <summary>
        /// Executes this type of combat
        /// </summary>
        public void Execute(GameClient Client, GameClient TargetClient, bool HitClosest = false)
        {
            if (!CanCombat(Client, TargetClient))
                return;

            #region Variables

            RoomUser RoomUser = Client.GetRoomUser();
            RoomUser TargetRoomUser = TargetClient.GetRoomUser();
            int Damage = GetDamage(Client, TargetClient);
            Weapon Weapon = Client.GetRoleplay().EquippedWeapon;
            Point ClientPos = RoomUser.Coordinate;
            Point TargetClientPos = TargetRoomUser.Coordinate;

            #endregion

            #region Ammo Check
            if (Client.GetRoleplay().GunShots >= Weapon.ClipSize)
            {
                Weapon.Reload(Client, TargetClient);
                Client.GetRoleplay().CooldownManager.CreateCooldown("reload", 1000, Weapon.ReloadTime);
                return;
            }
            #endregion

            #region Distance Check
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);
            if (Distance > Weapon.Range)
            {
                RoleplayManager.Shout(Client, "*Tenta acertar o tiro em " + TargetClient.GetHabbo().Username + ", mas erra o alvo*", 4);
                Client.GetRoleplay().GunShots++;

                if (Client.GetRoleplay().Game == null)
                    Client.GetRoleplay().Bullets--;
                return;
            }
            #endregion

            #region Target Death Procedure
            if (TargetClient.GetRoleplay().CurHealth - Damage <= 0)
            {
                Client.GetRoleplay().ClearWebSocketDialogue();

                string Text = Weapon.FiringText.Split(':')[1];
                string GunName = Weapon.PublicName;

                RoleplayManager.Shout(Client, FormatFiringText(Text, GunName, TargetClient.GetHabbo().Username, Damage, Weapon.Energy), 6);
				
				
                lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                if (client == null || client.GetHabbo() == null)
                continue;
                
                client.SendMessage(new RoomNotificationComposer("staff_notice", "message", "[Notícia Urgente] " + Client.GetHabbo().Username + " matou com tiros o cidadão " + TargetClient.GetHabbo().Username + ", tome cuidado pelas ruas!"));
                }
            }

                if (Client.GetRoleplay().LastKilled != TargetClient.GetHabbo().Id && TargetClient.GetRoleplay().Game == null)
                {
                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.KILL_USER);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Kills", 1);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(TargetClient, "ACH_Death", 1);

                    #region Player Stats
                    Client.GetRoleplay().LastKilled = TargetClient.GetHabbo().Id;
                    Client.GetRoleplay().Kills++;
                    Client.GetRoleplay().GunKills++;

                    if (GroupManager.HasJobCommand(TargetClient, "guide") && TargetClient.GetRoleplay().IsWorking)
                        TargetClient.GetRoleplay().CopDeaths++;
                    else
                        TargetClient.GetRoleplay().Deaths++;

                    if (!Client.GetRoleplay().WantedFor.Contains("cometer assassinato"))
                        Client.GetRoleplay().WantedFor = Client.GetRoleplay().WantedFor + "cometer assassinato, ";
                    #endregion

                    #region Exp Calculator
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
                    #endregion

                    #region Gang Stats 
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
                    #endregion

                    BountyManager.CheckBounty(Client, TargetClient.GetHabbo().Id);

                    if ((TargetClient.GetRoleplay().CurHealth - Damage) <= 0)
                        TargetClient.GetRoleplay().CurHealth = 0;
                }             
            }
            #endregion

            #region Target Damage Procedure (Did not die)
            else
            {
                string Text = Weapon.FiringText.Split(':')[0];
                string GunName = Weapon.PublicName;
                Client.GetRoleplay().OpenUsersDialogue(TargetClient);
                TargetClient.GetRoleplay().OpenUsersDialogue(Client);

                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.SHOOT_USER);
                RoleplayManager.Shout(Client, FormatFiringText(Text, GunName, TargetClient.GetHabbo().Username, Damage, Weapon.Energy), 6);
            }
            #endregion
            
            TargetClient.GetRoleplay().CurHealth -= Damage;

            if (Client.GetRoleplay().Game == null)
                Client.GetRoleplay().Bullets--;

            if (Client.GetRoleplay().Game == null)
                Client.GetRoleplay().CurEnergy -= Weapon.Energy;

            Client.GetRoleplay().GunShots++;

            if (!Client.GetRoleplay().WantedFor.Contains("tentativa de assaltSo + tentativa/assassinato"))
                Client.GetRoleplay().WantedFor = Client.GetRoleplay().WantedFor + "tentativa de assalto + tentativa/assassinato ";
        }

        /// <summary>
        /// Executes this type of combat on a Bot
        /// </summary>
        public void ExecuteBot(GameClient Client, RoleplayBot Bot = null)
        {

        }

        /// <summary>
        /// Checks if a client can complete this action
        /// </summary>
        public bool CanCombat(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null)
        {
            #region Variables
            RoomUser RoomUser = Client.GetRoomUser();
            RoomUser TargetRoomUser = TargetClient.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            #endregion

            #region Cooldown Conditions
            if (Client.GetRoleplay().TryGetCooldown("reload", false))
                return false;

            if (Client.GetRoleplay().TryGetCooldown("gun", false))
                return false;
            #endregion

            #region Main Conditions
            Weapon Weapon = Client.GetRoleplay().EquippedWeapon;
            Room Room = null;

            if (Weapon == null)
                return false;

            if (Client.GetHabbo().CurrentRoomId > 0)
                Room = Client.GetHabbo().CurrentRoom;

            if (Room != null)
            {
                if (Room.SafeZoneEnabled)
                {
                    Client.SendWhisper("Você não pode atirar nessa sala!", 1);
                    return false;
                }

                if (!RoleplayManager.PurgeStarted)
                {
                    if (!Room.ShootEnabled && Client.GetRoleplay().Game == null)
                    {
                        Client.SendWhisper("Você não pode atirar nessa sala!", 1);
                        return false;
                    }
                }
            }

            if (TargetRoomUser == null)
            {
                Client.SendWhisper("Esta pessoa não está na mesma sala que você!", 1);
                return false;
            }

            if (Client.GetRoleplay().Game == null)
            {
                if (RoleplayManager.LevelDifference)
                {
                    if (!Room.TurfEnabled)
                    {
                        int LevelDifference = Math.Abs(Client.GetRoleplay().Level - TargetClient.GetRoleplay().Level);

                        if (LevelDifference > 8)
                        {
                            Client.SendWhisper("Você não pode gravar esse usuário, pois sua diferença de nível é maior do que 8!", 1);
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (Client.GetRoleplay().Game.GetGameMode() != Games.GameMode.SoloQueueGuns)
                {
                    Client.SendWhisper("Você não pode atirar dentro de Eventos " + Client.GetRoleplay().Game.GetName() + "!", 1);
                    return false;
                }

                if (!Client.GetRoleplay().Game.HasGameStarted())
                {
                    Client.SendWhisper("O evento ainda não começou!!", 1);
                    return false;
                }

                if (TargetClient.GetRoleplay().Game != Client.GetRoleplay().Game)
                {
                    Client.SendWhisper("Seu alvo não faz parte desse evento!", 1);
                    return false;
                }
            }

            if (Weapon == null)
            { 
                Client.SendWhisper("Você atualmente não tem nenhuma arma equipada!", 1);
                return false; 
            }
            #endregion

            #region User Conditions

            if (RoomUser == null)
                return false;

            if (RoomUser.Frozen)
            {
                Client.SendWhisper("Você não pode fazer isso enquanto você está atordoado!", 1);
                return false;
            }

            if (RoomUser.IsAsleep)
            {
                Client.SendWhisper("Você não pode fazer em uma pessoa AUS", 1);
                return false;
            }

            if (Client.GetRoleplay().IsDead)
            {
                Client.SendWhisper("Você não pode completar esta ação enquanto está morto!", 1);
                return false;
            }

            if (Client.GetRoleplay().IsWorking)
            {
                Client.SendWhisper("Você não pode completar esta ação enquanto trabalha!", 1);
                return false;
            }

            if (Client.GetRoleplay().StaffOnDuty || Client.GetRoleplay().AmbassadorOnDuty)
            {
                Client.SendWhisper("Você não pode atirar em alguém enquanto está de plantão!", 1);
                return false;
            }

            if (Client.GetRoleplay().Cuffed)
            {
                Client.SendWhisper("Você não pode atirar em um cidadão enquanto você está algemado!", 1);
                return false;
            }

            if (Client.GetRoleplay().DrivingCar)
            {
                Client.SendWhisper("Por favor, pare de dirigir o seu veículo para disparar sua arma!", 1);
                return false;
            }

            if (Client.GetRoleplay().IsJailed)
            {
                Client.SendWhisper("Você não pode completar esta ação enquanto você está na prisão!", 1);
                return false;
            }

            if (Client.GetRoleplay().IsNoob)
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

            if (Client.GetRoleplay().Bullets <= 0 && Client.GetRoleplay().Game == null)
            {
                Client.SendWhisper("Você não pode completar esta ação, você está sem balas!", 1);
                return false;
            }

            if (Client.GetRoleplay().CurEnergy <= 0 && Client.GetRoleplay().Game == null)
            {
                Client.SendWhisper("Você não pode completar esta ação, você está sem energia!", 1);
                return false;
            }
            #endregion

            #region Target Conditions
            if (TargetClient == Client)
            {
                Client.SendWhisper("Você não pode se atirar!", 1);
                return false;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Client.SendWhisper("Você não pode atirar em alguém que está morto!", 1);
                return false;
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Client.SendWhisper("Você não pode atirar em alguém que está na prisão!", 1);
                return false;
            }

            if (TargetClient.GetRoleplay().StaffOnDuty)
            {
                Client.SendWhisper("Você não pode atirar em um funcionário que esteja de plantão!", 1);
                return false;
            }

            if (TargetClient.GetRoleplay().AmbassadorOnDuty)
            {
                Client.SendWhisper("Você não pode atirar em um embaixador que está de plantão!", 1);
                return false;
            }

            if (TargetClient.GetRoomUser().IsAsleep)
            {
                Client.SendWhisper("Você não pode atirar em alguém que está ausente!", 1);
                return false;
            }

            if (TargetClient.GetRoleplay().Level < 2)
            {
                Client.SendWhisper("Você não pode completar esta ação porque o usuário é Nível 1!", 1);
                return false;
            }
			
            if (Client.GetRoleplay().Level < 2)
            {
                Client.SendWhisper("Você não pode completar esta ação, pois você ainda é Nível 1!!", 1);
                return false;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Gets the damage
        /// </summary>
        private int GetDamage(GameClient Client, GameClient TargetClient)
        {
            CryptoRandom Randomizer = new CryptoRandom();
            Weapon Weapon = Client.GetRoleplay().EquippedWeapon;

            int MinDamage = Weapon.MinDamage;
            int MaxDamage = Weapon.MaxDamage;

            int Damage = Randomizer.Next(MinDamage, MaxDamage);

            if (Client.GetRoleplay().Class.ToLower() == "gunner")
                Damage += Randomizer.Next(1, 3);

            if (Client.GetRoleplay().GangId > 1000)
            {
                if (GroupManager.HasGangCommand(Client, "gunner"))
                {
                    if (RoleplayManager.GenerateRoom(Client.GetHabbo().CurrentRoomId, false).TurfEnabled || GroupManager.HasJobCommand(TargetClient, "guide"))
                        Damage += Randomizer.Next(0, 2);
                }
            }

            return Damage;
        }

        /// <summary>
        /// Formats the string
        /// </summary>
        private string FormatFiringText(string Text, string GunName, string TargetName, int Damage, int Energy)
        {
            Text = Text.Replace("[NAME]", GunName);
            Text = Text.Replace("[TARGET]", TargetName);
            Text = Text.Replace("[DAMAGE]", Damage.ToString());
            Text = Text.Replace("[ENERGY]", Energy.ToString());

            return Text;
        }

        /// <summary>
        /// calculates the amount of exp to give to the client
        /// </summary>
        public int GetEXP(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null)
        {
            CryptoRandom Random = new CryptoRandom();
            int LevelDifference = Math.Abs(Client.GetRoleplay().Level - TargetClient.GetRoleplay().Level);
            int Amount;
            int Bonus;

            if (LevelDifference > 8)
            {
                Amount = 0;
                Bonus = 0;
            }
            else
            {
                if (TargetClient.GetRoleplay().Level > Client.GetRoleplay().Level)
                    Bonus = (10 * (LevelDifference + 1)) + LevelDifference * 2;
                else if (TargetClient.GetRoleplay().Level == Client.GetRoleplay().Level)
                    Bonus = (10 * 2) + 3;
                else if (TargetClient.GetRoleplay().Level < Client.GetRoleplay().Level)
                    Bonus = 10;
                else
                    Bonus = 2 * LevelDifference;

                Amount = Random.Next(10, 10 + (LevelDifference + 5));
            }

            return (Amount + Bonus + 15);
        }

        /// <summary>
        /// Gets the coins from the users dead body
        /// </summary>
        public int GetCoins(GameClient TargetClient, RoleplayBot Bot = null)
        {
            return 0;
        }

        /// <summary>
        /// Gets the rewards from the dead body
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="TargetClient"></param>
        /// <param name="Bot"></param>
        public void GetRewards(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null)
        {
            
        }

    }
}
