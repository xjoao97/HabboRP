using System;
using System.Linq;
using System.Text;
using System.Drawing;
using Plus.Utilities;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System.Threading;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Criminal
{
    class SnortCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_criminal_activity_snort"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Cheira 10g de cocaína para ficar acelerado."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetRoleplay().Cocaine < 15)
            {
                Session.SendWhisper("Você precisa de pelo menos 15g de cocaína para cheirar!", 1);
                return;
            }
            
            if (Session.GetRoleplay().TryGetCooldown("cocaine", false))
            {
                Session.SendWhisper("Você já está drogado de cocaína!", 1);
                return;
            }

            if (Session.GetRoleplay().Game != null)
            {
                Session.SendWhisper("Você não pode usar drogas enquanto estiver dentro de um evento!", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode completar esta ação enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode completar esta ação enquanto está preso!", 1);
                return;
            }
            #endregion

            #region Execute
            Session.GetRoleplay().Cocaine -= 15;
            Session.GetRoleplay().HighOffCocaine = true;
            Session.GetRoleplay().CooldownManager.CreateCooldown("cocaine", 1000, 10);
            Session.Shout("*Pega 15g de cocaína e cheira rapidamente*", 4);

            if (!Session.GetRoleplay().WantedFor.Contains("fumando substancias ilegais"))
                Session.GetRoleplay().WantedFor = Session.GetRoleplay().WantedFor + "cheirar cocaína, ";
            return;
            #endregion
        }
    }
}