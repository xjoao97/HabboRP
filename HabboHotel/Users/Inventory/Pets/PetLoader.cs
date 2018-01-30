using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;

namespace Plus.HabboHotel.Users.Inventory.Pets
{
    static class PetLoader
    {
        public static List<Pet> GetPetsForUser(int UserId)
        {
            List<Pet> P = new List<Pet>();

            DataTable dPets = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`user_id`,`room_id`,`name`,`x`,`y`,`z` FROM `bots` WHERE `user_id` = '" + UserId + "' AND `room_id` = '0' AND `ai_type` = 'pet'");
                dPets = dbClient.getTable();

                if (dPets != null)
                {
                    foreach (DataRow dRow in dPets.Rows)
                    {
                        dbClient.SetQuery("SELECT `type`,`race`,`color`,`experience`,`energy`,`nutrition`,`respect`,`createstamp`,`have_saddle`,`anyone_ride`,`hairdye`,`pethair`,`gnome_clothing` FROM `bots_petdata` WHERE `id` = '" + Convert.ToInt32(dRow["id"]) + "' LIMIT 1");
                        DataRow mRow = dbClient.getRow();

                        if (mRow != null)
                        {
                            P.Add(new Pet(Convert.ToInt32(dRow["id"]), Convert.ToInt32(dRow["user_id"]), Convert.ToInt32(dRow["room_id"]), Convert.ToString(dRow["name"]), Convert.ToInt32(mRow["type"]), Convert.ToString(mRow["race"]), Convert.ToString(mRow["color"]),
                               Convert.ToInt32(mRow["experience"]), Convert.ToInt32(mRow["energy"]), Convert.ToInt32(mRow["nutrition"]), Convert.ToInt32(mRow["respect"]), Convert.ToDouble(mRow["createstamp"]), Convert.ToInt32(dRow["x"]), Convert.ToInt32(dRow["y"]),
                                Convert.ToDouble(dRow["z"]), Convert.ToInt32(mRow["have_saddle"]), Convert.ToInt32(mRow["anyone_ride"]), Convert.ToInt32(mRow["hairdye"]), Convert.ToInt32(mRow["pethair"]), Convert.ToString(mRow["gnome_clothing"])));
                        }
                    }
                }
            }

            foreach(RoleplayBot Pet in RoleplayBotManager.CachedRoleplayBots.Values)
            {
                if (Pet == null)
                    continue;
                if (!Pet.IsPet)
                    continue;
                if (Pet.OwnerId != UserId)
                    continue;

                P.Add(Pet.PetInstance);
            }

            return P;
        }
    }
}
