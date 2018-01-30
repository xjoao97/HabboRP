using System;
using System.Linq;

namespace Plus.HabboHotel.Groups
{
    public class GroupRank
    {
        #region Variables
        public int JobId;
        public int RankId;
        public string Name;
        public string MaleFigure;
        public string FemaleFigure;
        public int Pay;
        public string[] Commands;
        public string[] WorkRooms;
        public int Limit;
        #endregion

        public GroupRank(int JobId, int RankId, string Name, string MaleFigure, string FemaleFigure, int Pay, string[] Commands, string[] WorkRooms, int Limit)
        {
            this.JobId = JobId;
            this.RankId = RankId;
            this.Name = Name;
            this.MaleFigure = MaleFigure;
            this.FemaleFigure = FemaleFigure;
            this.Pay = Pay;
            this.Commands = Commands;
            this.WorkRooms = WorkRooms;
            this.Limit = Limit;
        }

        public bool HasCommand(string Command)
        {
            return this.Commands.ToList().Contains(Command.ToLower());
        }

        public bool CanWorkHere(int WorkRoom)
        {
            for (int i = 0; i < WorkRooms.Length; i++)
            {
                if (WorkRooms[i].ToLower() == WorkRoom.ToString() || WorkRooms[i].ToLower() == "*")
                    return true;
            }

            return false;
        }
    }
}