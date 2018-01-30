using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Bots.Types;
using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Items;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Core;
using Plus.HabboHotel.Pathfinding;
using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using System.Collections.Concurrent;


namespace Plus.HabboRoleplay.Bots
{
    public class RoleplayBotResponse
    {
        public string Message;
        public string Response;
        public int Bubble;
        public string Type;

        public RoleplayBotResponse(string Message, string Response, int Bubble, string Type)
        {
            this.Message = Message;
            this.Response = Response;
            this.Bubble = Bubble;
            this.Type = Type;
        }
    }
}