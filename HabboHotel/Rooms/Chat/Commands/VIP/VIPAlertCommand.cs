using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.VIP
{
    class VIPAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_alert_vip"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Envia uma mensagem digitada por você para todos os usuários vip online."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para enviar.", 1);
                return;
            }

            if (Session.GetRoleplay().DisableVIPA)
            {
                Session.SendWhisper("Você tem alertas VIP desativados! Digite ':ligarva' para re-ativar!", 1);
                return;
            }

            if (Session.GetRoleplay().VIPBanned > 0)
            {
                int TotalSeconds = Session.GetRoleplay().VIPBanned;
                int Minutes = Convert.ToInt32(Math.Floor((double)TotalSeconds / 60));
                int Seconds = TotalSeconds - (Minutes * 60);

                Session.SendWhisper("Você foi proibido de usar Alerta VIP! Seu ban expira em: " + Minutes + " minutos e " + Seconds + " segundos!", 1);
                return;
            }

            if (RoleplayManager.NewVIPAlert)
            {
                if (Session.GetRoleplay().BannedFromChatting)
                {
                    Session.SendWhisper("Você está impedido de usar alertas VIP!", 1);
                    return;
                }

                if (!Session.GetRoleplay().PhoneApps.Contains("whatsapp"))
                {
                    Session.SendWhisper("Você precisa do Aplicativo Whatsapp para fazer isso! Digite :baixar whatsapp.", 1);
                    return;
                }

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_chatroom", Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<object, object>()
                {
                    { "action", "requestjoin" },
                    { "chatname", "vip-chat" },

                }));
            }
            else
            {
                string Message = CommandManager.MergeParams(Params, 1);
                if (Session.GetHabbo().Translating)
                {
                    string LG1 = Session.GetHabbo().FromLanguage.ToLower();
                    string LG2 = Session.GetHabbo().ToLanguage.ToLower();

                    PlusEnvironment.GetGame().GetClientManager().VIPWhisperAlert(PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", Session);
                }
                else
                    PlusEnvironment.GetGame().GetClientManager().VIPWhisperAlert(Message, Session);
            }
        }
    }
}
