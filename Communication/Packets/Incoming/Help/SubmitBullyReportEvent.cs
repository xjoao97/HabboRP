using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Help;

namespace Plus.Communication.Packets.Incoming.Help
{
    class SubmitBullyReportEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            //0 = sent, 1 = blocked, 2 = no chat, 3 = already reported.
            if (Session == null)
                return;

            int UserId = Packet.PopInt();
            if (UserId == Session.GetHabbo().Id)//Hax
                return;

            if (Session.GetHabbo().AdvertisingReportedBlocked)
            {
                Session.SendMessage(new SubmitBullyReportComposer(1));//This user is blocked from reporting.
                return;
            }

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToInt32(UserId));
            if (Client == null)
            {
                Session.SendMessage(new SubmitBullyReportComposer(0));//Just say it's sent, the user isn't found.
                return;
            }

            if (Session.GetHabbo().LastAdvertiseReport > PlusEnvironment.GetUnixTimestamp())
            {
                Session.SendNotification("Os relatórios só podem ser enviados por 5 minutos!");
                return;
            }

            if (Client.GetHabbo().GetPermissions().HasRight("mod_tool"))//Reporting staff, nope!
            {
                Session.SendNotification("Desculpe, você não pode denunciar membros da equipe através desta ferramenta.");
                return;
            }

            //This user hasn't even said a word, nope!
            if (!Client.GetHabbo().HasSpoken)
            {
                Session.SendMessage(new SubmitBullyReportComposer(2));
                return;
            }

            //Already reported, nope.
            if (Client.GetHabbo().AdvertisingReported && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendMessage(new SubmitBullyReportComposer(3));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                Session.GetHabbo().LastAdvertiseReport = PlusEnvironment.GetUnixTimestamp() + 300;
            else
                Session.GetHabbo().LastAdvertiseReport = PlusEnvironment.GetUnixTimestamp();

            Client.GetHabbo().AdvertisingReported = true;
            Session.SendMessage(new SubmitBullyReportComposer(0));
            PlusEnvironment.GetGame().GetClientManager().ModAlert("Nova denunciada enviada de " + Client.GetHabbo().Username + ", a denúncia foi contra " + Session.GetHabbo().Username +".");
            PlusEnvironment.GetGame().GetClientManager().DoAdvertisingReport(Session, Client);     
            return;
        }
    }
}