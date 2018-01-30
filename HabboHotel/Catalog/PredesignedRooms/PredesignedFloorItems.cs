namespace Plus.HabboHotel.Catalog.PredesignedRooms
{
    internal class PredesignedFloorItems
    {
        internal uint BaseItem;
        internal int X, Y, Rot;
        internal double Z;
        internal string ExtraData;

        internal PredesignedFloorItems(uint baseItem, int x, int y, int rot, double z, string extraData)
        {
            BaseItem = baseItem;
            X = x;
            Y = y;
            Rot = rot;
            Z = z;
            ExtraData = extraData;
        }
    }
}