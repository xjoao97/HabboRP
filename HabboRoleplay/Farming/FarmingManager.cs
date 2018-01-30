using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using log4net;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using System.Threading;
using Plus.HabboRoleplay.Bots;
using System.Drawing;

namespace Plus.HabboRoleplay.Farming
{
    class FarmingManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Food.FarmingManager");

        /// <summary>
        /// Static int used to generate item ids
        /// </summary>
        public static int SignMultiplier = 2500000;

        /// <summary>
        /// Thread-safe dictionary containing all farming items
        /// </summary>
        public static ConcurrentDictionary<string, FarmingItem> FarmingItems = new ConcurrentDictionary<string, FarmingItem>();

        /// <summary>
        /// Thread-safe dictionary containing all farming spaces
        /// </summary>
        public static ConcurrentDictionary<int, FarmingSpace> FarmingSpaces = new ConcurrentDictionary<int, FarmingSpace>();

        #region Levels Dictionary
        public static readonly Dictionary<int, int> levels = new Dictionary<int, int>
        {
            {1,0},
            {2,2000},
            {3,4500},
            {4,10000},
            {5,17000},
            {6,30000},
            {7,53000},
            {8,91000},
            {9,154000},
            {10,256000},
            {11,423000},
            {12,692000},
            {13,1124000},
            {14,1816000},
            {15,2913000},
            {16,4671000},
            {17,7444000},
            {18,11823000},
            {19,18720000},
            {20,30000000},
            {21,40000000}
        };
        #endregion

        /// <summary>
        /// Generates the FarmingItems dictionary based on database
        /// </summary>
        public static void Initialize()
        {
            #region Farming Items
            FarmingItems.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_farming`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int Id = Convert.ToInt32(Row["id"]);
                        string BaseItem = Row["base_item"].ToString().ToLower();
                        int LevelRequired = Convert.ToInt32(Row["level_required"]);
                        int MinExp = Convert.ToInt32(Row["min_exp"]);
                        int MaxExp = Convert.ToInt32(Row["max_exp"]);
                        int SellPrice = Convert.ToInt32(Row["sell_price"]);
                        int BuyPrice = Convert.ToInt32(Row["buy_price"]);

                        FarmingItem FarmingItem = new FarmingItem(Id, BaseItem, LevelRequired, MinExp, MaxExp, SellPrice, BuyPrice);

                        if (!FarmingItems.ContainsKey(BaseItem))
                            FarmingItems.TryAdd(BaseItem, FarmingItem);
                    }
                }
            }
            //log.Info("Carregado " + FarmingItems.Count + " itens agrícolas.");
            #endregion

            #region Farming Spaces

            #region Remove all existing farming spaces
            if (FarmingSpaces.Count > 0)
            {
                foreach (var Space in FarmingSpaces.Values)
                {
                    if (Space.Item == null)
                        continue;

                    if (Space.Item.GetRoom() == null || Space.Item.GetRoom().GetRoomItemHandler() == null)
                        continue;

                    Space.Item.GetRoom().GetRoomItemHandler().RemoveFurniture(null, Space.Item.Id);

                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `rp_farming_spaces` SET `expiration` = @expiration, `owner_id` = @owner WHERE `id` = @id");
                        dbClient.AddParameter("owner", Space.OwnerId);
                        dbClient.AddParameter("expiration", Space.Expiration);
                        dbClient.AddParameter("id", Space.Id);
                        dbClient.RunQuery();
                    }
                }
            }
            #endregion

            FarmingSpaces.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_farming_spaces`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int Id = Convert.ToInt32(Row["id"]);
                        int ItemId = Convert.ToInt32(Row["item_id"]);
                        int RoomId = Convert.ToInt32(Row["room_id"]);
                        int Cost = Convert.ToInt32(Row["cost"]);
                        int X = Convert.ToInt32(Row["x"]);
                        int Y = Convert.ToInt32(Row["y"]);
                        double Z = Convert.ToDouble(Row["z"]);
                        int OwnerId = Convert.ToInt32(Row["owner_id"]);
                        int Expiration = Convert.ToInt32(Row["expiration"]);

                        FarmingSpace FarmingSpace = new FarmingSpace(Id, ItemId, RoomId, Cost, X, Y, Z, OwnerId, Expiration);

                        if (!FarmingSpaces.ContainsKey(Id))
                        {
                            FarmingSpaces.TryAdd(Id, FarmingSpace);
                            FarmingSpace.SpawnSign();
                        }
                    }
                }
            }
            //log.Info("Carregado " + FarmingSpaces.Count + " espaços agrícolas.");
            #endregion
        }

        /// <summary>
        /// Gets the farming item based on the base item_name
        /// </summary>
        public static FarmingItem GetFarmingItem(string BaseItem)
        {
            if (!FarmingItems.ContainsKey(BaseItem.ToLower()))
                return null;

            return FarmingItems[BaseItem.ToLower()];
        }

        /// <summary>
        /// Gets the farming item based on the id
        /// </summary>
        public static FarmingItem GetFarmingItem(int Id)
        {
            if (FarmingItems.Values.Where(x => x.Id == Id).ToList().Count <= 0)
                return null;

            return FarmingItems.Values.FirstOrDefault(x => x.Id == Id);
        }

        /// <summary>
        /// Adds exp to the farming skill
        /// </summary>
        public static void AddEXP(GameClient Session, int Amount)
        {
            try
            {
                Amount = Convert.ToInt32(RoleplayData.GetData("farming", "modifier")) * Amount;

                if (Session != null && Session.GetRoleplay() != null)
                {
                    Session.GetRoleplay().FarmingStats.Exp += Amount;

                    if (LevelUp(Session))
                    {
                        Session.GetRoleplay().FarmingStats.Level += 1;
                        Session.SendWhisper("Você acabou de upar sua agricultura! Agora é Nível: " + Session.GetRoleplay().FarmingStats.Level + ".");
                    }
                    else
                        Session.SendWhisper("Você recebeu " + Amount + " XP! para o seu próximo nível, você precisa " + (levels[Session.GetRoleplay().FarmingStats.Level + 1] - Session.GetRoleplay().FarmingStats.Exp) + "XP para alcançar " + (Session.GetRoleplay().FarmingStats.Level + 1), 1);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Checks if you can level up based on exp added
        /// </summary>
        public static bool LevelUp(GameClient Session)
        {
            try
            {
                if (Session != null && Session.GetRoleplay() != null)
                {
                    int Level = Session.GetRoleplay().FarmingStats.Level + 1;
                    int LevelEXP = Session.GetRoleplay().FarmingStats.Exp;

                    if (levels.ContainsKey(Level))
                    {
                        if (LevelEXP >= levels[Level])
                        {
                            return true;
                        }

                    }
                    return false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Equips the watering can
        /// </summary>
        public static void EquipWateringCan(GameClient Session, Item Item)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null || Session.GetRoomUser() == null || Item == null)
                return;
            
            if (!GroupManager.HasJobCommand(Session, "farming"))
            {
                Session.SendWhisper("Desculpe, apenas um fazendeiro pode usar o regador!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar o regador!", 1);
                return;
            }

            if (Session.GetRoleplay().WateringCan)
            {
                Session.SendWhisper("Você já está equipando um regador!", 1);
                return;
            }

            if (Session.GetRoleplay().EquippedWeapon != null)
                Session.GetRoleplay().EquippedWeapon = null;

            Session.GetRoleplay().WateringCan = true;
            Session.GetRoomUser().ApplyEffect(192);

            Session.Shout("*Grabs a watering can from the floor and gets ready to start farming*", 4);
        }

        /// <summary>
        /// Plants the seed on the dirt nest
        /// </summary>
        public static void PlantSeed(GameClient Session, Item Item)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null || Session.GetRoomUser() == null || Item == null || Item.GetRoom() == null)
                return;

            if (Item.GetRoom() == null)
                return;

            if (Session.GetRoleplay().TryGetCooldown("farming", false))
                return;

            #region Check Rentable Space
            if (Item.GetRoom().GetRoomItemHandler() != null && Item.GetRoom().GetRoomItemHandler().GetFloor != null)
            {
                lock (Item.GetRoom().GetRoomItemHandler().GetFloor)
                {
                    List<Item> OwnedRentableSpaces = Item.GetRoom().GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == InteractionType.RENTABLE_SPACE && x.RentableSpaceData != null && x.RentableSpaceData.FarmingSpace != null && x.RentableSpaceData.FarmingSpace.OwnerId == Session.GetHabbo().Id).ToList();
                    if (OwnedRentableSpaces.Count <= 0)
                    {
                        Session.SendWhisper("Você não possui esse terreno para cultivar!", 1);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                        return;
                    }

                    Item SpaceItem = OwnedRentableSpaces.FirstOrDefault();
                    List<Point> SpacePoints = SpaceItem.GetAffectedTiles;

                    if (!SpacePoints.Contains(Item.Coordinate))
                    {
                        Session.SendWhisper("Você não possui esse terreno para cultivar!", 1);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                        return;
                    }
                }
            }
            #endregion

            if (!GroupManager.HasJobCommand(Session, "farming"))
            {
                Session.SendWhisper("Desculpe, apenas um agricultor pode plantar sementes!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para plantar sementes!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                return;
            }

            if (RoleplayManager.FarmingCAPTCHABox)
            {
                if (Session.GetRoleplay().CaptchaSent)
                {
                    Session.SendWhisper("Você deve inserir o código no AFK para continuar cultivando!", 1);
                    Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                    return;
                }
            }

            if (Item.GetRoom().GetGameMap().GetRoomItemForSquare(Item.GetX, Item.GetY).Where(x => x.GetBaseItem().InteractionType == InteractionType.FARMING).ToList().Count > 0)
            {
                Item FarmItem = Item.GetRoom().GetGameMap().GetRoomItemForSquare(Item.GetX, Item.GetY).FirstOrDefault(x => x.GetBaseItem().InteractionType == InteractionType.FARMING);

                if (FarmItem == null)
                    return;

                if (FarmItem.Interactor == null)
                    return;

                FarmItem.Interactor.OnTrigger(Session, FarmItem, 0, true);

                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                return;
            }

            if (Session.GetRoleplay().FarmingItem == null)
            {
                Session.SendWhisper("Você ainda não selecionou uma semente para plantar! Use o comando ':plantar <id>' para selecionar um!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                return;
            }

            ItemData Furni;
            if (!PlusEnvironment.GetGame().GetItemManager().GetItem(Session.GetRoleplay().FarmingItem.BaseItem, out Furni))
            {
                Session.SendWhisper("Desculpe, mas a planta que você selecionou é inválida! Use o comando ':plantar <id>' para selecionar outro!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                return;
            }

            int Amount;
            if (!GetSatchelAmount(Session, false, out Amount))
            {
                Session.SendWhisper("Desculpe, mas a planta que você selecionou é inválida! Use o comando ':plantar <id>' para selecionar outro!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                return;
            }

            if (Amount <= 0)
            {
                Session.SendWhisper("Parece que você ficou sem sementes " + Furni.PublicName + "!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
                return;
            }

            if (!Gamemap.TilesTouching(Session.GetRoomUser().Coordinate.X, Session.GetRoomUser().Coordinate.Y, Item.GetX, Item.GetY))
            {
                Session.GetRoomUser().MoveTo(Item.Coordinate);
                return;
            }

            IncreaseSatchelCount(Session, Session.GetRoleplay().FarmingItem, -1, false);
            RoleplayManager.PlaceItemToRoom(null, Furni.Id, 0, Item.GetX, Item.GetY, (Item.GetZ + 0.01), 0, false, Item.GetRoom().Id, false);
            Session.Shout("*Planta uma semente " + Furni.PublicName + "no refugo*", 4);

            Session.GetRoleplay().CooldownManager.CreateCooldown("farming", 500);
            return;
        }

        /// <summary>
        /// Returns the satchel amount (bool plant, false = seeds)
        /// </summary>
        public static bool GetSatchelAmount(GameClient Session, bool Plant, out int Amount)
        {
            Amount = 0;

            if (Session == null || Session.GetRoleplay() == null || Session.GetRoleplay().FarmingItem == null)
                return false;

            #region Plant Satchel
            if (Plant)
            {
                switch (Session.GetRoleplay().FarmingItem.Id)
                {
                    case 1:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.YellowPlumerias;
                        return true;
                    case 2:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.BluePlumerias;
                        return true;
                    case 3:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.PinkPlumerias;
                        return true;
                    case 4:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.YellowPrimroses;
                        return true;
                    case 5:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.BluePrimroses;
                        return true;
                    case 6:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.PinkPrimroses;
                        return true;
                    case 7:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.YellowDahlias;
                        return true;
                    case 8:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.BlueDahlias;
                        return true;
                    case 9:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.PinkDahlias;
                        return true;
                    case 10:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.YellowStarflowers;
                        return true;
                    case 11:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.BlueStarflowers;
                        return true;
                    case 12:
                        Amount = Session.GetRoleplay().FarmingStats.PlantSatchel.RedStarflowers;
                        return true;
                }
            }
            #endregion

            #region Seed Satchel
            else
            {
                switch (Session.GetRoleplay().FarmingItem.Id)
                {
                    case 1:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.YellowPlumeriaSeeds;
                        return true;
                    case 2:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.BluePlumeriaSeeds;
                        return true;
                    case 3:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.PinkPlumeriaSeeds;
                        return true;
                    case 4:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.YellowPrimroseSeeds;
                        return true;
                    case 5:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.BluePrimroseSeeds;
                        return true;
                    case 6:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.PinkPrimroseSeeds;
                        return true;
                    case 7:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.YellowDahliaSeeds;
                        return true;
                    case 8:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.BlueDahliaSeeds;
                        return true;
                    case 9:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.PinkDahliaSeeds;
                        return true;
                    case 10:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.YellowStarflowerSeeds;
                        return true;
                    case 11:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.BlueStarflowerSeeds;
                        return true;
                    case 12:
                        Amount = Session.GetRoleplay().FarmingStats.SeedSatchel.RedStarflowerSeeds;
                        return true;
                }
            }
            #endregion

            return false;
        }

        /// <summary>
        /// Increases the sessions satchel based on the amount and plant bool (false = seed satchel)
        /// </summary>
        public static void IncreaseSatchelCount(GameClient Session, FarmingItem Item, int Amount, bool Plant)
        {
            if (Session == null || Item == null || Session.GetRoleplay() == null || Session.GetRoleplay().FarmingStats == null)
                return;

            #region Plant Satchel
            if (Plant)
            {
                switch (Item.Id)
                {
                    case 1:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.YellowPlumerias += Amount;
                        break;
                    case 2:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.BluePlumerias += Amount;
                        break;
                    case 3:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.PinkPlumerias += Amount;
                        break;
                    case 4:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.YellowPrimroses += Amount;
                        break;
                    case 5:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.BluePrimroses += Amount;
                        break;
                    case 6:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.PinkPrimroses += Amount;
                        break;
                    case 7:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.YellowDahlias += Amount;
                        break;
                    case 8:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.BlueDahlias += Amount;
                        break;
                    case 9:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.PinkDahlias += Amount;
                        break;
                    case 10:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.YellowStarflowers += Amount;
                        break;
                    case 11:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.BlueStarflowers += Amount;
                        break;
                    case 12:
                        Session.GetRoleplay().FarmingStats.PlantSatchel.RedStarflowers += Amount;
                        break;
                }
            }
            #endregion

            #region Seed Satchel
            else
            {
                switch (Item.Id)
                {
                    case 1:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.YellowPlumeriaSeeds += Amount;
                        break;
                    case 2:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.BluePlumeriaSeeds += Amount;
                        break;
                    case 3:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.PinkPlumeriaSeeds += Amount;
                        break;
                    case 4:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.YellowPrimroseSeeds += Amount;
                        break;
                    case 5:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.BluePrimroseSeeds += Amount;
                        break;
                    case 6:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.PinkPrimroseSeeds += Amount;
                        break;
                    case 7:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.YellowDahliaSeeds += Amount;
                        break;
                    case 8:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.BlueDahliaSeeds += Amount;
                        break;
                    case 9:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.PinkDahliaSeeds += Amount;
                        break;
                    case 10:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.YellowStarflowerSeeds += Amount;
                        break;
                    case 11:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.BlueStarflowerSeeds += Amount;
                        break;
                    case 12:
                        Session.GetRoleplay().FarmingStats.SeedSatchel.RedStarflowerSeeds += Amount;
                        break;
                }
            }
            #endregion
        }

        /// <summary>
        /// Sells all the sessions plants
        /// </summary>
        public static int SellPlants(GameClient Session)
        {
            if (Session == null || Session.GetRoleplay() == null || Session.GetRoleplay().FarmingStats == null)
                return 0;

            var Satchel = Session.GetRoleplay().FarmingStats.PlantSatchel;
            int Amount = 0;

            #region Plumerias
            if (Satchel.YellowPlumerias > 0)
            {
                FarmingItem Item = GetFarmingItem(1);
                if (Item != null)
                {
                    Amount += (Satchel.YellowPlumerias * Item.SellPrice);
                    Satchel.YellowPlumerias = 0;
                }
            }

            if (Satchel.BluePlumerias > 0)
            {
                FarmingItem Item = GetFarmingItem(2);
                if (Item != null)
                {
                    Amount += (Satchel.BluePlumerias * Item.SellPrice);
                    Satchel.BluePlumerias = 0;
                }
            }

            if (Satchel.PinkPlumerias > 0)
            {
                FarmingItem Item = GetFarmingItem(3);
                if (Item != null)
                {
                    Amount += (Satchel.PinkPlumerias * Item.SellPrice);
                    Satchel.PinkPlumerias = 0;
                }
            }
            #endregion

            #region Primroses
            if (Satchel.YellowPrimroses > 0)
            {
                FarmingItem Item = GetFarmingItem(4);
                if (Item != null)
                {
                    Amount += (Satchel.YellowPrimroses * Item.SellPrice);
                    Satchel.YellowPrimroses = 0;
                }
            }

            if (Satchel.BluePrimroses > 0)
            {
                FarmingItem Item = GetFarmingItem(5);
                if (Item != null)
                {
                    Amount += (Satchel.BluePrimroses * Item.SellPrice);
                    Satchel.BluePrimroses = 0;
                }
            }

            if (Satchel.PinkPrimroses > 0)
            {
                FarmingItem Item = GetFarmingItem(6);
                if (Item != null)
                {
                    Amount += (Satchel.PinkPrimroses * Item.SellPrice);
                    Satchel.PinkPrimroses = 0;
                }
            }
            #endregion

            #region Dahlias
            if (Satchel.YellowDahlias > 0)
            {
                FarmingItem Item = GetFarmingItem(7);
                if (Item != null)
                {
                    Amount += (Satchel.YellowDahlias * Item.SellPrice);
                    Satchel.YellowDahlias = 0;
                }
            }

            if (Satchel.BlueDahlias > 0)
            {
                FarmingItem Item = GetFarmingItem(8);
                if (Item != null)
                {
                    Amount += (Satchel.BlueDahlias * Item.SellPrice);
                    Satchel.BlueDahlias = 0;
                }
            }

            if (Satchel.PinkDahlias > 0)
            {
                FarmingItem Item = GetFarmingItem(9);
                if (Item != null)
                {
                    Amount += (Satchel.PinkDahlias * Item.SellPrice);
                    Satchel.PinkDahlias = 0;
                }
            }
            #endregion

            #region Starflowers
            if (Satchel.YellowStarflowers > 0)
            {
                FarmingItem Item = GetFarmingItem(10);
                if (Item != null)
                {
                    Amount += (Satchel.YellowStarflowers * Item.SellPrice);
                    Satchel.YellowStarflowers = 0;
                }
            }

            if (Satchel.BlueStarflowers > 0)
            {
                FarmingItem Item = GetFarmingItem(11);
                if (Item != null)
                {
                    Amount += (Satchel.BlueStarflowers * Item.SellPrice);
                    Satchel.BlueStarflowers = 0;
                }
            }

            if (Satchel.RedStarflowers > 0)
            {
                FarmingItem Item = GetFarmingItem(12);
                if (Item != null)
                {
                    Amount += (Satchel.RedStarflowers * Item.SellPrice);
                    Satchel.RedStarflowers = 0;
                }
            }
            #endregion

            return Amount;
        }

        /// <summary>
        /// Gets all farming sapces in the room based on room id
        /// </summary>
        public static List<FarmingSpace> GetFarmingSpacesByRoomId(int roomid)
        {
            if (FarmingSpaces.Values.Where(x => x.RoomId == roomid).ToList().Count > 0)
                return FarmingSpaces.Values.Where(x => x.RoomId == roomid).ToList();
            else
                return new List<FarmingSpace>();
        }

        /// <summary>
        /// Updates all farming space expirations on shutdown
        /// </summary>
        public static void UpdateAllFarmingSpaces()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                lock (FarmingSpaces)
                {
                    foreach (FarmingSpace Space in FarmingSpaces.Values)
                    {
                        dbClient.SetQuery("UPDATE `rp_farming_spaces` SET `expiration` = @expiration, `owner_id` = @owner WHERE `id` = @id");
                        dbClient.AddParameter("owner", Space.OwnerId);
                        dbClient.AddParameter("expiration", Space.Expiration);
                        dbClient.AddParameter("id", Space.Id);
                        dbClient.RunQuery();
                    }
                }
            }
        }
    }
}
