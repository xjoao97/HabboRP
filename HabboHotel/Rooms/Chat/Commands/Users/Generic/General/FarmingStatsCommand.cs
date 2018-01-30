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
    class FarmingStatsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_farming_stats"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Fornece uma lista das suas estatísticas agrícolas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder MessageToSend = new StringBuilder().Append(
                                   "<---------- Suas estatísticas agrícolas ---------->\n\n" +

                                   "<----- Informações [Básicas] ----->\n" +
                                   "Level: " + Session.GetRoleplay().FarmingStats.Level + "/" + RoleplayManager.FarmingLevelCap + "\n" +
                                   "Level XP: " + Session.GetRoleplay().FarmingStats.Exp + "/" + (!FarmingManager.levels.ContainsKey(Session.GetRoleplay().FarmingStats.Level + 1) ? 100000 : FarmingManager.levels[Session.GetRoleplay().FarmingStats.Level + 1]) + "\n\n" +

                                   "<----- [Informações [Saco de Sementes] ----->\n" +
                                   (Session.GetRoleplay().FarmingStats.HasSeedSatchel ? ("[1] Plumeria Amarela: " + Session.GetRoleplay().FarmingStats.SeedSatchel.YellowPlumeriaSeeds + "\n" +
                                   "[2] Plumeria Azul: " + Session.GetRoleplay().FarmingStats.SeedSatchel.BluePlumeriaSeeds + "\n" +
                                   "[3] Plumeria Rosa: " + Session.GetRoleplay().FarmingStats.SeedSatchel.PinkPlumeriaSeeds + "\n" +
                                   "[4] Rosa Amarela: " + Session.GetRoleplay().FarmingStats.SeedSatchel.YellowPrimroseSeeds + "\n" +
                                   "[5] Rosa Azul: " + Session.GetRoleplay().FarmingStats.SeedSatchel.BluePrimroseSeeds + "\n" +
                                   "[6] Rosa Rosa: " + Session.GetRoleplay().FarmingStats.SeedSatchel.PinkPrimroseSeeds + "\n" +
                                   "[7] Dalia Amarela: " + Session.GetRoleplay().FarmingStats.SeedSatchel.YellowDahliaSeeds + "\n" +
                                   "[8] Dalia Azul: " + Session.GetRoleplay().FarmingStats.SeedSatchel.BlueDahliaSeeds + "\n" +
                                   "[9] Dalia Rosa: " + Session.GetRoleplay().FarmingStats.SeedSatchel.PinkDahliaSeeds + "\n" +
                                   "[10] Flor Amarela: " + Session.GetRoleplay().FarmingStats.SeedSatchel.YellowStarflowerSeeds + "\n" +
                                   "[11] Flor Azul: " + Session.GetRoleplay().FarmingStats.SeedSatchel.BlueStarflowerSeeds + "\n" +
                                   "[12] Flor Vermelha: " + Session.GetRoleplay().FarmingStats.SeedSatchel.RedStarflowerSeeds + "\n\n") : "Você não possui um Saco de sementes!\n\n") +

                                   "<----- Plant Satchel ----->\n" +
                                   (Session.GetRoleplay().FarmingStats.HasPlantSatchel ? ("[1] Plumeria Amarela: " + Session.GetRoleplay().FarmingStats.PlantSatchel.YellowPlumerias + "\n" +
                                   "[2] Plumeria Azul: " + Session.GetRoleplay().FarmingStats.PlantSatchel.BluePlumerias + "\n" +
                                   "[3] Plumeria Rosa: " + Session.GetRoleplay().FarmingStats.PlantSatchel.PinkPlumerias + "\n" +
                                   "[4] Rosa Amarela: " + Session.GetRoleplay().FarmingStats.PlantSatchel.YellowPrimroses + "\n" +
                                   "[5] Rosa Azul: " + Session.GetRoleplay().FarmingStats.PlantSatchel.BluePrimroses + "\n" +
                                   "[6] Rosa Rosa: " + Session.GetRoleplay().FarmingStats.PlantSatchel.PinkPrimroses + "\n" +
                                   "[7] Dalia Amarela: " + Session.GetRoleplay().FarmingStats.PlantSatchel.YellowDahlias + "\n" +
                                   "[8] Dalia Azul: " + Session.GetRoleplay().FarmingStats.PlantSatchel.BlueDahlias + "\n" +
                                   "[9] Dalia Rosa: " + Session.GetRoleplay().FarmingStats.PlantSatchel.PinkDahlias + "\n" +
                                   "[10] Flor Amarela: " + Session.GetRoleplay().FarmingStats.PlantSatchel.YellowStarflowers + "\n" +
                                   "[11] Flor Azul: " + Session.GetRoleplay().FarmingStats.PlantSatchel.BlueStarflowers + "\n" +
                                   "[12] Flor Vermelha: " + Session.GetRoleplay().FarmingStats.PlantSatchel.RedStarflowers + "\n") : "Você não possui um Saco de plantas!"));

            Session.SendMessage(new MOTDNotificationComposer(MessageToSend.ToString()));
        }
    }
}