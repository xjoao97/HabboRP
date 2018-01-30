using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Rooms;


namespace Plus.HabboHotel.Moderation
{
    public class ModerationTicket
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public int Category { get; set; }
        public double Timestamp { get; set; }
        public int Priority { get; set; }
        public bool Answered { get; set; }
        public Habbo Sender { get; set; }
        public Habbo Reported { get; set; }
        public Habbo Moderator { get; set; }
        public string Issue { get; set; }
        public RoomData Room { get; set; }

        public ModerationTicket(int Id, int Type, int Category, double Timestamp, int Priority, Habbo Sender, Habbo Reported, string Issue, RoomData Room)
        {
            this.Id = Id;
            this.Type = Type;
            this.Category = Category;
            this.Timestamp = Timestamp;
            this.Priority = Priority;
            this.Sender = Sender;
            this.Reported = Reported;
            this.Moderator = null;
            this.Issue = Issue;
            this.Room = Room;
            this.Answered = false;
        }

        public int GetStatus(int Id)
        {
            if (Moderator == null)
                return 1;         
            else if (Moderator.Id == Id && !Answered)
                return 2;
            else if (Answered)
                return 3;
            else
                return 3;           
        }
    }
}
