using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class SayAllCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_say_all"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Força todos os usuários na sala a dizer a mensagem."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser ThisUser = Session.GetRoomUser();
            if (ThisUser == null)
                return;

            if (Params.Length == 1)
                Session.SendWhisper("Você deve digitar a mensagem que você deseja forçá-los a dizer.", 1);
            else
            {
                string Message = CommandManager.MergeParams(Params, 1);
                
                foreach (var User in Room.GetRoomUserManager().GetRoomUsers())
                {
                    if (User == null)
                        continue;

                    if (User.GetClient() == null)
                        continue;

                    if (User == ThisUser)
                        continue;

                    User.SendNameColourPacket();
                    Room.SendMessage(new ChatComposer(User.VirtualId, Message, 0, User.LastBubble));
                    User.SendNamePacket();
                }
            }
        }
    }
}
