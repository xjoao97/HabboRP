using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class CoordsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_coords"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Usado para obter sua posição atual dentro da sala."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser ThisUser = Session.GetRoomUser();
            if (ThisUser == null)
                return;

            Session.SendNotification("- X: " + ThisUser.X + "\n - Y: " + ThisUser.Y + "\n - Z: " + ThisUser.Z + "\n - Rot: " + ThisUser.RotBody + ", sqState: " + Room.GetGameMap().GameMap[ThisUser.X, ThisUser.Y].ToString() + "\n\n - Quarto ID: " + Session.GetHabbo().CurrentRoomId);                           
        }
    }
}
