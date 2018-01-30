using System;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Houses;

namespace Plus.HabboRoleplay.Farming
{
    public class FarmingStats
    {
        public int Level;
        public int Exp;

        public bool HasSeedSatchel;
        public bool HasPlantSatchel;

        public SeedSatchel SeedSatchel;
        public PlantSatchel PlantSatchel;

        public FarmingStats(DataRow Row)
        {
            this.Level = Convert.ToInt32(Row["level"]);
            this.Exp = Convert.ToInt32(Row["exp"]);

            this.HasSeedSatchel = PlusEnvironment.EnumToBool(Row["has_seed_satchel"].ToString());
            this.HasPlantSatchel = PlusEnvironment.EnumToBool(Row["has_plant_satchel"].ToString());

            this.SeedSatchel = new SeedSatchel(Row);
            this.PlantSatchel = new PlantSatchel(Row);
        }
    }
}