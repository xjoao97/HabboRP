using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Misc;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Apartment
{
    class RideCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_events_leave_game"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Monte um usuário"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Sintaxe de comando inválida! Use :montar <usuário>", 1);
                return;
            }

            string Username = Convert.ToString(Params[1]);

            RoomUser UserHorsie = Room.GetRoomUserManager().GetRoomUserByHabbo(Username);

            #region Conditions
            if (UserHorsie == null)
            {
                Session.SendWhisper("Usuário não encontrado ou não é um cavalo!", 1);
                return;
            }

            if (UserHorsie.GetClient() == null)
            {
                Session.SendWhisper("Usuário não encontrado ou não é um cavalo!", 1);
                return;
            }
         
            if (UserHorsie.GetClient().GetHabbo() == null)
            {
                Session.SendWhisper("Usuário não encontrado ou não é um cavalo!", 1);
                return;
            }  
            
            if (UserHorsie.GetClient().GetHabbo().PetId <= 0)
            {
                Session.SendWhisper("Usuário não é um cavalo!", 1);
                return;
            }
            #endregion


            if (UserHorsie.RidingHorse)
            {
                Session.SendWhisper("Este usuário já está sendo montado!", 1);
                return;
            }

            if (UserHorsie.HorseID == Session.GetRoomUser().VirtualId)
            {
                // unmount
                UserHorsie.Statusses.Remove("sit");
                UserHorsie.Statusses.Remove("lay");
                UserHorsie.Statusses.Remove("snf");
                UserHorsie.Statusses.Remove("eat");
                UserHorsie.Statusses.Remove("ded");
                UserHorsie.Statusses.Remove("jmp");
                Session.GetRoomUser().RidingHorse = false;
                Session.GetRoomUser().HorseID = 0;
                UserHorsie.RidingHorse = false;
                UserHorsie.HorseID = 0;
                Session.GetRoomUser().MoveTo(new Point(Session.GetRoomUser().X + 2, Session.GetRoomUser().Y + 2));
                Session.GetRoomUser().ApplyEffect(-1);
                Session.GetRoomUser().UpdateNeeded = true;
                UserHorsie.UpdateNeeded = true;
            }
            else
            {

                int NewX2 = Session.GetRoomUser().X;
                int NewY2 = Session.GetRoomUser().Y;
                Room.SendMessage(Room.GetRoomItemHandler().UpdateUserOnRoller(UserHorsie, new Point(NewX2, NewY2), 0, Room.GetGameMap().SqAbsoluteHeight(NewX2, NewY2)));
                Room.SendMessage(Room.GetRoomItemHandler().UpdateUserOnRoller(Session.GetRoomUser(), new Point(NewX2, NewY2), 0, Room.GetGameMap().SqAbsoluteHeight(NewX2, NewY2) + 1));

                Session.GetRoomUser().MoveTo(NewX2, NewY2);

                UserHorsie.ClearMovement(true);

                Session.GetRoomUser().RidingHorse = true;
                UserHorsie.RidingHorse = true;
                UserHorsie.HorseID = Session.GetRoomUser().VirtualId;
                Session.GetRoomUser().HorseID = UserHorsie.VirtualId;

                Session.GetRoomUser().ApplyEffect(77);

                Session.GetRoomUser().RotBody = UserHorsie.RotBody;
                Session.GetRoomUser().RotHead = UserHorsie.RotHead;

                Session.GetRoomUser().UpdateNeeded = true;
                UserHorsie.UpdateNeeded = true;


                Session.Shout("*Pula em " + UserHorsie.GetClient().GetHabbo().Username + ", e começa a cavalgar como sua vadia*");
            }
        }
    }
}
