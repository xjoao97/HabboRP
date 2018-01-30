using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Catalog.Pets
{
    public class PetRaceManager
    {
        private List<PetRace> _races = new List<PetRace>();

        public void Init()
        {
            if (this._races.Count > 0)
                this._races.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `catalog_pet_races`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        PetRace Race = new PetRace(Convert.ToInt32(Row["raceid"]), Convert.ToInt32(Row["color1"]), Convert.ToInt32(Row["color2"]), (Convert.ToString(Row["has1color"]) == "1"), (Convert.ToString(Row["has2color"]) == "1"));
                        if (!this._races.Contains(Race))
                            this._races.Add(Race);
                    }
                }
            }
        }

        public int GetPetId(string Type, out string Packet)
        {
            int PetId = 0;
            Packet = "";

            switch (Type)
            {
                case "a0 pet0":
                    Packet = "a0 pet0";
                    //count = 25;
                    PetId = 0;
                    break;

                case "a0 pet1":
                    Packet = "a0 pet1";
                    //count = 25;
                    PetId = 1;
                    break;

                case "a0 pet2":
                    Packet = "a0 pet2";
                    //count = 12;
                    PetId = 2;
                    break;

                case "a0 pet3":
                    Packet = "a0 pet3";
                    //count = 7;
                    PetId = 3;
                    break;

                case "a0 pet4":
                    Packet = "a0 pet4";
                    //count = 4;
                    PetId = 4;
                    break;

                case "a0 pet5":
                    Packet = "a0 pet5";
                    //count = 7;
                    PetId = 5;
                    break;

                case "a0 pet6":
                    Packet = "a0 pet6";
                    //count = 13;
                    PetId = 6;
                    break;

                case "a0 pet7":
                    Packet = "a0 pet7";
                    //count = 8;
                    PetId = 7;
                    break;

                case "a0 pet8":
                    Packet = "a0 pet8";
                    //count = 13;
                    PetId = 8;
                    break;

                case "a0 pet9":
                    Packet = "a0 pet9";
                    //count = 14;
                    PetId = 9;
                    break;

                case "a0 pet10":
                    Packet = "a0 pet10";
                    //count = 1;
                    PetId = 10;
                    break;

                case "a0 pet11":
                    Packet = "a0 pet11";
                    //count = 14;
                    PetId = 11;
                    break;

                case "a0 pet12":
                    Packet = "a0 pet12";
                    //count = 8;
                    PetId = 12;
                    break;

                case "a0 pet13": // Caballo - Horse
                    Packet = "a0 pet13";
                    //count = 17;
                    PetId = 13;
                    break;

                case "a0 pet14"://Monkey
                    Packet = "a0 pet14";
                    //count = 9;
                    PetId = 14;
                    break;

                case "a0 pet15":
                    Packet = "a0 pet15";
                    //count = 16;
                    PetId = 15;
                    break;

                case "a0 pet16": // MosterPlant
                    Packet = "a0 pet16";
                    //count = 18;
                    PetId = 16;
                    break;

                case "a0 pet17": // bunnyeaster
                    Packet = "a0 pet17";
                    //count = 19;
                    PetId = 17;
                    break;

                case "a0 pet18": // bunnydepressed
                    Packet = "a0 pet18";
                    //count = 20;
                    PetId = 18;
                    break;

                case "a0 pet19": // bunnylove
                    Packet = "a0 pet19";
                    //count = 21;
                    PetId = 19;
                    break;

                case "a0 pet20": // MosterPlant
                    Packet = "a0 pet20";
                    //count = 22;
                    PetId = 20;
                    break;

                case "a0 pet21": // pigeonevil
                    Packet = "a0 pet21";
                    //count = 23;
                    PetId = 21;
                    break;

                case "a0 pet22": //pigeongood
                    Packet = "a0 pet22";
                    //count = 24;
                    PetId = 22;
                    break;

                case "a0 pet28"://Baby kitten
                    Packet = "a0 pet28";
                    PetId = 28;
                    break;

                case "a0 pet29"://Baby puppy
                    Packet = "a0 pet29";
                    PetId = 29;
                    break;

                case "a0 pet30"://Baby piglet
                    Packet = "a0 pet30";
                    PetId = 30;
                    break;
            }

            return PetId;
        }

        public List<PetRace> GetRacesForRaceId(int RaceId)
        {
            return this._races.Where(Race => Race.RaceId == RaceId).ToList();
        }
    }
}