using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Support;


using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    class ModerationBanEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_soft_ban"))
                return;

            int UserId = Packet.PopInt();
            string Message = Packet.PopString();
            double Length = (Packet.PopInt() * 3600) + PlusEnvironment.GetUnixTimestamp();
            string Unknown1 = Packet.PopString();
            string Unknown2 = Packet.PopString();
            bool IPBan = Packet.PopBoolean();
            bool MachineBan = Packet.PopBoolean();

            if (MachineBan)
                IPBan = false;

            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);

            if (Habbo == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário no banco de dados.");
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_tool") && !Session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                Session.SendWhisper("Opa, você não pode banir esse usuário.");
                return;
            }

            Message = (Message != null ? Message : "Sem razão específica, mas provavelmente fez merda.");

            string Username = Habbo.Username;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
            }

            if (IPBan == false && MachineBan == false)
                PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.USERNAME, Habbo.Username, Message, Length);
            else if (IPBan == true)
                PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.IP, Habbo.Username, Message, Length);
            else if (MachineBan == true)
            {
                PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.IP, Habbo.Username, Message, Length);
                PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.USERNAME, Habbo.Username, Message, Length);
                PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.MACHINE, Habbo.Username, Message, Length);
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Habbo.Username);
            if (TargetClient != null)
            {
                TargetClient.Disconnect(true);
            }
        }
    }
}