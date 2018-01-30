using System;
using System.Drawing;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.Core;

namespace Plus.HabboRoleplay.Games
{
    public class RoleplayTeam
    {
        public string Name;
        public string Uniform;
        public int Score;
        public int SpawnRoom;
        public Point SpawnPoint;
        public int CaptureRoom;
        public bool InGame;
        public List<int> Members;
        public List<int> LostMembers;

        public RoleplayTeam(string name, string[] data)
        {
            this.Name = name;
            this.Score = 0;
            this.Uniform = data[0];
            this.InGame = true;
            this.SpawnRoom = Convert.ToInt32(data[1]);
            this.SpawnPoint = new Point(Convert.ToInt32(data[2]), Convert.ToInt32(data[3]));
            this.CaptureRoom = Convert.ToInt32(data[4]);
            this.Members = new List<int>();
            this.LostMembers = new List<int>();
        }

        public void SendToPoint(GameClient Client)
        {
            try
            {
                if (Client == null || Client.GetRoleplay() == null || Client.GetHabbo() == null || Client.GetRoomUser() == null)
                    return;

                if (Client.GetRoleplay().Game == null)
                    return;

                int SpawnX = 0;
                int SpawnY = 0;

                if (Client.GetRoleplay().Game.GetGameMode() == GameMode.ColourWars)
                {
                    SpawnX = Convert.ToInt32(RoleplayData.GetData("colourwarspoint", Name.ToLower() + "spawnx"));
                    SpawnY = Convert.ToInt32(RoleplayData.GetData("colourwarspoint", Name.ToLower() + "spawny"));
                }
                else if (Client.GetRoleplay().Game.GetGameMode() == GameMode.TeamBrawl)
                {
                    SpawnX = Convert.ToInt32(RoleplayData.GetData("teambrawlpoint", Name.ToLower() + "spawnx"));
                    SpawnY = Convert.ToInt32(RoleplayData.GetData("teambrawlpoint", Name.ToLower() + "spawny"));
                }
                else if (Client.GetRoleplay().Game.GetGameMode() == GameMode.MafiaWars)
                {
                    SpawnX = Convert.ToInt32(RoleplayData.GetData("mafiawarspoint", Name.ToLower() + "spawnx"));
                    SpawnY = Convert.ToInt32(RoleplayData.GetData("mafiawarspoint", Name.ToLower() + "spawny"));
                }

                var OldCoord = new Point(Client.GetRoomUser().Coordinate.X, Client.GetRoomUser().Coordinate.Y);
                var NewCoord = new Point(SpawnX, SpawnY);

                Client.GetHabbo().CurrentRoom.GetGameMap().UpdateUserMovement(OldCoord, NewCoord, Client.GetRoomUser());
                Client.GetRoomUser().SetPos(SpawnX, SpawnY, Client.GetHabbo().CurrentRoom.GetGameMap().GetHeightForSquare(NewCoord));
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in SendToPoint() void: " + e);
            }
        }

        public void Notify(string Message)
        {
            lock (this.Members)
            {
                foreach (var Member in this.Members)
                {
                    var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);

                    if (Client == null)
                        continue;

                    if (Client.GetHabbo() == null)
                        continue;

                     Client.SendWhisper(Message, 1);                 
                }
            }
        }
    }
}