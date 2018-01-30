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
    class CuffCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_cuff"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Algema o usuário alvo para prendê-los."; }
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

            if (!GroupManager.HasJobCommand(Session, "cuff") && !Session.GetRoleplay().PoliceTrial)
            {
                Session.SendWhisper("Apenas um policial pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking && !Session.GetRoleplay().PoliceTrial)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode algemar alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed && !TargetClient.GetRoleplay().Jailbroken)
            {
                Session.SendWhisper("Você não pode algemar alguém que está preso!", 1);
                return;
            }

            if (!TargetClient.GetRoomUser().Frozen)
            {
                Session.SendWhisper("Você não pode algemar alguém que não está atordoado!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Cuffed)
            {
                Session.SendWhisper("Você não pode algemar alguém que já está algemado!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode algemar alguém que está ausente!", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                if (TargetClient.GetRoleplay().EquippedWeapon != null)
                {
                    Session.Shout("*Segura " + TargetClient.GetHabbo().Username + ", pega sua " + TargetClient.GetRoleplay().EquippedWeapon.PublicName + " e joga longe*", 37);

                    if (RoleplayManager.ConfiscateWeapons)
                    {
                        using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            DB.SetQuery("UPDATE `rp_weapons_owned` SET `can_use` = '1' WHERE `user_id` = @userid AND `base_weapon` = @baseweapon LIMIT 1");
                            DB.AddParameter("userid", TargetClient.GetHabbo().Id);
                            DB.AddParameter("baseweapon", TargetClient.GetRoleplay().EquippedWeapon.Name.ToLower());
                            DB.RunQuery();
                        }

                        TargetClient.GetRoleplay().EquippedWeapon = null;
                        TargetClient.GetRoleplay().OwnedWeapons = null;
                        TargetClient.GetRoleplay().OwnedWeapons = TargetClient.GetRoleplay().LoadAndReturnWeapons();
                    }
                    else
                        TargetClient.GetRoleplay().EquippedWeapon = null;
                }
                Session.Shout("*Puxa suas algemas e coloca rapidamente no pulso do vagabundo " + TargetClient.GetHabbo().Username + "'", 37);
                TargetClient.GetRoleplay().Cuffed = true;
                TargetClient.GetRoleplay().CuffedTimeLeft = 8;
                TargetClient.GetRoleplay().TimerManager.CreateTimer("algemar", 1000, false);
                if (TargetClient.GetRoomUser() != null)
                    TargetClient.GetRoomUser().ApplyEffect(590);
                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para algema-lo!", 1);
                return;
            }
            #endregion
        }
    }
}