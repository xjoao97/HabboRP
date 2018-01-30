using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms.AI;


namespace Plus.HabboHotel.Items.Utilities
{
    public static class PetUtility
    {
        public static bool IsPet(InteractionType Type)
        {
            if (Type == InteractionType.pet0 || Type == InteractionType.pet1 || Type == InteractionType.pet2 || Type == InteractionType.pet3 || Type == InteractionType.pet4 || Type == InteractionType.pet5 || Type == InteractionType.pet6 ||
               Type == InteractionType.pet7 || Type == InteractionType.pet8 || Type == InteractionType.pet9 || Type == InteractionType.pet10 || Type == InteractionType.pet11 || Type == InteractionType.pet12 ||
               Type == InteractionType.pet13 || Type == InteractionType.pet14 || Type == InteractionType.pet15 || Type == InteractionType.pet16 || Type == InteractionType.pet17 || Type == InteractionType.pet18)
                return true;
            return false;
        }

        public static bool CheckPetName(string PetName)
        {
            if (PetName.Length < 1 || PetName.Length > 16)
                return false;

            if (!PlusEnvironment.IsValidAlphaNumeric(PetName))
                return false;

            return true;
        }

        public static Pet CreatePet(int UserId, string Name, int Type, string Race, string Color)
        {
            Pet pet = new Pet(0, UserId, 0, Name, Type, Race, Color, 0, 100, 100, 0, PlusEnvironment.GetUnixTimestamp(), 0, 0, 0.0, 0, 0, 0, -1, "-1");

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO bots (user_id,name, ai_type) VALUES (" + pet.OwnerId + ",@" + pet.PetId + "name, 'pet')");
                dbClient.AddParameter(pet.PetId + "name", pet.Name);
                pet.PetId = Convert.ToInt32(dbClient.InsertQuery());

                dbClient.SetQuery("INSERT INTO bots_petdata (id,type,race,color,experience,energy,createstamp) VALUES (" + pet.PetId + ", " + pet.Type + ",@" + pet.PetId + "race,@" + pet.PetId + "color,0,100,UNIX_TIMESTAMP())");
                dbClient.AddParameter(pet.PetId + "race", pet.Race);
                dbClient.AddParameter(pet.PetId + "color", pet.Color);
                dbClient.RunQuery();
            }
            return pet;
        }
    }
}
