using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Fleck;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboRoleplay.Web.Util.ChatRoom;
using Newtonsoft.Json;
using Plus.HabboRoleplay.Misc;
using System.Threading;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class DownloadAppCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_websocket_chat_downloadapp"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Descarrega uma aplicação."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Params
            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido, use :baixar whatsapp.", 1);
                return;
            }

            List<string> AvailableApps = new List<string>()
            {
                "whatsapp"
            };

            string App = Convert.ToString(Params[1]).ToLower();

            #endregion

            #region Conditions
            if (Session.GetRoleplay().PhoneApps.Contains(App))
            {
                Session.SendWhisper("Você já possui este aplicativo!", 1);
                return;
            }

            if (Session.GetRoleplay().PhoneType < 1)
            {
                Session.SendWhisper("Você precisa de um Celular para baixar o Aplicativo, vá até a loja de Celulares [ID:25]!", 1);
                return;
            }

            if (Session.GetRoleplay().DownloadingApplication)
            {
                Session.SendWhisper("Você já está baixando um aplicativo! Aguarde até o download finalizar!", 1);
                return;
            }

            if (!AvailableApps.Contains(App))
            {
                Session.SendWhisper("Este é um aplicativo inválido! Aplicações atuais disponíveis: " + String.Join(",", App), 1);
                return;
            }
            #endregion

            #region Execute            

            Session.Shout("*Puxa seu " + RoleplayManager.GetPhoneName(Session) + " e começa a baixar o Aplicativo " + App + "*", 4);

            #region Unequip
            if (Session.GetRoleplay().EquippedWeapon != null)
                Session.GetRoleplay().EquippedWeapon = null;
            #endregion

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().CurrentEffect != 65)
                    Session.GetRoomUser().ApplyEffect(EffectsList.CellPhone);
                Session.GetRoleplay().TextTimer = 500;
            }

            Session.GetRoleplay().DownloadingApplication = true;

            new Thread(() => {           
                Thread.Sleep(((Session.GetHabbo().VIPRank < 2) ? 30000 : 30));

                if (Session != null)
                {
                    if (Session.GetRoleplay() != null)
                    {
                        Session.GetRoleplay().PhoneApps.Add("whatsapp");
                        Session.Shout("*Finaliza o download do Aplicativo " + App + " no meu " + RoleplayManager.GetPhoneName(Session) + "*", 4);
                        Session.GetRoleplay().TextTimer = 0;
                        Session.GetRoleplay().DownloadingApplication = false;

                        if (Session.GetRoomUser() != null)
                        {
                            Session.GetRoomUser().ApplyEffect(0);
                        }
                    }
                }
            }).Start();           

            #endregion

        }

    }
}
