using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Core.Net;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Camera;

namespace Plus.Communication.Packets.Incoming.Camera
{
    class SaveRoomThumbnailMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            try
            {
                int count = Packet.PopInt();

                byte[] bytes = Packet.ReadBytes(count);
                string outData = Deflate(bytes);

                string url = WebManager.HttpPostJson("http://habborpg.com.br/swfs/servercamera/servercamera.php?room", outData);

                Session.SendMessage(new ThumbnailSuccessMessageComposer());
            }
            catch
            {
                Session.SendNotification("Por favor, tente novamente, a área da foto tem muito pixel.");
            }
        }

        internal string Deflate(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes, 2, bytes.Length - 2))

            using (DeflateStream inflater = new DeflateStream(stream, CompressionMode.Decompress))

            using (StreamReader streamReader = new StreamReader(inflater))
                return streamReader.ReadToEnd();
        }

    }
}
