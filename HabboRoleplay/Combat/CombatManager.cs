using System;
using System.Collections.Generic;
using Plus.HabboRoleplay.Combat.Types;
using log4net;

namespace Plus.HabboRoleplay.Combat
{
    // TODO :  cache speeches n make get speech shit yh
    public static class CombatManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Combat.CombatManager");

        /// <summary>
        /// Contains all of the combat types
        /// </summary>
        private static Dictionary<string, ICombat> CombatTypes;
                          
        /// <summary>
        /// Initializes the combat manager
        /// </summary>
        public static void Initialize()
        {
            CombatTypes = new Dictionary<string, ICombat>();
            CombatTypes.Add("fist", new Fist());
            CombatTypes.Add("gun", new Gun());

            //log.Info("Gerenciador de Combate (" + CombatTypes.Count + ") -> CARREGADO!");
        }

        /// <summary>
        /// Returns the combat type based on the string
        /// </summary>
        public static ICombat GetCombatType(string Type)
        {
            Type = Type.ToLower();
            return CombatTypes.ContainsKey(Type) ? CombatTypes[Type] : null;
        }

    }
}
