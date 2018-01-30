using System.Linq;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorNuking : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {

        }

        public void OnRemove(GameClient Session, Item Item)
        {

        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Item.GetBaseItem().ItemName.ToLower() == "ads_igorswitch")
                HandleNuke(Session, Item, Request, HasRights);

            if (Item.GetBaseItem().ItemName.ToLower() == "wf_floor_switch2")
                HandleBreakDown(Session, Item, Request, HasRights);
        }

        public void HandleNuke(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null || Session.GetHabbo() == null || Item == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = null;
            User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                if (Item.ExtraData == "")
                    Item.ExtraData = "0";

                if (Item.ExtraData == "0")
                {
                    int Minutes = RoleplayManager.NPACoolDown;

                    User.ClearMovement(true);
                    User.SetRot(Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                    // 135 Cycles approximately 1 minute
                    Item.ExtraData = "1";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(135 * Minutes, true);

                    // Start the nuking process.
                    object[] Params = { Session };
                    RoleplayManager.TimerManager.CreateTimer("nuking", 1000, false, Params);

                    Session.Shout("*Começa a invadir a máquina nuclear, comandando-a para explodir a cidade*", 4);

                    #region Notify all on-duty NPA associates

                    lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            if (client == null || client.GetHabbo() == null || client.GetRoleplay() == null)
                                continue;

                            if (!Groups.GroupManager.HasJobCommand(client, "npa") && !client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                continue;

                            if (!client.GetRoleplay().IsWorking)
                                continue;

                            if (client.GetRoleplay().DisableRadio)
                                continue;

                            client.SendWhisper("[Alerta RÁDIO] [BOMBA NUCLEAR] Atenção! Alguém entrou na máquina nuclear, e ordenou que explodisse a cidade! Descubra quem é e interrompe-os rapidamente!", 30);
                        }
                    }

                    #endregion
                }
                else
                    Session.SendWhisper("Opa, parece que alguém recentemente usou. Por favor, tente novamente mais tarde!", 1);
            }
            else
            {
                if (User.CanWalk)
                    User.MoveTo(Item.SquareInFront);
            }
        }

        public void HandleBreakDown(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null || Session.GetHabbo() == null || Item == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = null;
            User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                if (Item.ExtraData == "")
                    Item.ExtraData = "0";

                if (Item.ExtraData == "0")
                {
                    int Minutes = RoleplayManager.NPACoolDown;

                    User.ClearMovement(true);
                    User.SetRot(Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                    // 135 Cycles approximately 1 minute
                    Item.ExtraData = "1";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(135 * Minutes, true);

                    // Start the nuking breakdown process.
                    object[] Params = { Session };
                    RoleplayManager.TimerManager.CreateTimer("nuking_bd", 1000, false, Params);

                    Session.Shout("*Começa a quebrar as portas do sistema NPA*", 4);
                }
                else
                    Session.SendWhisper("Opa, parece que o interruptor foi usado recentemente. Por favor, tente novamente mais tarde!", 1);
            }
            else
            {
                if (User.CanWalk)
                    User.MoveTo(Item.SquareInFront);
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}