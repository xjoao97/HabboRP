using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class StopTranslateCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_translate_undo"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Interrompe a tradução da mensagem."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!Session.GetHabbo().Translating)
            {
                Session.SendWhisper("Você já possui a tradução desativada!", 1);
                return;
            }

            Session.SendWhisper("Parou de traduzir de " + Session.GetHabbo().FromLanguage.ToUpper() + " para " + Session.GetHabbo().ToLanguage.ToUpper() + "!", 1);

            Session.GetHabbo().Translating = false;
            Session.GetHabbo().FromLanguage = "";
            Session.GetHabbo().ToLanguage = "";
        }
    }
}