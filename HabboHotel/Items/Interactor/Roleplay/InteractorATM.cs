using System.Drawing;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Polls;
using Plus.HabboHotel.Pathfinding;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorATM : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {  
            if (Session.GetRoomUser() == null)
                return;

            var User = Session.GetRoomUser();
          
            if (Session.GetRoleplay().WebSocketConnection != null)
            {
                if (Rooms.Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                {
                    Session.GetRoleplay().UsingAtm = true;
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_atm", "open");
                }
                else
                    User.MoveTo(Item.SquareInFront);
            }
            else
            {
                #region Poll Code
                Session.SendWhisper("o nosso servidor geral de websocket está offline, você pode usar nossa caixa eletrônica de backup!", 1);
                if (Rooms.Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                {
                    string PollName = "HabboRPG - Caixa";
                    string PollInvitation = "HabboRPG - Caixa";
                    string PollThanks = "Obrigado por usar o caixa eletrônico do HabboRPG!";

                    Polls.Poll ATMPoll = new Polls.Poll(500000, 0, PollName, PollInvitation, PollThanks, "", 1, null);
                    Session.SendMessage(new SuggestPollMessageComposer(ATMPoll));
                }
                else
                    User.MoveTo(Item.SquareInFront);
                #endregion
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}