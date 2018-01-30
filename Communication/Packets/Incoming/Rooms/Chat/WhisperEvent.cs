using System;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Utilities;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    public class WhisperEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Opa, você atualmente está mudo.", 1);
                return;
            }

            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
                return;

            if (Session.LoggingOut)
                return;

            string[] Params = Packet.PopString().Split(' ');

            if (Params.Length < 2)
                return;

            string ToUser = Params[0];
            string Message = CommandManager.MergeParams(Params, 1);

            int Colour = Packet.PopInt();

            if (Colour != 0 && !Session.GetHabbo().GetPermissions().HasRight("use_any_bubble"))
                Colour = 0;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            if (User == null)
                return;

            RoomUser User2 = Room.GetRoomUserManager().GetRoomUserByHabbo(ToUser);
            if (User2 == null)
                return;

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendMessage(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);

            ChatStyle Style = null;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) || (Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight)))
                Colour = 0;

            User.LastBubble = Session.GetHabbo().CustomBubbleId == 0 ? Colour : Session.GetHabbo().CustomBubbleId;

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                int MuteTime;
                if (User.IncrementAndCheckFlood(out MuteTime))
                {
                    Session.SendMessage(new FloodControlComposer(MuteTime));
                    return;
                }
            }

            if (!User2.GetClient().GetHabbo().ReceiveWhispers && !Session.GetHabbo().GetPermissions().HasRight("room_whisper_override"))
            {
                Session.SendWhisper("Opa, este usuário desativou os sussurros!", 1);
                return;
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs().StoreChatlog(new Plus.HabboHotel.Rooms.Chat.Logs.ChatlogEntry(Session.GetHabbo().Id, Room.Id, "<Sussurro para " + ToUser + ">: " + Message, UnixTimestamp.GetNow(), Session.GetHabbo(), Room));

            if (!Session.GetHabbo().GetPermissions().HasRight("advertisement_filter_override"))
            {
                string Phrase = "";
                if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(Message, out Phrase))
                {
                    Session.GetHabbo().AdvertisingStrikes++;

                    if (Session.GetHabbo().AdvertisingStrikes < 2)
                    {
                        Session.SendMessage(new RoomNotificationComposer("Perigo!", "Por favor, abstenha de anunciar outros sites que não são endossados, afiliados ou oferecidos pelo HabboRPG.Você ficará mudo se você fizer isso de novo!<br><br>Frase da Lista Negra: '" + Phrase + "'", "frank10", "ok", "event:"));
                        return;
                    }

                    if (Session.GetHabbo().AdvertisingStrikes >= 2)
                    {
                        Session.GetHabbo().TimeMuted = 3600;

                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `users` SET `time_muted` = '3600' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                        }

                        Session.SendMessage(new RoomNotificationComposer("Você ficou mudo!", "Desculpe, mas você foi automaticamente mutado por divulgar '" + Phrase + "'.<br><br>A equipe de moderação foi notificada e ações serão tomadas dentro de sua conta!", "frank10", "ok", "event:"));

                        List<string> Messages = new List<string>();
                        Messages.Add(Message);
                        PlusEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, 9, Session.GetHabbo().Id, "[Servidor] O cidadão já recebeu uma advertência " + Phrase + ".", Messages);
                        return;
                    }

                    return;
                }
            }

            if (Session.GetRoleplay() != null)
            {
                if (!Session.GetRoleplay().IsWorking)
                    User.UnIdle();
            }
            else
                User.UnIdle();

            User.SendNameColourPacket();
            if (Session.GetRoleplay() != null)
            {
                if (User.GetClient().GetRoleplay().IsWorking && HabboHotel.Groups.GroupManager.HasJobCommand(Session, "guide"))
                {
                    if (User.GetClient().GetHabbo().Translating)
                    {
                        string LG1 = User.GetClient().GetHabbo().FromLanguage.ToLower();
                        string LG2 = User.GetClient().GetHabbo().ToLanguage.ToLower();

                        User.GetClient().SendMessage(new WhisperComposer(User.VirtualId, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", 0, 37));
                    }
                    else
                        User.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, 37));
                }
                else
                {
                    if (User.GetClient().GetHabbo().Translating)
                    {
                        string LG1 = User.GetClient().GetHabbo().FromLanguage.ToLower();
                        string LG2 = User.GetClient().GetHabbo().ToLanguage.ToLower();

                        User.GetClient().SendMessage(new WhisperComposer(User.VirtualId, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", 0, User.LastBubble));
                    }
                    else
                        User.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));
                }
            }
            else
            {
                if (User.GetClient().GetHabbo().Translating)
                {
                    string LG1 = User.GetClient().GetHabbo().FromLanguage.ToLower();
                    string LG2 = User.GetClient().GetHabbo().ToLanguage.ToLower();

                    User.GetClient().SendMessage(new WhisperComposer(User.VirtualId, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", 0, User.LastBubble));
                }
                else
                    User.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));
            }

            if (User2 != null && !User2.IsBot && User2.UserId != User.UserId)
            {
                if (!User2.GetClient().GetHabbo().MutedUsers.Contains(Session.GetHabbo().Id))
                {
                    if (User.GetClient().GetHabbo().Translating)
                    {
                        string LG1 = User.GetClient().GetHabbo().FromLanguage.ToLower();
                        string LG2 = User.GetClient().GetHabbo().ToLanguage.ToLower();

                        User2.GetClient().SendMessage(new WhisperComposer(User.VirtualId, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", 0, User.LastBubble));
                    }
                    else
                        User2.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));
                }
            }

            List<RoomUser> ToNotify = Room.GetRoomUserManager().GetRoomUserBySpecialRights();
            if (ToNotify.Count > 0)
            {
                foreach (RoomUser user in ToNotify)
                {
                    if (user != null && user.HabboId != User2.HabboId && user.HabboId != User.HabboId)
                    {
                        if (user.GetClient() != null && user.GetClient().GetHabbo() != null && !user.GetClient().GetHabbo().IgnorePublicWhispers)
                        {
                            if (User.GetClient().GetHabbo().Translating)
                            {
                                string LG1 = User.GetClient().GetHabbo().FromLanguage.ToLower();
                                string LG2 = User.GetClient().GetHabbo().ToLanguage.ToLower();

                                user.GetClient().SendMessage(new WhisperComposer(User.VirtualId, "[Sussurro para " + ToUser + "] " + PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", 0, User.LastBubble));
                            }
                            else
                                user.GetClient().SendMessage(new WhisperComposer(User.VirtualId, "[Sussurro para " + ToUser + "] " + Message, 0, User.LastBubble));
                        }
                    }
                }
            }
            User.SendNamePacket();
        }
    }
}