using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Groups;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class UpdateGroupIdentityEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            string Name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            string Desc = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());

            Group Group = null;

            if (GroupId < 1000)
                Group = GroupManager.GetJob(GroupId);
            else
                Group = GroupManager.GetGang(GroupId);

            if (Group == null)
                return;

            if (Group.CreatorId != Session.GetHabbo().Id && !Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Group.Id < 1000)
                    dbClient.SetQuery("UPDATE `rp_jobs` SET `name`= @name, `desc` = @desc WHERE `id` = '" + GroupId + "'");
                else
                    dbClient.SetQuery("UPDATE `rp_gangs` SET `name`= @name, `desc` = @desc WHERE `id` = '" + GroupId + "'");
                dbClient.AddParameter("name", Name);
                dbClient.AddParameter("desc", Desc);
                dbClient.RunQuery();
            }

            Group.Name = Name;
            Group.Description = Desc;

            Session.SendMessage(new GroupInfoComposer(Group, Session));
        }
    }
}
