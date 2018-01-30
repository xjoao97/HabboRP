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
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class StunCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_stun"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Atordoa/Pulveriza o usuário alvo para detê-lo."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            string Type = "";

            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            if (Params[0].ToLower() == "pulverizar")
                Type = "pulverizar";

            if (Params[0].ToLower() == "atordoar")
                Type = "atordoar";

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

            if (!GroupManager.HasJobCommand(Session, "stun") && !Session.GetRoleplay().PoliceTrial)
            {
                Session.SendWhisper("Somente um policial pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking && !Session.GetRoleplay().PoliceTrial)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar este comando!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("atirar"))
                return;

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode atordoar alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed && !TargetClient.GetRoleplay().Jailbroken)
            {
                Session.SendWhisper("Você não pode atordoar alguém que está preso!", 1);
                return;
            }

            if (TargetClient.GetRoomUser().Frozen)
            {
                Session.SendWhisper("Este usuário já está atordoado, agora coloque as algemas!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode atordoar ou pulverizar alguém que está ausente!", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.Coordinate.X, RoomUser.Coordinate.Y);
            Point TargetClientPos = new Point(TargetUser.Coordinate.X, TargetUser.Coordinate.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            CryptoRandom Random = new CryptoRandom();
            int Chance = Random.Next(1, 101);

            if (Distance <= 5)
            {
                if (Chance <= 10)
                {
                    if (Type == "atordoar")
                        Session.Shout("*Tenta atordoar " + TargetClient.GetHabbo().Username + ", mas não consegue acertar*", 37);

                    if (Type == "pulverizar")
                        Session.Shout("*Tenta atordoar " + TargetClient.GetHabbo().Username + ", mas não consegue acertar*", 37);

                    Session.GetRoleplay().CooldownManager.CreateCooldown("atordoar", 1000, 3);
                    return;
                }
                else
                {
                    if (Distance > 6 && Type == "pulverizar")
                    {
                        Session.Shout("*Solta seu spray de pimenta em " + TargetClient.GetHabbo().Username + ", mas não consegue acertar*", 37);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("atordoar", 1000, 3);
                        return;
                    }
                    else
                    {
                        if (Type == "atordoar")
                        {
                            Session.Shout("*Atordoa o vagabundo " + TargetClient.GetHabbo().Username + " com sua arma de choques*", 37);
                            TargetClient.GetRoleplay().TimerManager.CreateTimer("atordoar", 1000, false);
                            TargetClient.SendMessage(new FloodControlComposer(5));
                            TargetClient.GetRoomUser().ApplyEffect(53);

                        }
                        else if (Type == "spray")
                        {
                            Session.Shout("*Pulveriza o vagabundo " + TargetClient.GetHabbo().Username + "*", 37);
                            TargetClient.GetRoleplay().TimerManager.CreateTimer("pulverizar", 1000, false);
                        }

                        if (TargetClient.GetRoleplay().InsideTaxi)
                            TargetClient.GetRoleplay().InsideTaxi = false;

                        TargetClient.GetRoomUser().Frozen = true;
                        TargetClient.GetRoomUser().CanWalk = false;
                        TargetClient.GetRoomUser().ClearMovement(true);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("atordoar", 1000, 3);
                        return;
                    }
                }
            }
            else
            {
                if (Type == "atordoar")
                    Session.Shout("*Atira sua arma de choque em " + TargetClient.GetHabbo().Username + ", mas os tiros não alcançam*", 37);

                if (Type == "spray")
                    Session.Shout("*Atira seu spray de pimenta em " + TargetClient.GetHabbo().Username + ", mas não consegue acertar*", 37);

                Session.GetRoleplay().CooldownManager.CreateCooldown("atordoar", 1000, 3);
                return;
            }
            #endregion
        }
    }
}