using Plus.HabboHotel.Cache;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboRoleplay.RoleplayUsers;

namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// GetUserComponent class.
    /// </summary>
    public class GetUserComponent
    {

        /// <summary>
        /// Returns the user statistics.
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static string ReturnUserStatistics(GameClient User)
        {
            if (User == null)
                return null;

            int UserID = User.GetHabbo().Id;
            string Figure = User.GetHabbo().Look;
            int CurHealth = User.GetRoleplay().CurHealth;
            int MaxHealth = User.GetRoleplay().MaxHealth;
            int MaxEnergy = User.GetRoleplay().MaxEnergy;
            int CurEnergy = User.GetRoleplay().CurEnergy;
            int CurXP = User.GetRoleplay().LevelEXP;
            int NeedXP = LevelManager.Levels.ContainsKey(User.GetRoleplay().Level + 1) ? LevelManager.Levels[User.GetRoleplay().Level + 1] : 100000;
            int Level = User.GetRoleplay().Level;

            string Statistics = 
                UserID + "," +
                Figure + "," +
                CurHealth + "," +
                MaxHealth + "," +
                CurEnergy + "," +
                MaxEnergy + "," + 
                CurXP + "," +
                NeedXP + "," + 
                Level + ","
            ;

            return Statistics;
        }

        /// <summary>
        /// Returns the user statistics via database.
        /// </summary>
        /// <param name="dRow"></param>
        /// <param name="dRowRP"></param>
        /// <returns></returns>
        public static string ReturnUserStatistics(DataRow dRow, DataRow dRowRP)
        {
            int UserID = Convert.ToInt32(dRowRP["id"]);
            string Figure = Convert.ToString(dRow["look"]);
            int CurHealth = Convert.ToInt32(dRowRP["curhealth"]);
            int MaxHealth = Convert.ToInt32(dRowRP["maxhealth"]);
            int MaxEnergy = Convert.ToInt32(dRowRP["maxenergy"]);
            int CurEnergy = Convert.ToInt32(dRowRP["curenergy"]);
            int CurXP = Convert.ToInt32(dRowRP["level_exp"]);
            int NeedXP = LevelManager.Levels.ContainsKey(Convert.ToInt32(dRowRP["level"]) + 1) ? LevelManager.Levels[Convert.ToInt32(dRowRP["level"]) + 1] : 100000;
            int Level = Convert.ToInt32(dRowRP["level"]);

            string Statistics =
                UserID + "," +
                Figure + "," +
                CurHealth + "," +
                MaxHealth + "," +
                CurEnergy + "," +
                MaxEnergy + "," +
                CurXP + "," +
                NeedXP + "," +
                Level + ","
            ;

            return Statistics;
        }

        /// <summary>
        /// Return user statistics via cache.
        /// </summary>
        /// <param name="CachedUser"></param>
        /// <returns></returns>
        public static string ReturnUserStatistics(UserCache CachedUser)
        {
            string[] SocketParts = CachedUser.SocketStatistics.Split(',');

            int UserID = Convert.ToInt32(SocketParts[0]);
           
            string Figure = Convert.ToString(SocketParts[1]);
            int CurHealth = Convert.ToInt32(SocketParts[2]);
            int MaxHealth = Convert.ToInt32(SocketParts[3]);
            int MaxEnergy = Convert.ToInt32(SocketParts[4]);
            int CurEnergy = Convert.ToInt32(SocketParts[5]);
            int CurXP = Convert.ToInt32(SocketParts[6]);
            int NeedXP = Convert.ToInt32(SocketParts[7]);
            int Level = Convert.ToInt32(SocketParts[8]);

            string Statistics =
                UserID + "," +
                Figure + "," +
                CurHealth + "," +
                MaxHealth + "," +
                CurEnergy + "," +
                MaxEnergy + "," + 
                CurXP + "," +
                NeedXP + "," + 
                Level + ","
            ;

            return Statistics;
        }

        /// <summary>
        /// Clears the statistic dialogue.
        /// </summary>
        /// <param name="User"></param>
        public static void ClearStatisticsDialogue(GameClient User)
        {
            if (User.GetRoleplay().WebSocketConnection != null)
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, "compose_clear_characterbar:true");
        }
    }
}
