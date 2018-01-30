using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupCreationWindowComposer : ServerPacket
    {
        public GroupCreationWindowComposer(ICollection<RoomData> Rooms)
            : base(ServerPacketHeader.GroupCreationWindowMessageComposer)
        {
            base.WriteInteger(PlusStaticGameSettings.GroupPurchaseAmount);//Price

            /*
            base.WriteInteger(Rooms.Count);//Room count that the user has.
            foreach (RoomData Room in Rooms)
            {
                base.WriteInteger(Room.Id);//Room Id
                base.WriteString(Room.Name);//Room Name
                base.WriteBoolean(false);//What?
            }
            */

            #region Roleplay Room Settings
            base.WriteInteger(1);
            base.WriteInteger(1);
            base.WriteString("Quarto de Gangue");
            base.WriteBoolean(false);
            #endregion

            base.WriteInteger(5);
            base.WriteInteger(5);
            base.WriteInteger(11);
            base.WriteInteger(4);

            base.WriteInteger(6);
            base.WriteInteger(11);
            base.WriteInteger(4);

            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(0);

            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(0);

            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(0);
        }
    }
}
