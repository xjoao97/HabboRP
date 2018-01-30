using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Rooms.Chat.Commands.VIP
{
    class FlagMeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_flag_me"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Dá-lhe a opção de alterar o seu nome."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!this.CanChangeName(Session.GetHabbo()))
            {
                Session.SendWhisper("Desculpe, parece que você atualmente não tem a opção de alterar seu nome de usuário!", 1);
                return;
            }

            Session.GetHabbo().ChangingName = true;
            Session.SendNotification("Atenção: se o seu nome de usuário for considerado inapropriado, você será banido sem dúvida.\r\rObserve também que os Staffs NÃO mudará seu nome de usuário novamente se você tiver um problema com o que você escolheu.\r\rFeche esta janela e clique em si mesmo para começar a escolher um novo nome de usuário!");
            Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
        }

        private bool CanChangeName(Habbo Habbo)
        {
            if (Habbo.Rank == 1 && Habbo.VIPRank == 1 && (Habbo.LastNameChange == 0 || (PlusEnvironment.GetUnixTimestamp() + 604800) > Habbo.LastNameChange))
                return true;
            else if (Habbo.GetPermissions().HasRight("mod_tool"))
                return true;

            return false;
        }
    }
}
