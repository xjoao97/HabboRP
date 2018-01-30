using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class CorpListCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_list"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Mostra uma lista de todas empresas disponíveis."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            lock (GroupManager.Jobs)
            {
                List<Group> CorpList = GroupManager.Jobs.Values.Where(x => x.Id > 1).ToList();

                StringBuilder Message = new StringBuilder();
                Message.Append("<---------- Empresas Disponíveis ---------->\n\n");

                foreach (Group Corp in CorpList)
                {
                    Room CorpRoom = RoleplayManager.GenerateRoom(Corp.RoomId);

                    Message.Append("<----- " + Corp.Name + " [ID: " + Corp.Id + "] ----->\n");
                    Message.Append("Descrição: " + Corp.Description + "\n");

                    if (CorpRoom != null)
                        Message.Append("Quarto(s) principal: " + CorpRoom.Name + " [Quarto ID: " + CorpRoom.RoomId + "]\n");
                    else
                        Message.Append("Quarto(s) principal: Desconhecido [Quarto ID: N/A]\n");

                    if (Corp.Members.Values.Where(x => x.UserRank == 6).ToList().Count > 0)
                        Message.Append("Gerenciado por: " + ((PlusEnvironment.GetHabboById(Corp.Members.Values.FirstOrDefault(x => x.UserRank == 6).UserId) != null) ? PlusEnvironment.GetHabboById(Corp.Members.Values.FirstOrDefault(x => x.UserRank == 6).UserId).Username : "HabboRPG") + "\n\n");
                    else
                        Message.Append("Gerenciado por: HabboRPG\n\n");
                }

                Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
            }
        }
    }
}