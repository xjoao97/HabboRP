using System;
using System.Data;
using System.Linq;
using System.Collections.Concurrent;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;
using log4net;
using System.Collections.Generic;

namespace Plus.HabboRoleplay.Misc
{
    public class ToDoManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Misc.ToDoManager");

        /// <summary>
        /// Thread-safe dictionary containing todos
        /// </summary>
        public static ConcurrentDictionary<int, ToDo> ToDoList = new ConcurrentDictionary<int, ToDo>();

        /// <summary>
        /// Generates the todo dictionary values from database
        /// </summary>
        public static void Initialize()
        {
            ToDoList.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_todo_list`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int Id = Convert.ToInt32(Row["id"]);
                        int AddedBy = Convert.ToInt32(Row["added_by"]);
                        double TimeStamp = Convert.ToDouble(Row["timestamp"]);
                        string String = Row["todo"].ToString();

                        ToDo ToDo = new ToDo(Id, String, AddedBy, TimeStamp);

                        if (!ToDoList.ContainsKey(Id))
                            ToDoList.TryAdd(Id, ToDo);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new todo to the list
        /// </summary>
        public static void AddNewTodo(ToDo New)
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `rp_todo_list` (added_by, todo, timestamp) VALUES (@addedby, @string, @timestamp)");
                dbClient.AddParameter("addedby", New.AddedBy);
                dbClient.AddParameter("string", New.String);
                dbClient.AddParameter("timestamp", New.TimeStamp);
                New.Id = Convert.ToInt32(dbClient.InsertQuery());
            }

            if (ToDoList.ContainsKey(New.Id))
                return;

            ToDoList.TryAdd(New.Id, New);
        }

        /// <summary>
        /// Removes a todo from the list
        /// </summary>
        public static void DeleteToDo(int Id)
        {
            ToDo Junk;
            ToDoList.TryRemove(Id, out Junk);

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `rp_todo_list` WHERE `id` = '" + Id + "' LIMIT 1");
            }
        }
    }

    public class ToDo
    {
        public int Id;
        public string String;
        public int AddedBy;
        public double TimeStamp;

        public ToDo(int Id, string String, int AddedBy, double TimeStamp)
        {
            this.Id = Id;
            this.String = String;
            this.AddedBy = AddedBy;
            this.TimeStamp = TimeStamp;
        }
    }
}