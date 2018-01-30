using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class BackupCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_backup"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Solicita ajuda a todos os policiais online."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Group Job = GroupManager.GetJob(Session.GetRoleplay().JobId);

            if (Job == null)
            {
                Session.SendWhisper("Você está desempregado!", 1);
                return;
            }

            if (Job.Id <= 0)
            {
                Session.SendWhisper("Você está desempregado!", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "radio") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Apenas policiais podem usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("pajuda"))
                return;

            PlusEnvironment.GetGame().GetClientManager().JailAlert(Session.GetHabbo().Username + " está pedido ajuda no [" + Room.Name + " ID: " + Room.Id + "] Vá lá rapidamente!");
            Session.GetRoleplay().CooldownManager.CreateCooldown("pajuda", 1000, 5);
        }
    }
}
