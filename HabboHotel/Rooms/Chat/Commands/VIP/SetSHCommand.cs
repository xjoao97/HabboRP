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
    class SetSHCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_set_stack_height"; }
        }

        public string Parameters
        {
            get { return "%altura%"; }
        }

        public string Description
        {
            get { return "Defina uma altura para móveis serem empilhados."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir uma altura!", 1);
                return;
            }

            double StackHeight = 0;
            if (!double.TryParse(Params[1].ToString(), out StackHeight))
            {
                Session.SendWhisper("Por favor, digite um número válido.", 1);
                return;
            }

            Session.GetHabbo().DebugStacking = true;
            Session.GetHabbo().StackHeight = StackHeight;
            Session.SendWhisper("Altura alterada para: " + StackHeight + "", 1);
            return;
        }
    }
}