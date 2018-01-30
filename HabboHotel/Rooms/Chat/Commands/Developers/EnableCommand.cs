using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Rooms.Games.Teams;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class EnableCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_enable"; }
        }

        public string Parameters
        {
            get { return "%effectid%"; }
        }

        public string Description
        {
            get { return "Muda seu efeito atual para o ID de efeito escolhido."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Você deve inserir um ID de efeito!", 1);
                return;
            }

            if (!Room.EnablesEnabled && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("Opa, o proprietário do quarto desativou a capacidade de usar o comando", 1);
                return;
            }

            RoomUser ThisUser = Session.GetRoomUser();
            if (ThisUser == null)
                return;

            if (ThisUser.RidingHorse)
            {
                Session.SendWhisper("Você não pode habilitar os efeitos enquanto anda de cavalo!", 1);
                return;
            }
            else if (ThisUser.Team != TEAM.NONE || Session.GetRoleplay().Game != null)
            {
                Session.SendWhisper("Você não pode usar esse comando enquanto estiver em um jogo!", 1);
                return;
            }
            else if (ThisUser.isLying)
            {
                Session.SendWhisper("Você não pode usar este comando enquanto está deitado!", 1);
                return;
            }

            int EffectId = 0;
            if (!int.TryParse(Params[1], out EffectId))
            {
                Session.SendWhisper("Insira um numero!", 1);
                return;
            }

            if (EffectId > int.MaxValue || EffectId < int.MinValue)
            {
                Session.SendWhisper("Digite um ID de efeito válido!", 1);
                return;
            }

            ThisUser.ApplyEffect(EffectId);
        }
    }
}
