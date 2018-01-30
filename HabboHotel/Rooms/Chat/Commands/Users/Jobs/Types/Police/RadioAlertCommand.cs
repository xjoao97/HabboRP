using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class RadioAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_alert_radio"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Envia uma mensagem digitada por todos os policiais online."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para enviar.", 1);
                return;
            }

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

            if (Session.GetRoleplay().DisableRadio)
            {
                Session.SendWhisper("Você desativou os alertas de rádio! Digite ':adradio' para reativá-los!", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);

            if (Session.GetHabbo().Translating)
            {
                string LG1 = Session.GetHabbo().FromLanguage.ToLower();
                string LG2 = Session.GetHabbo().ToLanguage.ToLower();

                PlusEnvironment.GetGame().GetClientManager().RadioAlert(PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", Session);
            }
            else
                PlusEnvironment.GetGame().GetClientManager().RadioAlert(Message, Session);
        }
    }
}
