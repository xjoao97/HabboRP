using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.VIP
{
    class StopSHCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_set_stack_height_undo"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desliga a altura da pilha de depuração de :setsh."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().DebugStacking = false;
            Session.GetHabbo().StackHeight = 0;
            Session.SendWhisper("Você desativou o empilhamento.", 1);
            return;
        }
    }
}