namespace Plus.HabboHotel.Groups
{
    public class GroupBases
    {
        public int Id;
        public string Value1;
        public string Value2;

        public GroupBases(int Id, string Value1, string Value2)
        {
            this.Id = Id;
            this.Value1 = Value1;
            this.Value2 = Value2;
        }
    }

    public class GroupSymbols
    {
        public int Id;
        public string Value1;
        public string Value2;

        public GroupSymbols(int Id, string Value1, string Value2)
        {
            this.Id = Id;
            this.Value1 = Value1;
            this.Value2 = Value2;
        }
    }

    public class GroupBaseColours
    {
        public string Colour;
        public int Id;

        public GroupBaseColours(int Id, string Colour)
        {
            this.Id = Id;
            this.Colour = Colour;
        }
    }

    public class GroupSymbolColours
    {
        public string Colour;
        public int Id;

        public GroupSymbolColours(int Id, string Colour)
        {
            this.Id = Id;
            this.Colour = Colour;
        }
    }

    public class GroupBackGroundColours
    {
        public string Colour;
        public int Id;

        public GroupBackGroundColours(int Id, string Colour)
        {
            this.Id = Id;
            this.Colour = Colour;
        }
    }
}