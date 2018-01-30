using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class SetSpeedCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_set_speed"; }
        }

        public string Parameters
        {
            get { return "%value%"; }
        }

        public string Description
        {
            get { return "Defina a velocidade dos rollers na sala atual."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Insira um valor para a velocidade do roller", 1);
                return;
            }

            int Speed;
            if (int.TryParse(Params[1], out Speed))
            {
                Session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(Speed);
                Session.Shout("Atualiza a velocidade dos rollers na sala*", 23);
            }
            else
                Session.SendWhisper("Valor inválido, por favor insira um número válido.", 1);
        }
    }
}