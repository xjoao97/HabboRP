using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Items;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Camera;

namespace Plus.Communication.Packets.Incoming.Camera
{
    class HabboCameraMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            using (IQueryAdapter commitableQueryReactor = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                commitableQueryReactor.SetQuery("SELECT * FROM `cms_stories_photos_preview` WHERE `user_id` = '" + Session.GetHabbo().Id + "' AND `type` = 'PHOTO' ORDER BY `id` DESC LIMIT 1");

                DataTable table = commitableQueryReactor.getTable();

                foreach (DataRow dataRow in table.Rows)
                {
                    object date = dataRow["date"];
                    object room = dataRow["room_id"];
                    object photo = dataRow["id"];
                    object image = dataRow["image_url"];

                    using (IQueryAdapter commitableQueryReactor2 = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        commitableQueryReactor2.SetQuery("INSERT INTO cms_stories_photos (`user_id`,`user_name`,`room_id`,`image_preview_url`,`image_url`,`type`,`date`,`tags`) VALUES (@user_id,@user_name,@room_id,@image_url,@image_url,@type,@date,@tags)");
                        commitableQueryReactor2.AddParameter("user_id", Session.GetHabbo().Id);
                        commitableQueryReactor2.AddParameter("user_name", Session.GetHabbo().Username);
                        commitableQueryReactor2.AddParameter("room_id", room);
                        commitableQueryReactor2.AddParameter("image_url", image);
                        commitableQueryReactor2.AddParameter("type", "PHOTO");
                        commitableQueryReactor2.AddParameter("date", date);
                        commitableQueryReactor2.AddParameter("tags", "");
                        commitableQueryReactor2.RunQuery();

                        string newPhotoData = "{\"t\":" + date + ",\"u\":\"" + photo + "\",\"m\":\"\",\"s\":" + room + ",\"w\":\"" + image + "\"}";

                        // 5235 = large photo size, 7480 = small photo size
                        Item item = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, Convert.ToInt32(RoleplayData.GetData("camera", "small")), newPhotoData, 0, true, false, 0, 0);

                        Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                        Session.GetHabbo().Credits -= 2;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Session.GetHabbo().GetInventoryComponent().SendNewItems(item.Id);
                    }
                }
            }
            Session.SendMessage(new CameraPurchaseOkComposer());
        }
    }
}
