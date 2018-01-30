using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class StopWorkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_work_stop"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Parar de trabalhar."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você não está trabalhando!", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode parar de trabalhar enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode parar de trabalhar enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("stopwork", true))
                return;

            #endregion

            if (GroupManager.HasJobCommand(Session, "guide"))
            {
                if (Session.GetRoleplay().GuideOtherUser != null)
                    return;

                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                return;
            }

            WorkManager.RemoveWorkerFromList(Session);
            Session.GetRoleplay().IsWorking = false;
            Session.GetHabbo().Poof();
            Session.GetRoleplay().CooldownManager.CreateCooldown("stopwork", 1000, 10);
        }
    }
}