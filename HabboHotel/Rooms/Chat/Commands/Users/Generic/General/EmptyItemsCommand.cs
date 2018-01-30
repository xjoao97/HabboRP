using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class EmptyItemsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_empty_items"; }
        }

        public string Parameters
        {
            get { return "%sim%"; }
        }

        public string Description
        {
            get { return "Seu inventário está cheio? Você pode remover todos os itens digitando esse comando."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendNotification("Você tem certeza que deseja Limpar seu inventário? Você perderá todos os mobis!\n" +
                 "Para confirmar, digite \":limparinventario sim\". \n\nDepois de fazer isso, não há retorno!\n(Se você não quiser esvaziá-lo, ignore esta mensagem!)\n\n" +
                 "OBSERVE! Se você tiver mais de 3000 itens, os itens ocultos também serão DELETADOS.");
                return;
            }
            else
            {
                if (Params.Length == 2 && Params[1].ToString() == "sim")
                {
                    Session.GetHabbo().GetInventoryComponent().ClearItems();
                    Session.SendWhisper("Seu inventário foi limpo!", 1);   
                    return;
                }
                else if (Params.Length == 2 && Params[1].ToString() != "sim")
                {
                    Session.SendWhisper("Para confirmar, você deve digitar :limparinventario sim", 1);
                    return;
                }
            }
        }
    }
}
