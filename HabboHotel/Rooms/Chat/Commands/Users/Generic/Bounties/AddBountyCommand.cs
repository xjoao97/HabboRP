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
    class AddBountyCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_bounty_add"; }
        }

        public string Parameters
        {
            get { return "%usuário% %recompensa%"; }
        }

        public string Description
        {
            get { return "Adiciona o usuário à recompensa."; }
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

            if (Session.GetRoleplay().TryGetCooldown("addrecompensa"))
                return;

            if (Session.GetRoleplay().Level < 3)
            {
                Session.SendWhisper("Você deve ser ao menos nível 3 para definir recompensas!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Level < 3)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " O nível é muito baixo para definir uma recompensa! Ele deve ser pelo menos nível 3!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode definir uma recompensa em alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode definir uma recompensa em alguém que está preso!", 1);
                return;
            }

            if (TargetClient == Session)
            {
                Session.SendWhisper("Você não pode definir uma recompensa em si mesmo!", 1);
                return;
            }

            var BountyList = BountyManager.BountyUsers;
            int Amount;

            if (Params.Length < 3)
            {
                Session.SendWhisper("Comando inválido! Use :addrecompensa <usuário> <quantidade do pagamento>", 1);
                return;
            }

            if (int.TryParse((Params[2]), out Amount))
            {
                if (BountyList.ContainsKey(TargetClient.GetHabbo().Id))
                {
                    Session.SendWhisper(TargetClient.GetHabbo().Username + " já está na lista de recompensas!", 1);
                    return;
                }

                if (Amount <= 0)
                {
                    Session.SendWhisper("Por favor insira uma quantia válida de dinheiro!", 1);
                    return;
                }

                if (Amount < 50)
                {
                    Session.SendWhisper("O valor mínimo de recompensa é de R$50!", 1);
                    return;
                }

                if (Session.GetHabbo().Credits < Amount)
                {
                    Session.SendWhisper("Você não tem R$" + String.Format("{0:N0}", Amount) + " para pagar a recompensas!", 1);
                    return;
                }

                Bounty NewBounty = new Bounty(TargetClient.GetHabbo().Id, Session.GetHabbo().Id, Amount, PlusEnvironment.GetUnixTimestamp(), PlusEnvironment.GetUnixTimestamp() + 3600);
                BountyManager.AddBounty(NewBounty);

                Session.Shout("*Coloca uma recompensa de R$" + String.Format("{0:N0}", Amount) + " para quem matar o vagabundo " + TargetClient.GetHabbo().Username + "*", 4);

                Session.GetHabbo().Credits -= Amount;
                Session.GetHabbo().UpdateCreditsBalance();
                Session.GetRoleplay().CooldownManager.CreateCooldown("addrecompensa", 1000, 5);
            }
            else
                Session.SendWhisper("Por favor insira um número válido!", 1);
        }
    }
}
