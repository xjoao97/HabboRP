using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class WeaponsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_stats_weapons"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Fornece uma lista de todas as armas que você possui."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetRoleplay().OwnedWeapons.Count <= 0)
            {
                Session.SendWhisper("Você não possui armas!", 1);
                return;
            }

            StringBuilder Message = new StringBuilder().Append("<----- Suas armas ----->\n\n");

            lock (Session.GetRoleplay().OwnedWeapons.Values)
            {
                foreach (Weapon Weapon in Session.GetRoleplay().OwnedWeapons.Values)
                {
                    Message.Append(Weapon.PublicName + " ---> Distância: " + Weapon.Range + " e Damage: " + Weapon.MinDamage + " - " + Weapon.MaxDamage + ".\n\n");
                    //Message.Append("Bullets: " + Weapon.Clip + "/" + Weapon.ClipSize + " inside the clip.\n\n");
                }
            }
            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
        }
    }
}