using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Basic
{
    class MeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_me"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Roleplay with this command."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para enviar.", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("atencao"))
                return;

            #endregion

            #region Execute
            
            string Message = CommandManager.MergeParams(Params, 1);

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);

            Session.Shout("*" + Message + "*", 3);
            if (Session == null)
                return;
            if (Session.GetRoleplay() == null)
                return;
            if (Session.GetRoleplay().CooldownManager == null)
                return;
            Session.GetRoleplay().CooldownManager.CreateCooldown("atencao", 1000, 3);

            #endregion
        }
    }
}