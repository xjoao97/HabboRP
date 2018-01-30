using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.VIP
{
    class FacelessCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_faceless"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite que você fique sem rosto!"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            RoomUser User = Session.GetRoomUser();
            if (User == null || User.GetClient() == null)
                return;

            string[] headParts;
            string[] figureParts = Session.GetHabbo().Look.Split('.');
            foreach (string Part in figureParts)
            {
                if (Part.StartsWith("hd"))
                {
                    headParts = Part.Split('-');
                    if (!headParts[1].Equals("99999"))
                        headParts[1] = "99999";
                    else
                        return;

                    Session.GetHabbo().Look = Session.GetHabbo().Look.Replace(Part, "hd-" + headParts[1] + "-" + headParts[2]);
                    break;
                }
            }
            Session.GetHabbo().Look = PlusEnvironment.GetGame().GetAntiMutant().RunLook(Session.GetHabbo().Look);
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `look` = '" + Session.GetHabbo().Look + "' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
            }

            Session.SendWhisper("Você ficou sem rosto!", 1);
            Session.SendMessage(new UserChangeComposer(User, true));
            Session.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(User, false));
            Session.GetRoleplay().OriginalOutfit = Session.GetHabbo().Look;
            return;
        }
    }
}
