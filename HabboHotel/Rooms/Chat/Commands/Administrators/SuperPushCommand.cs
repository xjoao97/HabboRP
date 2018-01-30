using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class SuperPushCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_push_super"; }
        }

        public string Parameters
        {
            get { return "%alvo%"; }
        }

        public string Description
        {
            get { return "Super empurrar. (Empurra por 3 quadrados)"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome de usuário do usuário que deseja empurrar.", 1);
                return;
            }

            if (!Room.SPushEnabled)
            {
                Session.SendWhisper("Opa, parece que o proprietário do quarto desativou a capacidade de usar o comando aqui.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Você não pode empurrar-se!", 1);
                return;
            }

            if (TargetUser.TeleportEnabled)
            {
                Session.SendWhisper("Opa, você não pode empurrar um usuário que tem o modo teleporte ativado.", 1);
                return;
            }

            RoomUser ThisUser = Session.GetRoomUser();
            if (ThisUser == null)
                return;

            if (!((Math.Abs(TargetUser.X - ThisUser.X) >= 2) || (Math.Abs(TargetUser.Y - ThisUser.Y) >= 2)))
            {
                if (TargetUser.RotBody == 4)
                {
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                }

                if (ThisUser.RotBody == 0)
                {
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                }

                if (ThisUser.RotBody == 6)
                {
                    TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                }

                if (ThisUser.RotBody == 2)
                {
                    TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                }

                if (ThisUser.RotBody == 3)
                {
                    TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                }

                if (ThisUser.RotBody == 1)
                {
                    TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                }

                if (ThisUser.RotBody == 7)
                {
                    TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                }

                if (ThisUser.RotBody == 5)
                {
                    TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                }

                Session.Shout("*Dá um empurrão em " + Params[1] + "*", 23);
            }
            else
            {
                Session.SendWhisper("Opa, " + Params[1] + " não está perto!", 1);
            }
        }
    }
}
