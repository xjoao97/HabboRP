using System;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Houses;

namespace Plus.HabboRoleplay.Farming
{
    public class SeedSatchel
    {
        public int BlueStarflowerSeeds;
        public int YellowStarflowerSeeds;
        public int PinkDahliaSeeds;
        public int YellowPlumeriaSeeds;
        public int PinkPrimroseSeeds;
        public int BluePrimroseSeeds;
        public int YellowPrimroseSeeds;
        public int YellowDahliaSeeds;
        public int BluePlumeriaSeeds;
        public int PinkPlumeriaSeeds;
        public int RedStarflowerSeeds;
        public int BlueDahliaSeeds;

        public SeedSatchel(DataRow Row)
        {
            this.BlueStarflowerSeeds = Convert.ToInt32(Row["blue_starflower"].ToString().Split(':')[0]);
            this.YellowStarflowerSeeds = Convert.ToInt32(Row["yellow_starflower"].ToString().Split(':')[0]);
            this.PinkDahliaSeeds = Convert.ToInt32(Row["pink_dahlia"].ToString().Split(':')[0]);
            this.YellowPlumeriaSeeds = Convert.ToInt32(Row["yellow_plumeria"].ToString().Split(':')[0]);
            this.PinkPrimroseSeeds = Convert.ToInt32(Row["pink_primrose"].ToString().Split(':')[0]);
            this.BluePrimroseSeeds = Convert.ToInt32(Row["blue_primrose"].ToString().Split(':')[0]);
            this.YellowPrimroseSeeds = Convert.ToInt32(Row["yellow_primrose"].ToString().Split(':')[0]);
            this.YellowDahliaSeeds = Convert.ToInt32(Row["yellow_dahlia"].ToString().Split(':')[0]);
            this.BluePlumeriaSeeds = Convert.ToInt32(Row["blue_plumeria"].ToString().Split(':')[0]);
            this.PinkPlumeriaSeeds = Convert.ToInt32(Row["pink_plumeria"].ToString().Split(':')[0]);
            this.RedStarflowerSeeds = Convert.ToInt32(Row["red_starflower"].ToString().Split(':')[0]);
            this.BlueDahliaSeeds = Convert.ToInt32(Row["blue_dahlia"].ToString().Split(':')[0]);
        }
    }

    public class PlantSatchel
    {
        public int BlueStarflowers;
        public int YellowStarflowers;
        public int PinkDahlias;
        public int YellowPlumerias;
        public int PinkPrimroses;
        public int BluePrimroses;
        public int YellowPrimroses;
        public int YellowDahlias;
        public int BluePlumerias;
        public int PinkPlumerias;
        public int RedStarflowers;
        public int BlueDahlias;

        public PlantSatchel(DataRow Row)
        {
            this.BlueStarflowers = Convert.ToInt32(Row["blue_starflower"].ToString().Split(':')[1]);
            this.YellowStarflowers = Convert.ToInt32(Row["yellow_starflower"].ToString().Split(':')[1]);
            this.PinkDahlias = Convert.ToInt32(Row["pink_dahlia"].ToString().Split(':')[1]);
            this.YellowPlumerias = Convert.ToInt32(Row["yellow_plumeria"].ToString().Split(':')[1]);
            this.PinkPrimroses = Convert.ToInt32(Row["pink_primrose"].ToString().Split(':')[1]);
            this.BluePrimroses = Convert.ToInt32(Row["blue_primrose"].ToString().Split(':')[1]);
            this.YellowPrimroses = Convert.ToInt32(Row["yellow_primrose"].ToString().Split(':')[1]);
            this.YellowDahlias = Convert.ToInt32(Row["yellow_dahlia"].ToString().Split(':')[1]);
            this.BluePlumerias = Convert.ToInt32(Row["blue_plumeria"].ToString().Split(':')[1]);
            this.PinkPlumerias = Convert.ToInt32(Row["pink_plumeria"].ToString().Split(':')[1]);
            this.RedStarflowers = Convert.ToInt32(Row["red_starflower"].ToString().Split(':')[1]);
            this.BlueDahlias = Convert.ToInt32(Row["blue_dahlia"].ToString().Split(':')[1]);
        }
    }
}