using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Houses;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Moodlight;

namespace Plus.HabboHotel.Rooms.Chat.Commands.VIP
{
    class OpenDimmerCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_open_dimmer"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Defina uma altura para os móveis serem empilhados."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Room.MoodlightData == null)
            {
                foreach (Item Item in Room.GetRoomItemHandler().GetWall.ToList())
                {
                    if (Item.GetBaseItem().InteractionType == InteractionType.MOODLIGHT)
                        Room.MoodlightData = new MoodlightData(Item.Id);
                }
            }

            if (Room.MoodlightData == null)
            {
                Session.SendWhisper("Opa, parece que não há reguladores de luz nesta sala!", 1);
                return;
            }

            Session.SendMessage(new MoodlightConfigComposer(Room.MoodlightData));
        }
    }
}