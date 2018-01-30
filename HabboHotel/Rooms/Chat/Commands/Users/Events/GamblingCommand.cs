using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboRoleplay.Gambling;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Events
{
    class GamblingCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_events_gambling"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite que você use comandos de jogo!"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            var Game = TexasHoldEmManager.GetGameForUser(Session.GetHabbo().Id);

            if (Game == null || Session.GetRoleplay().TexasHoldEmPlayer <= 0)
            {
                Session.SendWhisper("Você deve estar em um Texas Hold para usar esse comando!", 1);
                return;
            }

            if (!Game.GameStarted)
            {
                Session.SendWhisper("O Texas Holdainda não começou!", 1);
                return;
            }

            if (Params[0].ToLower() == "apostar" || Params[0].ToLower() == "passar")
            {
                ExecuteBet(Session, Room, Params, Game);
                return;
            }
        }

        public void ExecuteBet(GameClients.GameClient Session, Rooms.Room Room, string[] Params, TexasHoldEm Game)
        {
            if (Game.GameSequence <= 0 || Game.GameSequence > 2)
            {
                Session.SendWhisper("Você não pode fazer uma aposta (ou passar) agora!", 1);
                return;
            }

            int Number = Session.GetRoleplay().TexasHoldEmPlayer;

            if (Game.PlayersTurn != Number)
            {
                Session.SendWhisper("Não é a sua vez no jogo!", 1);
                return;
            }

            if (!Game.PlayerList.ContainsKey(Number))
                return;

            var Player = Game.PlayerList[Number];

            if (Player == null || Player.UserId != Session.GetHabbo().Id)
                return;

            if (Player.TotalAmount <= 0)
            {
                // Already has all their chips in (or has the maximum bet rn)
                Game.ChangeTurn();
                return;
            }

            bool Zero = false;
            if (Params.Length > 1 && Params[1].ToLower() == "0")
                Zero = true;

            if (Params[0].ToLower() == "passar" && Game.MinimumBet(Number) > 0)
            {
                Session.SendWhisper("Você não pode passar neste turno! Você deve :apostar ou :sairjogo!", 1);
                return;
            }

            if (Game.MinimumBet(Number) == 0 && (Params.Length == 1 || Params[0].ToLower() == "passar" || Zero))
            {
                // Doesnt need to make a bet, can pass
                Game.ChangeTurn();
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o valor que você gostaria de apostar!", 1);
                return;
            }

            int Amount;
            if (!int.TryParse(Params[1], out Amount))
            {
                Session.SendWhisper("Por favor insira um número válido como o valor que você gostaria de apostar!", 1);
                return;
            }

            // Check if its a multiple of 5
            if (Convert.ToDouble((double)Amount / 5) != Math.Floor(Convert.ToDouble((double)Amount / 5)))
            {
                Session.SendWhisper("Você só pode apostar dinheiro em múltiplos de 5!", 1);
                return;
            }

            if (Player.TotalAmount < Amount)
                Amount = Player.TotalAmount;

            if (Amount < Game.MinimumBet(Number) && Amount != Player.TotalAmount)
            {
                Session.SendWhisper("Você deve apostar pelo menos R$" + String.Format("{0:N0}", Game.MinimumBet(Number)) + " para combinar o pote atual!", 1);
                return;
            }

            if (Session.GetHabbo().Credits < Amount)
            {
                Session.SendWhisper("Você não tem tanto dinheiro para apostar!", 1);
                return;
            }

            Session.Shout("*Faz uma aposta de R$" + String.Format("{0:N0}", Amount) + " para os jogos Pot*", 4);

            Session.GetHabbo().Credits -= Amount;
            Session.GetHabbo().UpdateCreditsBalance();

            Game.PlacePotFurni(Number, Amount);
            Game.SpawnStartingBet(Number);
            Game.ChangeTurn();
            return;
        }
    }
}