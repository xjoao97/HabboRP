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

namespace Plus.HabboRoleplay.Food
{
    class FoodManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Food.FoodManager");

        /// <summary>
        /// Thread-safe dictionary containing all roleplay foods
        /// </summary>
        public static ConcurrentDictionary<string, Food> FoodList = new ConcurrentDictionary<string, Food>();

        /// <summary>
        /// Initializes the food list dictionary
        /// </summary>
        public static void Initialize()
        {
            FoodList.Clear();
            DataTable Foods;

            using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT * from `rp_food`");
                Foods = DB.getTable();

                if (Foods != null)
                {
                    foreach (DataRow Food in Foods.Rows)
                    {
                        string Name = Food["name"].ToString();
                        string Type = Convert.ToString(Food["type"]);
                        int ItemId = Convert.ToInt32(Food["item_id"]);
                        string ExtraData = Food["extra_data"].ToString();
                        int Cost = Convert.ToInt32(Food["cost"]);
                        int Health = Convert.ToInt32(Food["health"]);
                        int Energy = Convert.ToInt32(Food["energy"]);
                        int Hunger = Convert.ToInt32(Food["hunger"]);
                        string ServeText = Food["serve_text"].ToString();
                        string EatText = Food["eat_text"].ToString();
                        bool Servable = PlusEnvironment.EnumToBool(Food["servable"].ToString());

                        Food newFood = new Food(Name, Type, ItemId, ExtraData, Cost, Health, Energy, Hunger, ServeText, EatText, Servable);
                        FoodList.TryAdd(Name, newFood);
                    }
                }
            }

            log.Info("Carregado " + FoodList.Count + " comidas.");
        }

        /// <summary>
        /// Gets the food based on itemid
        /// </summary>
        public static Food GetFood(int itemid)
        {
            try
            {
                Food thefood = null;

                foreach (Food food in FoodList.Values)
                {
                    if (food.ItemId == itemid)
                    {
                        return food;
                    }
                }

                return thefood;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the food/drink based on food/drink name
        /// </summary>
        public static Food GetFoodAndDrink(string name)
        {
            try
            {
                Food thefoodanddrink = null;

                foreach (Food food in FoodList.Values)
                {
                    if (food.Name == name.ToLower())
                    {
                        return food;
                    }
                }

                return thefoodanddrink;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the food based on food name
        /// </summary>
        public static Food GetFood(string name)
        {
            try
            {
                Food thefood = null;

                foreach (Food food in FoodList.Values)
                {
                    if (food.Type != "food")
                        continue;

                    if (food.Name == name.ToLower())
                    {
                        return food;
                    }
                }

                return thefood;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the drink based on drink name
        /// </summary>
        public static Food GetDrink(string name)
        {
            try
            {
                Food thedrink = null;

                foreach (Food food in FoodList.Values)
                {
                    if (food.Type != "drink")
                        continue;

                    if (food.Name == name.ToLower())
                    {
                        return food;
                    }
                }

                return thedrink;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if the square infront of the user can be served on
        /// </summary>
        public static bool CanServe(RoomUser User)
        {
            try
            {

                // hardys a retard

                bool CanPlace = false;

                Room Room;
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(User.RoomId, out Room))
                    return false;

                foreach (Item Item in Room.GetRoomItemHandler().GetFloor)
                {
                    foreach (var point in Item.GetAffectedTiles)
                    {
                        if (User.SquareInFront.X == point.X && User.SquareInFront.Y == point.Y)
                        {
                            if (Item.Data.ItemName.ToLower().Contains("table") || Item.Data.PublicName.ToLower().Contains("table"))
                            {
                                CanPlace = true;
                                break;
                            }
                        }
                    }

                    if (CanPlace)
                        break;
                }

                foreach (Item Item in Room.GetRoomItemHandler().GetFloor)
                {
                    if (Item.GetX == User.SquareInFront.X && Item.GetY == User.SquareInFront.Y)
                    {
                        if (GetFood(Item.BaseItem) != null)
                        {
                            CanPlace = false;
                            break;
                        }
                    }
                }
                return CanPlace;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Provides a string containing all the food or drinks the session can serve
        /// </summary>
        public static string GetServableItems(HabboHotel.GameClients.GameClient Session)
        {
            if (!HabboHotel.Groups.GroupManager.HasJobCommand(Session, "serve"))
                return "";

            StringBuilder Items = new StringBuilder();
            var List = new List<Food>();

            foreach (var item in FoodList.Values)
            {
                if (!HabboHotel.Groups.GroupManager.HasJobCommand(Session, item.Type.ToLower()))
                    continue;

                if (!List.Contains(item))
                    List.Add(item);
            }

            int count = 0;
            foreach (var item in List)
            {
                count++;

                Items.Append(item);

                if (count < List.Count)
                    Items.Append(",");
            }

            List = null;
            return Items.ToString();
        }

        /// <summary>
        /// Provides a list containing all the food or drinks the bot can serve
        /// </summary>
        public static List<Food> GetServableBotItems(string type)
        {
            List<Food> ServableItems = new List<Food>();

            foreach (var food in FoodList.Values)
            {
                if (food.Type.ToLower() != type.ToLower())
                    continue;

                if (!ServableItems.Contains(food))
                    ServableItems.Add(food);
            }

            return ServableItems;
        }
    }
}
