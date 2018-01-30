using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.Database.Interfaces;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    class MuteUserEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            int UserId = Packet.PopInt();
            int RoomId = Packet.PopInt();
            int Time = Packet.PopInt();

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (((Room.WhoCanMute == 0 && !Room.CheckRights(Session, true) && Room.Group == null) || (Room.WhoCanMute == 1 && !Room.CheckRights(Session)) && Room.Group == null && !Session.GetHabbo().GetPermissions().HasRight("ambassador")) || (Room.Group != null && !Room.CheckRights(Session, false, true) && !Session.GetHabbo().GetPermissions().HasRight("ambassador")))
                return;

            RoomUser Target = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
            if (Target == null)
                return;
            else if (Target.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            if (Session.GetHabbo().GetPermissions().HasRight("ambassador"))
            {
                if (Target.GetClient().GetHabbo().TimeMuted > 0)
                {
                    Session.SendWhisper("Desculpe, mas este usuário está em silêncio por " + String.Format("{0:N0}", Math.Floor((Target.GetClient().GetHabbo().TimeMuted / 60))) + " minuto(s) - então você não pode mutá-lo novamente.", 1);
                    return;
                }

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `users` SET `time_muted` = '" + (Time * 60) + "' WHERE `id` = '" + Target.GetClient().GetHabbo().Id + "' LIMIT 1");
                }

                Target.GetClient().GetHabbo().TimeMuted = (Time * 60);

                Target.GetClient().SendNotification("Você foi silenciado por " + String.Format("{0:N0}", Time) + " minutos por um embaixador porque seu comportamento foi apropriado.");
                Session.SendWhisper("Você silenciou com sucesso " + Target.GetClient().GetHabbo().Username + " por " + String.Format("{0:N0}", Time) + " minuto(s).", 1);

                PlusEnvironment.GetGame().GetChatManager().GetCommands().LogCommand(Session.GetHabbo().Id, "mute " + Target.GetClient().GetHabbo().Username + " " + Time, Session.GetHabbo().MachineId, "ambassador");
                return;
            }
            else
            {
                if (Room.MutedUsers.ContainsKey(UserId))
                {
                    if (Room.MutedUsers[UserId] < PlusEnvironment.GetUnixTimestamp())
                        Room.MutedUsers.Remove(UserId);
                    else
                        return;
                }

                Room.MutedUsers.Add(UserId, (PlusEnvironment.GetUnixTimestamp() + (Time * 60)));

                Target.GetClient().SendWhisper("O proprietário do quarto silenciou você por " + Time + " minutos!", 1);
                //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModMuteSeen", 1);
            }
        }
    }
}
