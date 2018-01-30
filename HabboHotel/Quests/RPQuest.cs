namespace Plus.HabboHotel.Quests
{
    public class RPQuest
    {
        public int UserId;
        public int QuestLine;
        public int QuestLine2;
        public int QuestLine3;
        public int QuestLine4;
        public int QuestLine5;

        public RPQuest(int UserId, int QuestLine, int QuestLine2, int QuestLine3, int QuestLine4, int QuestLine5)
        {
            this.UserId = UserId;
            this.QuestLine = QuestLine;
            this.QuestLine2 = QuestLine2;
            this.QuestLine3 = QuestLine3;
            this.QuestLine4 = QuestLine4;
            this.QuestLine5 = QuestLine5;
        }
    }
}