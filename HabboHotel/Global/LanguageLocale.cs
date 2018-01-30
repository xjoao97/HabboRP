using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;


using log4net;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Global
{
    public class LanguageLocale
    {
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Global.LanguageLocale");

        public LanguageLocale()
        {
            this._values = new Dictionary<string, string>();

            this.Init();
        }

        public void Init()
        {
            if (this._values.Count > 0)
                this._values.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_locale`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        this._values.Add(Row["key"].ToString(), Row["value"].ToString());
                    }
                }
            }

            //log.Info("Language Locale Manager -> LOADED");
        }

        public string TryGetValue(string value)
        {
            return this._values.ContainsKey(value) ? this._values[value] : "Local de idioma faltando para [" + value + "]";
        }
    }
}