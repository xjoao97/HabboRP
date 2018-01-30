using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using log4net;

namespace Plus.HabboRoleplay.Weapons
{
    public static class WeaponManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Weapons");

        /// <summary>
        /// Thread-safe dictionary containing all the weapons
        /// </summary>
        public static ConcurrentDictionary<string, Weapon> Weapons;

        /// <summary>
        /// List containing all weapon enables
        /// </summary>
        public static List<int> Enables;

        /// <summary>
        /// List containing all weapon handitems
        /// </summary>
        public static List<int> HandItems;

        /// <summary>
        /// Initializes the weapon manager
        /// </summary>
        public static void Initialize()
        {
            if (Weapons == null)
            {
                Weapons = new ConcurrentDictionary<string, Weapon>();
                Enables = new List<int>();
                HandItems = new List<int>();
            }
            else
            {
                Weapons.Clear();
                Enables.Clear();
                HandItems.Clear();
            }

            using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT * FROM `rp_weapons`");
                DataTable WeaponTable = DB.getTable();

                if (WeaponTable == null)
                    log.Error("Falha ao carregar Armas do Roleplay!");
                else
                    ProcessWeaponsTable(WeaponTable);
            }
        }

        /// <summary>
        /// Creates an instance of the weapon and stores it in the dictionary
        /// </summary>
        /// <param name="WeaponTable"></param>
        private static void ProcessWeaponsTable(DataTable WeaponTable)
        {
            foreach (DataRow Row in WeaponTable.Rows)
            {
                uint ID = Convert.ToUInt32(Row["id"]);

                string WeaponUnfriendlyName = Convert.ToString(Row["name"]);
                string WeaponName = Convert.ToString(Row["publicname"]);
                string FiringText = Convert.ToString(Row["firingtext"]);
                string EquipText = Convert.ToString(Row["equiptext"]);
                string UnEquipText = Convert.ToString(Row["unequiptext"]);
                string ReloadText = Convert.ToString(Row["reloadtext"]);
                int Energy = Convert.ToInt32(Row["energy"]);
                int EffectID = Convert.ToInt32(Row["effectid"]);
                int HandItem = Convert.ToInt32(Row["handitem"]);

                int Range = Convert.ToInt32(Row["firingrange"]);
                int MinDamage = Convert.ToInt32(Row["mindamage"]);
                int MaxDamage = Convert.ToInt32(Row["maxdamage"]);
                int ClipSize = Convert.ToInt32(Row["clipsize"]);
                int ReloadTime = Convert.ToInt32(Row["reloadtime"]);

                int Cost = Convert.ToInt32(Row["cost"]);
                int CostFine = Convert.ToInt32(Row["costfine"]);
                int Stock = Convert.ToInt32(Row["stock"]);
                int LevelRequirement = Convert.ToInt32(Row["level_requirement"]);

                if (Weapons.ContainsKey(WeaponUnfriendlyName))
                    continue;

                Weapon Weapon = new Weapon(ID, WeaponUnfriendlyName, WeaponName, FiringText, EquipText, UnEquipText, ReloadText, Energy, EffectID, HandItem, Range, MinDamage, MaxDamage, ClipSize, ReloadTime, Cost, CostFine, Stock, LevelRequirement, true);
                Weapons.TryAdd(WeaponUnfriendlyName, Weapon);

                Enables.Add(Weapon.EffectID);
                HandItems.Add(Weapon.HandItem);
            }

            log.Info("Carregado " + Weapons.Count + " armas.");
        }

        /// <summary>
        /// Gets the weapon based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Weapon getWeapon(string name)
        {
            if (Weapons.ContainsKey(name))
                return Weapons[name];
            else
                return null;
        }
    }
}
