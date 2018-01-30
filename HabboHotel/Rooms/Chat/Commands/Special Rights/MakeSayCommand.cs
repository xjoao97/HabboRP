using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class MakeSayCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_make_say"; }
        }

        public string Parameters
        {
            get { return "%usuário% %mensagem%"; }
        }

        public string Description
        {
            get { return "Força o usuário especificado a dizer a mensagem."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Variables
            if (Params.Length == 1)
            {
                Session.SendWhisper("Você deve digitar um nome de usuário e mensagem que você deseja forçá-lo a dizer.", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 2);
            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            #endregion

            #region Conditions
            if (TargetSession == null)
            {
                Session.SendWhisper("Este usuário não pôde ser encontrado!", 1);
                return;
            }

            if (TargetSession.GetRoomUser() == null)
            {
                Session.SendWhisper("Este usuário não pôde ser encontrado!", 1);
                return;
            }

            if (TargetSession.GetHabbo().GetPermissions().HasRight("mod_make_say_any"))
            {
                Session.SendWhisper("Você não pode usar esse comando neste usuário.", 1);
                return;
            }
            #endregion

            #region Execute

            if (Session.GetHabbo().Id == 1 || Session.GetHabbo().Id == 46)
            {
                if (Message.StartsWith(":", StringComparison.CurrentCulture))
                {
                    if (PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(TargetSession, Message))
                        Session.SendWhisper("Comando executado: " + Message, 1);
                    else
                        Session.SendWhisper("Comando inválido: " + Message + "!", 1);
                    return;
                }
            }

            TargetSession.GetRoomUser().SendNameColourPacket();
            Room.SendMessage(new ChatComposer(TargetSession.GetRoomUser().VirtualId, Message, 0, TargetSession.GetRoomUser().LastBubble));
            TargetSession.GetRoomUser().SendNamePacket();

            #endregion

        }
    }
}
