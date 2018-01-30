using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class SummonPetsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_summon_pets"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Convoca todos os usuários transformados."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            #region Variables
            int Count = 0;
            #endregion

            #region Execute
            lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (Client == null || Client.GetHabbo() == null || Client.GetRoleplay() == null)
                        continue;

                    if (Client == Session)
                        continue;

                    if (Client.GetHabbo().PetId <= 0)
                        continue;

                    if (Client.GetRoleplay().IsDead)
                    {
                        Client.GetRoleplay().IsDead = false;
                        Client.GetRoleplay().ReplenishStats(true);
                        Client.GetHabbo().Poof();
                    }

                    if (Client.GetRoleplay().IsJailed)
                    {
                        Client.GetRoleplay().IsJailed = false;
                        Client.GetRoleplay().JailedTimeLeft = 0;
                        Client.GetHabbo().Poof();
                    }

                    Count++;
                    RoleplayManager.SendUser(Client, Room.Id, "Você foi convocado por " + Session.GetHabbo().Username + "!");
                }
                if (Count > 0)
                    Session.Shout("*Puxa todos os usuários transformados para minha sala*", 23);
                else
                    Session.SendWhisper("Desculpe, mas não houve usuários transformados para invocar!", 1);
            }
            #endregion
        }
    }
}