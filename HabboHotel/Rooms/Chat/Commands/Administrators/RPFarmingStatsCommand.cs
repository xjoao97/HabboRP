using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Farming;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class RPFarmingStatsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_rpfarmingstats"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Fornece uma lista das metas estatísticas agrícolas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Você esqueceu de inserir o usuário deseja verificar!", 1);
                return;
            }

            #region Variables
            int Level;
            int Exp;

            bool HasSeedSatchel;
            bool HasPlantSatchel;

            int BlueStarflowerSeeds;
            int YellowStarflowerSeeds;
            int PinkDahliaSeeds;
            int YellowPlumeriaSeeds;
            int PinkPrimroseSeeds;
            int BluePrimroseSeeds;
            int YellowPrimroseSeeds;
            int YellowDahliaSeeds;
            int BluePlumeriaSeeds;
            int PinkPlumeriaSeeds;
            int RedStarflowerSeeds;
            int BlueDahliaSeeds;

            int BlueStarflowers;
            int YellowStarflowers;
            int PinkDahlias;
            int YellowPlumerias;
            int PinkPrimroses;
            int BluePrimroses;
            int YellowPrimroses;
            int YellowDahlias;
            int BluePlumerias;
            int PinkPlumerias;
            int RedStarflowers;
            int BlueDahlias;

            string Username = Params[1];
            GameClients.GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            #endregion

            #region Variables Client Check & Set
            if (TargetClient == null)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `users` where `username` = '" + Username + "' LIMIT 1");
                    var Row = dbClient.getRow();

                    if (Row == null)
                    {
                        Session.SendWhisper("Esta pessoa não existe!", 1);
                        return;
                    }

                    int UserId = Convert.ToInt32(Row["id"]);

                    dbClient.SetQuery("SELECT * FROM `rp_stats_farming` where `id` = '" + UserId + "' LIMIT 1");
                    var Stats = dbClient.getRow();

                    if (Stats == null)
                    {
                        Session.SendWhisper("Esta pessoa não existe!", 1);
                        return;
                    }

                    Level = Convert.ToInt32(Stats["level"]);
                    Exp = Convert.ToInt32(Stats["level_exp"]);

                    HasSeedSatchel = PlusEnvironment.EnumToBool(Row["has_seed_satchel"].ToString());
                    HasPlantSatchel = PlusEnvironment.EnumToBool(Row["has_plant_satchel"].ToString());

                    BlueStarflowerSeeds = Convert.ToInt32(Row["blue_starflower"].ToString().Split(':')[0]);
                    YellowStarflowerSeeds = Convert.ToInt32(Row["yellow_starflower"].ToString().Split(':')[0]);
                    PinkDahliaSeeds = Convert.ToInt32(Row["pink_dahlia"].ToString().Split(':')[0]);
                    YellowPlumeriaSeeds = Convert.ToInt32(Row["yellow_plumeria"].ToString().Split(':')[0]);
                    PinkPrimroseSeeds = Convert.ToInt32(Row["pink_primrose"].ToString().Split(':')[0]);
                    BluePrimroseSeeds = Convert.ToInt32(Row["blue_primrose"].ToString().Split(':')[0]);
                    YellowPrimroseSeeds = Convert.ToInt32(Row["yellow_primrose"].ToString().Split(':')[0]);
                    YellowDahliaSeeds = Convert.ToInt32(Row["yellow_dahlia"].ToString().Split(':')[0]);
                    BluePlumeriaSeeds = Convert.ToInt32(Row["blue_plumeria"].ToString().Split(':')[0]);
                    PinkPlumeriaSeeds = Convert.ToInt32(Row["pink_plumeria"].ToString().Split(':')[0]);
                    RedStarflowerSeeds = Convert.ToInt32(Row["red_starflower"].ToString().Split(':')[0]);
                    BlueDahliaSeeds = Convert.ToInt32(Row["blue_dahlia"].ToString().Split(':')[0]);

                    BlueStarflowers = Convert.ToInt32(Row["blue_starflower"].ToString().Split(':')[1]);
                    YellowStarflowers = Convert.ToInt32(Row["yellow_starflower"].ToString().Split(':')[1]);
                    PinkDahlias = Convert.ToInt32(Row["pink_dahlia"].ToString().Split(':')[1]);
                    YellowPlumerias = Convert.ToInt32(Row["yellow_plumeria"].ToString().Split(':')[1]);
                    PinkPrimroses = Convert.ToInt32(Row["pink_primrose"].ToString().Split(':')[1]);
                    BluePrimroses = Convert.ToInt32(Row["blue_primrose"].ToString().Split(':')[1]);
                    YellowPrimroses = Convert.ToInt32(Row["yellow_primrose"].ToString().Split(':')[1]);
                    YellowDahlias = Convert.ToInt32(Row["yellow_dahlia"].ToString().Split(':')[1]);
                    BluePlumerias = Convert.ToInt32(Row["blue_plumeria"].ToString().Split(':')[1]);
                    PinkPlumerias = Convert.ToInt32(Row["pink_plumeria"].ToString().Split(':')[1]);
                    RedStarflowers = Convert.ToInt32(Row["red_starflower"].ToString().Split(':')[1]);
                    BlueDahlias = Convert.ToInt32(Row["blue_dahlia"].ToString().Split(':')[1]);
                }
            }
            else
            {
                Level = TargetClient.GetRoleplay().FarmingStats.Level;
                Exp = TargetClient.GetRoleplay().FarmingStats.Exp;

                HasSeedSatchel = TargetClient.GetRoleplay().FarmingStats.HasSeedSatchel;
                HasPlantSatchel = TargetClient.GetRoleplay().FarmingStats.HasPlantSatchel;

                BlueStarflowerSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.BlueStarflowerSeeds;
                YellowStarflowerSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.YellowStarflowerSeeds;
                PinkDahliaSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.PinkDahliaSeeds;
                YellowPlumeriaSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.YellowPlumeriaSeeds;
                PinkPrimroseSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.PinkPrimroseSeeds;
                BluePrimroseSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.BluePrimroseSeeds;
                YellowPrimroseSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.YellowPrimroseSeeds;
                YellowDahliaSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.YellowDahliaSeeds;
                BluePlumeriaSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.BluePlumeriaSeeds;
                PinkPlumeriaSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.PinkPlumeriaSeeds;
                RedStarflowerSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.RedStarflowerSeeds;
                BlueDahliaSeeds = TargetClient.GetRoleplay().FarmingStats.SeedSatchel.BlueDahliaSeeds;

                BlueStarflowers = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.BlueStarflowers;
                YellowStarflowers = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.YellowStarflowers;
                PinkDahlias = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.PinkDahlias;
                YellowPlumerias = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.YellowPlumerias;
                PinkPrimroses = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.PinkPrimroses;
                BluePrimroses = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.BluePrimroses;
                YellowPrimroses = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.YellowPrimroses;
                YellowDahlias = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.YellowDahlias;
                BluePlumerias = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.BluePlumerias;
                PinkPlumerias = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.PinkPlumerias;
                RedStarflowers = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.RedStarflowers;
                BlueDahlias = TargetClient.GetRoleplay().FarmingStats.PlantSatchel.BlueDahlias;
            }
            #endregion

            StringBuilder MessageToSend = new StringBuilder().Append(
                                   "<---------- " + Username + " - Estatísticas agrícolas ---------->\n\n" +

                                   "<----- Informações Básicas ----->\n" +
                                   "Level: " + Level + "/" + RoleplayManager.FarmingLevelCap + "\n" +
                                   "Level XP: " + Exp + "/" + (!FarmingManager.levels.ContainsKey(Level + 1) ? 100000 : FarmingManager.levels[Level + 1]) + "\n\n" +

                                   "<----- Saco de Sementes ----->\n" +
                                   (HasSeedSatchel ? ("Flor Azul: " + BlueStarflowerSeeds + "\n" +
                                   "Flor Amarela: " + YellowStarflowerSeeds + "\n" +
                                   "Flor Vermelha: " + RedStarflowerSeeds + "\n" +
                                   "Dalia Azul: " + BlueDahliaSeeds + "\n" +
                                   "Dalia Amarela: " + YellowDahliaSeeds + "\n" +
                                   "Dalia Rosa: " + PinkDahliaSeeds + "\n" +
                                   "Rosa Azul " + BluePrimroseSeeds + "\n" +
                                   "Rosa Amarela: " + YellowPrimroseSeeds + "\n" +
                                   "Rosa Rosa: " + PinkPrimroseSeeds + "\n" +
                                   "Plumeria Azul: " + BluePlumeriaSeeds + "\n" +
                                   "Plumeria Amarela: " + YellowPlumeriaSeeds + "\n" +
                                   "Plumeria Rosa: " + PinkPlumeriaSeeds + "\n\n") : "Este usuário não possui um Saco de Sementes!\n\n") +

                                   "<----- Saco de Plantas ----->\n" +
                                   (HasPlantSatchel ? ("Flor Azul: " + BlueStarflowers + "\n" +
                                   "Flor Amarela: " + YellowStarflowers + "\n" +
                                   "Flor Vermelha: " + RedStarflowers + "\n" +
                                   "Dalia Azul: " + BlueDahlias + "\n" +
                                   "Dalia Amarela: " + YellowDahlias + "\n" +
                                   "Dalia Rosa: " + PinkDahlias + "\n" +
                                   "Rosa Azul " + BluePrimroses + "\n" +
                                   "Rosa Amarela: " + YellowPrimroses + "\n" +
                                   "Rosa Rosa: " + PinkPrimroses + "\n" +
                                   "lumeria Azul: " + BluePlumerias + "\n" +
                                   "Plumeria Amarela: " + YellowPlumerias + "\n" +
                                   "Plumeria Rosa: " + PinkPlumerias + "\n\n") : "Este usuário não possui um Saco de Plantas!\n\n"));

            Session.SendMessage(new MOTDNotificationComposer(MessageToSend.ToString()));
        }
    }
}