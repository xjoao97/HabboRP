using System;
using System.Data;
using Plus.HabboHotel.GameClients;
using Plus.Database.Interfaces;
using log4net;

namespace Plus.HabboRoleplay.RoleplayUsers
{
    public class UserDataHandler
    {
        /// <summary>
        /// Log mechanism
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("UserDataHandler");

        /// <summary>
        /// The users session
        /// </summary>
        GameClient Client;

        /// <summary>
        /// The users roleplay instance
        /// </summary>
        RoleplayUser RoleplayUser;

        /// <summary>
        /// Constructs the class
        /// </summary>
        public UserDataHandler(GameClient Client, RoleplayUser RoleplayUser)
        {
            this.Client = Client;
            this.RoleplayUser = RoleplayUser;
        }

        /// <summary>
        /// Saves all rp data for the user to the db
        /// </summary>
        /// <returns></returns>
        public bool SaveData()
        {
            if (Client == null)
                return false;

            if (Client.GetHabbo() == null)
                return false;

            if (Client.GetRoleplay() == null)
                return false;

            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery(GetQueryString());
                AddParameters(DB);

                DB.RunQuery();
            }
            return true;
        }

        /// <summary>
        /// Saves all cooldown for the user to the db
        /// </summary>
        /// <returns></returns>
        public bool SaveCooldownData()
        {
            if (Client == null || RoleplayUser == null)
                return false;

            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                try
                {
                    DB.SetQuery(GetCooldownQueryString());
                    AddCooldownParameters(DB);

                    DB.RunQuery();
                }
                catch (Exception Ex)
                { log.Info("ERROR WHILE TRYING TO SAVE COOLDOWN USER DATA! EXCEPTION: " + Ex.Message); }
            }
            return true;
        }

        /// <summary>
        /// Saves all farming stats for the user to the db
        /// </summary>
        /// <returns></returns>
        public bool SaveFarmingData()
        {
            if (Client == null || RoleplayUser == null)
                return false;

            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                try
                {
                    DB.SetQuery(GetFarmingQueryString());
                    AddFarmingParameters(DB);

                    DB.RunQuery();
                }
                catch (Exception Ex)
                { log.Info("ERROR WHILE TRYING TO SAVE FARMING USER DATA! EXCEPTION: " + Ex.Message); }
            }
            return true;
        }

        /// <summary>
        /// Gets the query string to update the user details
        /// </summary>
        private string GetQueryString()
        {
            string Query = @"UPDATE rp_stats SET 
                                        level = @level,
                                        level_exp = @exp,

                                        job_id = @jobid,
                                        job_rank = @jobrank,
                                        job_request = @jobrequest,

                                        maxhealth = @maxhealth, 
                                        curhealth = @curhealth,
                                        maxenergy = @maxenergy, 
                                        curenergy = @curenergy,
                                        hunger = @hunger,
                                        hygiene = @hygiene,

                                        intelligence = @intelligence, 
                                        strength = @strength,
                                        stamina = @stamina,

                                        intelligence_exp = @intelligence_exp, 
                                        strength_exp = @strength_exp,
                                        stamina_exp = @stamina_exp,

                                        is_dead = @is_dead,
                                        dead_time_left = @dead_time_left,
                                        is_jailed = @is_jailed,
                                        jailed_time_left = @jailed_time_left,
                                        is_wanted = @is_wanted,
                                        wanted_level = @wanted_level,
                                        wanted_time_left = @wanted_time_left,
                                        on_probation = @on_probation,
                                        probation_time_left = @probation_time_left,
                                        sendhome_time_left = @sendhome_time_left,
                                        is_cuffed = @is_cuffed,
                                        cuffed_time_left = @cuffed_time_left,

                                        last_killed = @last_killed,
                                        married_to = @married,

                                        gang_id = @gangid,
                                        gang_rank = @gangrank,
                                        gang_request = @gangrequest,

                                        punches = @punches,
                                        kills = @kills,
                                        hit_kills = @hit_kills,
                                        gun_kills = @gun_kills,
                                        deaths = @deaths,
                                        cop_deaths = @cop_deaths,
                                        time_worked = @time_worked,
                                        arrests = @arrests,
                                        arrested = @arrested,
                                        evasions = @evasions,

                                        bank_account = @bank_account,
                                        bank_chequings = @bank_chequings,
                                        bank_savings = @bank_savings,
                                        
                                        phone = @phone,
                                        phone_apps = @phone_apps,
                                        car = @car,
                                        car_fuel = @car_fuel,
                                        cigarette = @cigarette,
                                        weed = @weed,
                                        cocaine = @cocaine,
                                        bullets = @bullets,
                                        dynamite = @dynamite,

                                        brawl_wins = @brawl_wins,
                                        soloqueue_wins = @soloqueue_wins,
                                        cw_wins = @cw_wins,
                                        mw_wins = @mw_wins,

                                        is_noob = @is_noob,
                                        noob_time_left = @noob_time_left,
                                        
                                        vip_banned = @vip_banned,
                                        wchat_banned = @wchat_banned,
                                        wchat_making_banned = @wchat_making_banned,
                                        last_coordinates = @last_coordinates
                            
                                        WHERE id = @userid";
            return Query;
        }

        /// <summary>
        /// Gets the group query string to update the cooldown details
        /// </summary>
        private string GetCooldownQueryString()
        {
            string Query = @"UPDATE rp_stats_cooldowns SET 
                                        robbery = @robbery
                                        
                                        WHERE id = @userid";
            return Query;
        }

        /// <summary>
        /// Gets the group query string to update the farming stats
        /// </summary>
        private string GetFarmingQueryString()
        {
            string Query = @"UPDATE rp_stats_farming SET 
                                        level = @level,
                                        exp = @exp,
                                        
                                        has_seed_satchel = @has_seed_satchel,
                                        has_plant_satchel = @has_plant_satchel,

                                        blue_starflower = @blue_starflower,
                                        yellow_starflower = @yellow_starflower,
                                        pink_dahlia = @pink_dahlia,
                                        yellow_plumeria = @yellow_plumeria,
                                        pink_primrose = @pink_primrose,
                                        blue_primrose = @blue_primrose,
                                        yellow_primrose = @yellow_primrose,
                                        yellow_dahlia = @yellow_dahlia,
                                        blue_plumeria = @blue_plumeria,
                                        pink_plumeria = @pink_plumeria,
                                        red_starflower = @red_starflower,
                                        blue_dahlia = @blue_dahlia
                                        
                                        WHERE id = @userid";
            return Query;
        }

        /// <summary>
        /// Adds the farming parameters to the mysql command
        /// </summary>
        private void AddFarmingParameters(IQueryAdapter DB)
        {
            DB.AddParameter("userid", Client.GetHabbo().Id);

            DB.AddParameter("level", Client.GetRoleplay().FarmingStats.Level);
            DB.AddParameter("exp", Client.GetRoleplay().FarmingStats.Exp);

            DB.AddParameter("has_seed_satchel", PlusEnvironment.BoolToEnum(Client.GetRoleplay().FarmingStats.HasSeedSatchel));
            DB.AddParameter("has_plant_satchel", PlusEnvironment.BoolToEnum(Client.GetRoleplay().FarmingStats.HasPlantSatchel));

            DB.AddParameter("blue_starflower", Client.GetRoleplay().FarmingStats.SeedSatchel.BlueStarflowerSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.BlueStarflowers);
            DB.AddParameter("yellow_starflower", Client.GetRoleplay().FarmingStats.SeedSatchel.YellowStarflowerSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.YellowStarflowers);
            DB.AddParameter("pink_dahlia", Client.GetRoleplay().FarmingStats.SeedSatchel.PinkDahliaSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.PinkDahlias);
            DB.AddParameter("yellow_plumeria", Client.GetRoleplay().FarmingStats.SeedSatchel.YellowPlumeriaSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.YellowPlumerias);
            DB.AddParameter("pink_primrose", Client.GetRoleplay().FarmingStats.SeedSatchel.PinkPrimroseSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.PinkPrimroses);
            DB.AddParameter("blue_primrose", Client.GetRoleplay().FarmingStats.SeedSatchel.BluePrimroseSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.BluePrimroses);
            DB.AddParameter("yellow_primrose", Client.GetRoleplay().FarmingStats.SeedSatchel.YellowPrimroseSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.YellowPrimroses);
            DB.AddParameter("yellow_dahlia", Client.GetRoleplay().FarmingStats.SeedSatchel.YellowDahliaSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.YellowDahlias);
            DB.AddParameter("blue_plumeria", Client.GetRoleplay().FarmingStats.SeedSatchel.BluePlumeriaSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.BluePlumerias);
            DB.AddParameter("pink_plumeria", Client.GetRoleplay().FarmingStats.SeedSatchel.PinkPlumeriaSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.PinkPlumerias);
            DB.AddParameter("red_starflower", Client.GetRoleplay().FarmingStats.SeedSatchel.RedStarflowerSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.RedStarflowers);
            DB.AddParameter("blue_dahlia", Client.GetRoleplay().FarmingStats.SeedSatchel.BlueDahliaSeeds + ":" + Client.GetRoleplay().FarmingStats.PlantSatchel.BlueDahlias);
        }

        /// <summary>
        /// Adds the cooldown parameters to the mysql command
        /// </summary>
        private void AddCooldownParameters(IQueryAdapter DB)
        {
            DB.AddParameter("userid", Client.GetHabbo().Id);

            DB.AddParameter("robbery", Client.GetRoleplay().SpecialCooldowns["robbery"]);
        }

        /// <summary>
        /// Adds the parameters to the mysql command
        /// </summary>
        private void AddParameters(IQueryAdapter DB)
        {
            // User ID
            DB.AddParameter("userid", Client.GetHabbo().Id);

            // Basic Info
            DB.AddParameter("level", RoleplayUser.Level);
            DB.AddParameter("exp", RoleplayUser.LevelEXP);

            // Job Info
            DB.AddParameter("jobid", RoleplayUser.JobId);
            DB.AddParameter("jobrank", RoleplayUser.JobRank);
            DB.AddParameter("jobrequest", RoleplayUser.JobRequest);

            // Human Needs
            DB.AddParameter("maxhealth", RoleplayUser.MaxHealth);
            DB.AddParameter("curhealth", RoleplayUser.CurHealth);
            DB.AddParameter("maxenergy", RoleplayUser.MaxEnergy);
            DB.AddParameter("curenergy", RoleplayUser.CurEnergy);
            DB.AddParameter("hunger", RoleplayUser.Hunger);
            DB.AddParameter("hygiene", RoleplayUser.Hygiene);

            // Levelable Stats
            DB.AddParameter("intelligence", RoleplayUser.Intelligence);
            DB.AddParameter("strength", RoleplayUser.Strength);
            DB.AddParameter("stamina", RoleplayUser.Stamina);

            // Extra Variables for Levelable Stats
            DB.AddParameter("intelligence_exp", RoleplayUser.IntelligenceEXP);
            DB.AddParameter("strength_exp", RoleplayUser.StrengthEXP);
            DB.AddParameter("stamina_exp", RoleplayUser.StaminaEXP);

            // Jailed/Dead - Wanted/Probation - Sendhome
            DB.AddParameter("is_dead", PlusEnvironment.BoolToEnum(RoleplayUser.IsDead));
            DB.AddParameter("dead_time_left", RoleplayUser.DeadTimeLeft);
            DB.AddParameter("is_jailed", PlusEnvironment.BoolToEnum(RoleplayUser.IsJailed));
            DB.AddParameter("jailed_time_left", RoleplayUser.JailedTimeLeft);
            DB.AddParameter("is_wanted", PlusEnvironment.BoolToEnum(RoleplayUser.IsWanted));
            DB.AddParameter("wanted_level", RoleplayUser.WantedLevel);
            DB.AddParameter("wanted_time_left", RoleplayUser.WantedTimeLeft);
            DB.AddParameter("on_probation", PlusEnvironment.BoolToEnum(RoleplayUser.OnProbation));
            DB.AddParameter("probation_time_left", RoleplayUser.ProbationTimeLeft);
            DB.AddParameter("sendhome_time_left", RoleplayUser.SendHomeTimeLeft);
            DB.AddParameter("is_cuffed", PlusEnvironment.BoolToEnum(RoleplayUser.Cuffed));
            DB.AddParameter("cuffed_time_left", RoleplayUser.CuffedTimeLeft);

            // Affiliations
            DB.AddParameter("last_killed", RoleplayUser.LastKilled);
            DB.AddParameter("married", RoleplayUser.MarriedTo);

            // Gang Info
            DB.AddParameter("gangid", RoleplayUser.GangId);
            DB.AddParameter("gangrank", RoleplayUser.GangRank);
            DB.AddParameter("gangrequest", RoleplayUser.GangRequest);

            // Statistics
            DB.AddParameter("punches", RoleplayUser.Punches);
            DB.AddParameter("kills", RoleplayUser.Kills);
            DB.AddParameter("hit_kills", RoleplayUser.HitKills);
            DB.AddParameter("gun_kills", RoleplayUser.GunKills);
            DB.AddParameter("deaths", RoleplayUser.Deaths);
            DB.AddParameter("cop_deaths", RoleplayUser.CopDeaths);
            DB.AddParameter("time_worked", RoleplayUser.TimeWorked);
            DB.AddParameter("arrests", RoleplayUser.Arrests);
            DB.AddParameter("arrested", RoleplayUser.Arrested);
            DB.AddParameter("evasions", RoleplayUser.Evasions);

            // Banking
            DB.AddParameter("bank_account", RoleplayUser.BankAccount);
            DB.AddParameter("bank_chequings", RoleplayUser.BankChequings);
            DB.AddParameter("bank_savings", RoleplayUser.BankSavings);

            // Inventory
            DB.AddParameter("phone", RoleplayUser.PhoneType);
            DB.AddParameter("phone_apps", (RoleplayUser.PhoneApps.Count > 1) ? String.Join(":", RoleplayUser.PhoneApps) : Convert.ToString(RoleplayUser.PhoneApps[0]));
            DB.AddParameter("car", RoleplayUser.CarType);
            DB.AddParameter("car_fuel", RoleplayUser.CarFuel);
            DB.AddParameter("weed", RoleplayUser.Weed);
            DB.AddParameter("cocaine", RoleplayUser.Cocaine);
            DB.AddParameter("cigarette", RoleplayUser.Cigarettes);
            DB.AddParameter("bullets", RoleplayUser.Bullets);
            DB.AddParameter("dynamite", RoleplayUser.Dynamite);

            // Minigames
            DB.AddParameter("brawl_wins", RoleplayUser.BrawlWins);
            DB.AddParameter("cw_wins", RoleplayUser.CwWins);
            DB.AddParameter("mw_wins", RoleplayUser.MwWins);
            DB.AddParameter("soloqueue_wins", RoleplayUser.SoloQueueWins);

            // Newcomer Misc
            DB.AddParameter("is_noob", PlusEnvironment.BoolToEnum(RoleplayUser.IsNoob));
            DB.AddParameter("noob_time_left", RoleplayUser.NoobTimeLeft);

            // Uncategorized
            DB.AddParameter("vip_banned", RoleplayUser.VIPBanned);
            DB.AddParameter("wchat_banned", PlusEnvironment.BoolToEnum(RoleplayUser.BannedFromChatting));
            DB.AddParameter("wchat_making_banned", PlusEnvironment.BoolToEnum(RoleplayUser.BannedFromMakingChat));

            DB.AddParameter("last_coordinates", RoleplayUser.LastCoordinates);
            
        }
    }
}