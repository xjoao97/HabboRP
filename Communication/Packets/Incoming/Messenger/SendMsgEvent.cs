using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    class SendMsgEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            int userId = Packet.PopInt();

            if (userId == 0 || userId == Session.GetHabbo().Id)
                return;

            string message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            if (string.IsNullOrWhiteSpace(message))
                return;
            
            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendNotification("Opa, você está mudo - você não pode enviar mensagens.");
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("advertisement_filter_override"))
            {
                string Phrase = "";
                if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(message, out Phrase))
                {
                    Session.GetHabbo().AdvertisingStrikes++;

                    if (Session.GetHabbo().AdvertisingStrikes < 2)
                    {
                        Session.SendMessage(new RoomNotificationComposer("Atenção!", "Por favor, pare de anunciar outros sites que não são afiliados ou oferecidos pelo HabboRPG. Você será silenciado se você fizer isso de novo!<br><br>Frase da Lista Negra: '" + Phrase + "'", "frank10", "ok", "event:"));
                        return;
                    }

                    if (Session.GetHabbo().AdvertisingStrikes >= 2)
                    {
                        Session.GetHabbo().TimeMuted = 3600;

                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `users` SET `time_muted` = '3600' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                        }

                        Session.SendMessage(new RoomNotificationComposer("Você ficou mudo!", "Desculpe, mas você foi automaticamente silenciado por divulgar o hotel '" + Phrase + "'.<br><br>A equipe de moderação foi notificada e ações serão tomadas dentro de sua conta!", "frank10", "ok", "event:"));

                        List<string> Messages = new List<string>();
                        Messages.Add(message);
                        PlusEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, 9, Session.GetHabbo().Id, "[Servidor] O civil já recebeu uma advertência; " + Phrase + ".", Messages);
                        return;
                    }

                    return;
                }
            }

            int TextCost;
            bool CanAfford;

            if (Session.GetRoleplay().PhoneType == 1)
                TextCost = 3;
            else if (Session.GetRoleplay().PhoneType == 2)
                TextCost = 2;
            else if (Session.GetRoleplay().PhoneType == 3)
                TextCost = 1;
            else
                TextCost = 3;

            if (Session.GetHabbo().Duckets < TextCost)
                CanAfford = false;
            else
                CanAfford = true;

            Session.GetHabbo().GetMessenger().SendInstantMessage(userId, message, CanAfford, TextCost);

        }
    }
}