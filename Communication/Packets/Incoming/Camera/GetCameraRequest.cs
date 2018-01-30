using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Web.Script.Serialization;

using Plus.Communication.Packets.Outgoing.Camera;
using Plus.HabboRoleplay.Misc;
using Plus.Database.Interfaces;
using Plus.Core.Net;

namespace Plus.Communication.Packets.Incoming.Camera
{
    class GetCameraRequest : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            try
            {
                int count = Packet.PopInt();
                byte[] bytes = Packet.ReadBytes(count);
                string outData = Deflate(bytes);

                string url = WebManager.HttpPostJson("http://habborpg.com.br/swfs/servercamera/servercamera.php?run", outData);
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                dynamic jsonArray = serializer.Deserialize(outData, typeof(object));;
                string encodedurl = "http://habborpg.com.br/swfs/servercamera/" + url;
                encodedurl = encodedurl.Replace("\n", string.Empty);

                int roomId = jsonArray["roomid"];
                long timeStamp = jsonArray["timestamp"];

                using (IQueryAdapter commitableQueryReactor = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    commitableQueryReactor.SetQuery("INSERT INTO `cms_stories_photos_preview` (`user_id`,`user_name`,`room_id`,`image_preview_url`,`image_url`,`type`,`date`,`tags`) VALUES (@userid,@username,@roomid,@imagepreviewurl,@imageurl,@types,@dates,@tag)");
                    commitableQueryReactor.AddParameter("userid", Session.GetHabbo().Id);
                    commitableQueryReactor.AddParameter("username", Session.GetHabbo().Username);
                    commitableQueryReactor.AddParameter("roomid", roomId);
                    commitableQueryReactor.AddParameter("imagepreviewurl", encodedurl);
                    commitableQueryReactor.AddParameter("imageurl", encodedurl);
                    commitableQueryReactor.AddParameter("types", "PHOTO");
                    commitableQueryReactor.AddParameter("dates", timeStamp);
                    commitableQueryReactor.AddParameter("tag", "");
                    commitableQueryReactor.RunQuery();
                }

                Session.SendMessage(new CameraStorageUrlMessageComposer(url));
            }
            catch
            {
                Session.SendNotification("Esta imagem tem muito pixel, pegue a foto em uma área com menos tamanho!");
                return;
            }
        }

        internal string Deflate(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes, 2, bytes.Length - 2))
            {
                using (DeflateStream inflater = new DeflateStream(stream, CompressionMode.Decompress))
                {
                    using (StreamReader streamReader = new StreamReader(inflater))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}
