using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Toggles
{
    class ToggleTextsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_toggle_texts"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite escolher a opção para ativar ou desativar mensagens de texto."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().AllowConsoleMessages = !Session.GetHabbo().AllowConsoleMessages;
            Session.SendWhisper("Você agora " + (Session.GetHabbo().AllowConsoleMessages ? "está" : "não está mais") + " aceitando mensagens do console.", 1);
            Session.Shout("*Pega o seu telefone e " + (Session.GetHabbo().AllowConsoleMessages ? "liga" : "desliga") + ", e " + (Session.GetHabbo().AllowConsoleMessages ? "ativa" : "desativa") + " seus serviços de mensagens de texto*", 4);
        }
    }
}