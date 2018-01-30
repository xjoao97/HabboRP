using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class ColourChangeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_colour_change"; }
        }

        public string Parameters
        {
            get { return "%colour%"; }
        }

        public string Description
        {
            get { return "Permite alterar a cor dos seus nomes."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Você não inseriu um código de cores para!", 1);
                return;
            }

            if (Session.GetHabbo() == null)
                return;

            if (Params[1].ToLower() == "remover")
            {
                Session.GetHabbo().Colour = string.Empty;
                UpdateDatabase(Session);
            }
            else
            {
                Session.GetHabbo().Colour = Params[1];
                UpdateDatabase(Session);
            }

            Session.SendWhisper("Você alterou com êxito o seu código de cores!", 1);
            return;
        }

        public void UpdateDatabase(GameClients.GameClient Session)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `colour` = '" + Session.GetHabbo().Colour + "' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
            }
        }
    }
}