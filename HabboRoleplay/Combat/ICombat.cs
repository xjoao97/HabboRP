using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Bots;

namespace Plus.HabboRoleplay.Combat
{
    public interface ICombat
    {
        /// <summary>
        /// Checks to see if the client can complete the action
        /// </summary>
        bool CanCombat(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null);

        /// <summary>
        /// Performs the action
        /// </summary>
        void Execute(GameClient Client, GameClient TargetClient, bool HitClosest = false);

        /// <summary>
        /// Performs the action on a bot
        /// </summary>
        void ExecuteBot(GameClient Client, RoleplayBot Bot);

        /// <summary>
        /// Gets the XP from the combat type
        /// </summary>
        /// <returns>EXP retrieved</returns>
        int GetEXP(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null);

        /// <summary>
        /// Gets the coins from the combat type
        /// </summary>
        /// <returns>Coins retrieved</returns>
        int GetCoins(GameClient TargetClient, RoleplayBot Bot = null);

        /// <summary>
        /// Get Rewards from combat type
        /// </summary>
        void GetRewards(GameClient Client, GameClient TargetClient, RoleplayBot Bot = null);
    }
}
