using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Bounties
{
    class RemoveBountyCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_bounty_undo"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Remove o usuário da recompensa."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("rrecompensa"))
                return;

            var BountyList = BountyManager.BountyUsers;
            List<Bounty> Bounty = BountyManager.BountyUsers.Values.Where(x => x.AddedBy == Session.GetHabbo().Id).ToList();

            if (Bounty.Count <= 0)
            {
                Session.SendWhisper("Você não pode fazer isso, você não é o dono da recompensa de " + TargetClient.GetHabbo().Username + "!", 1);
                return;
            }

            if (!BountyList.ContainsKey(TargetClient.GetHabbo().Id))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " não está na lista de recompensas!", 1);
                return;
            }

            BountyManager.RemoveBounty(TargetClient.GetHabbo().Id);
            Session.Shout("*Remove " + TargetClient.GetHabbo().Username + " da lista de recompensas*", 4);
            Session.GetRoleplay().CooldownManager.CreateCooldown("rrecompensa", 1000, 5);
        }
    }
}
