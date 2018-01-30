namespace Plus.HabboHotel.Catalog.PredesignedRooms
{
    internal class PredesignedWallItems
    {
        internal uint BaseItem;
        internal string WallCoord;
        internal string ExtraData;

        internal PredesignedWallItems(uint baseItem, string wallCoord, string extraData)
        {
            BaseItem = baseItem;
            WallCoord = wallCoord;
            ExtraData = extraData;
        }
    }
}