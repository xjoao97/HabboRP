using System;
using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Moderation;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    public class ShoutEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (Session.LoggingOut)
                return;

            string Message = StringCharFilter.Escape(Packet.PopString(), false, Session);
            if (Message.Length > 150)
                Message = Message.Substring(0, 150);

            int Colour = Packet.PopInt();

            if (Colour != 0 && !User.GetClient().GetHabbo().GetPermissions().HasRight("use_any_bubble"))
                Colour = 0;

            ChatStyle Style = null;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) || (Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight)))
                Colour = 0;

            User.LastBubble = Session.GetHabbo().CustomBubbleId == 0 ? Colour : Session.GetHabbo().CustomBubbleId;

            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
                return;

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendMessage(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("room_ignore_mute") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Opa, atualmente você está mudo.", 1);
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                int MuteTime;
                if (Message.StartsWith(":", StringComparison.CurrentCulture) && PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, Message))
                    return;
                else if (User.IncrementAndCheckFlood(out MuteTime))
                {
                    Session.SendMessage(new FloodControlComposer(MuteTime));
                    return;
                }
            }

            if (Message.Equals("x"))
            {
                if (Session.GetRoleplay().LastCommand != "")
                    Message = Session.GetRoleplay().LastCommand.ToString();
            }

            if (Message.StartsWith(":", StringComparison.CurrentCulture) && PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, Message))
                return;

            PlusEnvironment.GetGame().GetChatManager().GetLogs().StoreChatlog(new Plus.HabboHotel.Rooms.Chat.Logs.ChatlogEntry(Session.GetHabbo().Id, Room.Id, Message, UnixTimestamp.GetNow(), Session.GetHabbo(), Room));

            if (!Session.GetHabbo().GetPermissions().HasRight("advertisement_filter_override"))
            {
                string Phrase = "";
                if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(Message, out Phrase))
                {
                    Session.GetHabbo().AdvertisingStrikes++;

                    if (Session.GetHabbo().AdvertisingStrikes < 2)
                    {
                        Session.SendMessage(new RoomNotificationComposer("Atenção!", "Por favor, evite de anunciar outros sites que não são permitidos, afiliados ou oferecidos pelo HabboRPG. Você ficará mudo se você fizer isso de novo!<br><br>Frase da lista Negra: '" + Phrase + "'", "frank10", "ok", "event:"));
                        return;
                    }

                    if (Session.GetHabbo().AdvertisingStrikes >= 2)
                    {
                        Session.GetHabbo().TimeMuted = 3600;

                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `users` SET `time_muted` = '3600' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                        }

                        Session.SendMessage(new RoomNotificationComposer("Você foi mutado!", "Desculpe, mas você foi automaticamente mutadod por divulgar '" + Phrase + "'.<br><br>A equipe de moderação foi notificada e ações serão tomadas dentro de sua conta", "frank10", "ok", "event:"));

                        List<string> Messages = new List<string>();
                        Messages.Add(Message);
                        PlusEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, 9, Session.GetHabbo().Id, "[Servidor] O civil já recebeu uma advertência " + Phrase + ".", Messages);
                        return;
                    }

                    return;
                }
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);

            if (Message.ToLower().Equals("o/"))
            {
                Room.SendMessage(new ActionComposer(User.VirtualId, 1));
                return;
            }

            if (Message.ToLower().Equals("_b"))
            {
                Room.SendMessage(new ActionComposer(User.VirtualId, 7));
                return;
            }

            User.UnIdle();

            if (Session.GetRoleplay() != null)
            {
                if (Session.GetRoleplay().IsWorking && HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide"))
                    User.OnChat(37, Message, true);
                else if (Session.GetRoleplay().StaffOnDuty && Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    User.OnChat(23, Message, true);
                else if (Session.GetRoleplay().AmbassadorOnDuty && Session.GetHabbo().GetPermissions().HasRight("ambassador"))
                    User.OnChat(37, Message, true);

                // Roleplay
                else if (Session.GetRoleplay().CurHealth > 25 && Session.GetRoleplay().CurHealth <= 40 && !Session.GetRoleplay().IsDead)
                    User.OnChat(5, Message, true);
                else if (Session.GetRoleplay().CurHealth <= 25 && !Session.GetRoleplay().IsDead)
                    User.OnChat(3, Message, true);
                else if (Session.GetRoleplay().IsDead)
                    User.OnChat(3, "[ " + Message + " ]", true);
                else
                    User.OnChat(User.LastBubble, Message, true);
            }
            else
                User.OnChat(User.LastBubble, Message, true);
        }
    }
}