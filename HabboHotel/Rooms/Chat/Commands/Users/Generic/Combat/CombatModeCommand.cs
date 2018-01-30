using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class CombatModeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_mode"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Ativa o modo de combate (clicar em um usuário irá atacá-lo)."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetRoleplay().CombatMode = !Session.GetRoleplay().CombatMode;
            Session.GetRoleplay().InCombat = false;
            Session.SendWhisper("O modo combate agora está " + (Session.GetRoleplay().CombatMode == true ? "ativado!" : "desativado!"), 1);
            return;
        }
    }
}