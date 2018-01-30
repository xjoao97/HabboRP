
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
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Bots;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class HitCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_hit"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Dá um soco no usuário alvo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.GetRoleplay().LastCommand = ":soco";
                CombatManager.GetCombatType("fist").Execute(Session, null, true);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                RoomUser Bot = Room.GetRoomUserManager().GetBotByName(Params[1]);

                if (Bot != null && Bot.GetBotRoleplay() != null)
                {
                    Session.GetRoleplay().LastCommand = ":soco " + Params[1];
                    CombatManager.GetCombatType("fist").ExecuteBot(Session, Bot.GetBotRoleplay());
                    return;
                }

                Session.GetRoleplay().LastCommand = ":soco " + Params[1];
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            if (Room == null)
            {
                Session.GetRoleplay().LastCommand = ":soco " + Params[1];
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetClient == null)
            {
                Session.GetRoleplay().LastCommand = ":soco " + Params[1];
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetClient.GetHabbo() == null)
            {
                Session.GetRoleplay().LastCommand = ":soco " + Params[1];
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.GetRoleplay().LastCommand = ":soco " + Params[1];
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            Session.GetRoleplay().LastCommand = ":soco " + Params[1];
            CombatManager.GetCombatType("fist").Execute(Session, TargetClient);
        }
    }
}