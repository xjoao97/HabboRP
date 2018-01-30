using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Games;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Notifications
{
    class RoomNotificationComposer : ServerPacket
    {
        public RoomNotificationComposer(string Type, string Key, string Value)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            base.WriteString(Type);
            base.WriteInteger(1);//Count
            {
                base.WriteString(Key);//Type of message
                base.WriteString(Value);
            }
        }

        public RoomNotificationComposer(string Type)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            base.WriteString(Type);
            base.WriteInteger(0);//Count
        }

        public RoomNotificationComposer(string Title, string Message, string Image, string HotelName = "", string HotelURL = "")
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            base.WriteString(Image);
            base.WriteInteger(string.IsNullOrEmpty(HotelName) ? 2 : 4);
            base.WriteString("title");
            base.WriteString(Title);
            base.WriteString("message");
            base.WriteString(Message);

            if (!string.IsNullOrEmpty(HotelName))
            {
                base.WriteString("linkUrl");
                base.WriteString(HotelURL);
                base.WriteString("linkTitle");
                base.WriteString(HotelName);
            }
        }

        public RoomNotificationComposer(GameClient Session, string Type, string message, int amount = 0, GameMode Mode = GameMode.None, Room Room = null)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            if (Mode != GameMode.None)
            {
                base.WriteString("avatarimage_HoloRP");
                base.WriteInteger(4);
                base.WriteString("title");
                base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("alert_event_title"));
                base.WriteString("message");
                base.WriteString("<b>HabboRPG (Eventos Automáticos)</b> Irá começar um evento agora, e vale pontos de eventos!\n\n" +
                        "Detalhes adicionais: Esses são eventos automáticos do sistema.\n\n<i>Os eventos automáticos não são supervisionados por um membro da Equipe.</i>");
                base.WriteString("linkUrl");
                base.WriteString("event:navigator/goto/" + Room.RoomId);
                base.WriteString("linkTitle");
                base.WriteString("Ir para '" + Room.Name + " (" + Room.Id + ")'!");
            }
            else
            {
                if (Type == "death")
                {
                    base.WriteString("room_death_axe"); // Image
                    base.WriteInteger(4);
                    base.WriteString("title");
                    base.WriteString("Você morreu!"); // Title
                    base.WriteString("message");
                    base.WriteString("Parece que você acabou de morrer!\n\nVocê está sendo transportado para o hospital no momento."); // Message
                    base.WriteString("linkUrl");
                    base.WriteString("event:"); // Should clicking the button do something?
                    base.WriteString("linkTitle");
                    base.WriteString("Click Here To Close"); // Button Message
                }
                else if (Type == "jail")
                {
                    base.WriteString("room_jail_prison"); // Image
                    base.WriteInteger(4);
                    base.WriteString("title");
                    base.WriteString("Você foi preso!"); // Title
                    base.WriteString("message");
                    base.WriteString("Você foi preso por " + Session.GetHabbo().Username + " por " + amount + " minutos!\n\nVocê está sendo transportado para a cadeia."); // Message
                    base.WriteString("linkUrl");
                    base.WriteString("event:"); // Should clicking the button do something?
                    base.WriteString("linkTitle");
                    base.WriteString("Clique aqui para fechar"); // Button Message
                }
                else if (Type == "brawl")
                {
                    base.WriteString("room_kick_cannonball"); // Image
                    base.WriteInteger(4);
                    base.WriteString("title");
                    base.WriteString("Você foi nocauteado!"); // Title
                    base.WriteString("message");
                    base.WriteString("Você foi nocauteado da briga!\n\n Mais sorte da próxima vez!"); // Message
                    base.WriteString("linkUrl");
                    base.WriteString("event:"); // Should clicking the button do something?
                    base.WriteString("linkTitle");
                    base.WriteString("Clique aqui para fechar"); // Button Message
                }
                else
                {
                    base.WriteString("avatarimage_" + Session.GetHabbo().Username); // Image
                    base.WriteInteger(4);
                    base.WriteString("title");
                    base.WriteString(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("alert_event_title"));
                    base.WriteString("message");
                    base.WriteString("<b>" + Session.GetHabbo().Username + "</b> está fazendo um evento. Prêmios e Pontos de Evento serão dados aos vencedores!\n\n" +
                        "Detalhes adicionais: " + message + "\n\n<i>Este evento está sendo supervisionado por um membro da Equipe</i>");
                    base.WriteString("linkUrl");
                    base.WriteString("event:navigator/goto/" + Session.GetHabbo().CurrentRoomId);
                    base.WriteString("linkTitle");
                    base.WriteString("Ir para '" + Session.GetHabbo().CurrentRoom.Name + " (" + Session.GetHabbo().CurrentRoom.Id + ")'!");
                }
            }
        }
    }
}
