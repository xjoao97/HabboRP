using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class StopTaxiCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_taxi_stop"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Para a chamada para o táxi se você mudar de idéia."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!Session.GetRoleplay().InsideTaxi)
            {
                Session.SendWhisper("Você não está dentro de um táxi!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("ptaxi", true))
                return;

            bool IsVip = Session.GetHabbo().VIPRank < 1 ? false : true;
            string TaxiText = IsVip ? " [Uber]" : "";

            Session.Shout("*Cancela o seu Taxi" + TaxiText + " antes que ele chegue*", 4);
            Session.GetRoleplay().InsideTaxi = false;
            Session.GetRoleplay().CooldownManager.CreateCooldown("ptaxi", 1000, 5);
        }
    }
}