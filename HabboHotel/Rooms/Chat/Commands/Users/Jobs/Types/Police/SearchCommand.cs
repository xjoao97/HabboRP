using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class SearchCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_search"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Vasculha o usuário alvo para ver se ele têm alguma droga"; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
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

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "search"))
            {
                Session.SendWhisper("Apenas um policial pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode vasculhar alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode vasculhar alguém que está preso!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode vasculhar alguém que está ausente", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                Random Random = new Random();

                int Chance = Random.Next(1, 101);

                if (Chance <= 8)
                {
                    Session.Shout("*Vasculha " + TargetClient.GetHabbo().Username + " tentando encontrar drogas*", 37);
                    return;
                }
                else
                {
                    bool HasWeed = TargetClient.GetRoleplay().Weed > 0;
                    bool HasCocaine = TargetClient.GetRoleplay().Cocaine > 0;

                    if (!HasWeed && !HasCocaine)
                    {
                        Session.Shout("*Vasculha " + TargetClient.GetHabbo().Username + " mas não acha nenhuma droga*", 37);
                        return;
                    }
                    else if (HasWeed && !HasCocaine)
                    {
                        Session.Shout("*Vasculha " + TargetClient.GetHabbo().Username + " e acha " + String.Format("{0:N0}", TargetClient.GetRoleplay().Weed) + "g de maconha*", 37);
                        return;
                    }
                    else if (HasCocaine && !HasWeed)
                    {
                        Session.Shout("*Vasculha " + TargetClient.GetHabbo().Username + " e acha " + String.Format("{0:N0}", TargetClient.GetRoleplay().Cocaine) + "g de cocaína*", 37);
                        return;
                    }
                    else
                    {
                        Session.Shout("*Vasculha " + TargetClient.GetHabbo().Username + " e acha " + String.Format("{0:N0}", TargetClient.GetRoleplay().Cocaine) + "g de cocaína e " + String.Format("{0:N0}", TargetClient.GetRoleplay().Weed) + "g de maconha*", 37);
                        return;
                    }
                }
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para vasculhar!", 1);
                return;
            }
            #endregion
        }
    }
}