using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class CheckMinutesCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_checkminutes"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Veja o tempo de trabalho de um empregado da sua empresa."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu ded inserir um usuário!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontar este usuário, talvez ele esteja offline.", 1);
                return;
            }
            int JobRank = TargetClient.GetRoleplay().JobRank;

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode fazer isso enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode fazer isso enquanto está preso!", 1);
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (!GroupManager.HasJobCommand(Session, "checkminutes"))
                {
                    Session.SendWhisper("Você deve ter um cargo maior para usar este comando da empresa!", 1);
                    return;
                }
                if (Session.GetRoleplay().JobId != TargetClient.GetRoleplay().JobId)
                {
                    Session.SendWhisper("Este cidadão não trabalha para você!", 1);
                    return;
                }
            }

            if (Session.GetRoleplay().TryGetCooldown("checkminutes"))
                return;

            #endregion

            #region Execute

            Session.SendWhisper(TargetClient.GetHabbo().Username + " trabalhou na empresa '" + GroupManager.GetJob(TargetClient.GetRoleplay().JobId).Name + "' por " + String.Format("{0:N0}", TargetClient.GetRoleplay().TimeWorked) + " minutos!", 1);
            Session.GetRoleplay().CooldownManager.CreateCooldown("checkminutes", 1000, 5);

            #endregion
        }
    }
}