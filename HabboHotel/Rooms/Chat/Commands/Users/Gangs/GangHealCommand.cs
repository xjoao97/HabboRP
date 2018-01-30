using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.Utilities;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangHealCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_heal"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Usa uma das suas mediphas de gangues para curar um membro da gangue."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 2)
            {
                Session.SendWhisper("Digite o nome de usuário do membro da gangue que deseja curar!", 1);
                return;
            }

            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);
            GroupRank GangRank = GroupManager.GetGangRank(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank);

            if (Gang == null)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            if (Gang.Id <= 1000)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            if (!GroupManager.HasGangCommand(Session, "gheal"))
            {
                Session.SendWhisper("Você não possui um cargo alto o suficiente para usar esse comando!", 1);
                return;
            }

            if (Gang.MediPacks <= 0)
            {
                Session.SendWhisper("Sua gangue não tem mais pacotes de cura!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("Desculpe, mas este usuário não pôde ser encontrado!", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetRoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetRoomUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().CurHealth == TargetClient.GetRoleplay().MaxHealth)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " não precisa ser curado!", 1);
                return;
            }

            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetRoomUser.X, TargetRoomUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance > 1)
            {
                Session.SendWhisper("Aproxime-se de " + TargetClient.GetHabbo().Username + " para curá-lo com um pacote médico!", 1);
                return;
            }

            Session.Shout("*Puxa um pacote médico e aplica alguns band-aid nas feridas de " + TargetClient.GetHabbo().Username + "'*", 4);
            Gang.MediPacks -= 1;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                dbClient.RunQuery("UPDATE `rp_gangs` SET `medipacks` = '" + Gang.MediPacks + "' WHERE `id` = '" + Gang.Id + "'");

            CryptoRandom Random = new CryptoRandom();
            int HealAmount = Random.Next(5, 16);

            new Thread(() =>
            {
                Thread.Sleep(3000);

                if (!TargetClient.GetRoleplay().IsDead)
                {
                    if (TargetRoomUser != null)
                        TargetRoomUser.ApplyEffect(23);

                    int NewHealth = TargetClient.GetRoleplay().CurHealth + HealAmount;

                    if (NewHealth > TargetClient.GetRoleplay().MaxHealth)
                        TargetClient.GetRoleplay().CurHealth = TargetClient.GetRoleplay().MaxHealth;
                    else
                        TargetClient.GetRoleplay().CurHealth = NewHealth;

                    TargetClient.SendWhisper(Session.GetHabbo().Username + " Os kits médicos começou a produzir efeito!", 1);
                }
            }).Start();
        }
    }
}