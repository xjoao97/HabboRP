using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Timers;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class SendhomeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_send_home"; }
        }

        public string Parameters
        {
            get { return "%usuário% %minutos%"; }
        }

        public string Description
        {
            get { return "Envia um dos seus empregados para casa."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int SendHomeTime;
            int Bubble = 4;
            #endregion

            #region Conditions
            if (Params.Length != 3)
            {
                Session.SendWhisper("Digite o nome de usuário e o tempo de envio!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar este usuário, talvez ele esteja offline!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("sendhome"))
                return;

            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (!GroupManager.HasJobCommand(Session, "sendhome"))
                {
                    Session.SendWhisper("Você não tem um cargo tão alto na empresa para usar este comando!", 1);
                    return;
                }

                if (TargetClient == Session)
                {
                    Session.SendWhisper("Você não pode se enviar para casa!");
                    return;
                }

                if (Session.GetRoleplay().JobId != TargetClient.GetRoleplay().JobId)
                {
                    Session.SendWhisper("Este cidadão não trabalha para você!", 1);
                    return;
                }

                if (TargetClient.GetRoleplay().JobRank >= Session.GetRoleplay().JobRank)
                {
                    Session.SendWhisper("Você não pode enviar esse cidadão para casa!", 1);
                    return;
                }
                Bubble = 4;
            }
            if (Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                Bubble = 23;
            #endregion

            if (int.TryParse(Params[2], out SendHomeTime))
            {
                if (SendHomeTime > 30)
                {
                    Session.SendWhisper("Você não pode enviar o seu trabalhador por mais de 30 minutos!", 1);
                    return;
                }

                if (TargetClient.GetRoleplay().IsWorking)
                {
                    WorkManager.RemoveWorkerFromList(TargetClient);
                    TargetClient.GetRoleplay().IsWorking = false;
                    TargetClient.GetHabbo().Poof();
                }

                TargetClient.GetRoleplay().SendHomeTimeLeft = SendHomeTime;
                TargetClient.SendWhisper("Você foi enviado para casa por " + SendHomeTime + " minutos! Você não pode trabalhar até que seu tempo enviado tenha sido concluído!", 1);

                if (!TargetClient.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("sendhome"))
                    TargetClient.GetRoleplay().TimerManager.CreateTimer("sendhome", 1000, false);
                
                if (Bubble == 4)
                    Session.Shout("*Manda " + TargetClient.GetHabbo().Username + " para casa por " + SendHomeTime + " minutos*", Bubble);
                else
                    Session.Shout("*Manda imediatamente " + TargetClient.GetHabbo().Username + " para casa por " + SendHomeTime + " minutos*", Bubble);

                Session.GetRoleplay().CooldownManager.CreateCooldown("sendhome", 1000, 5);
            }
            else
            {
                Session.SendWhisper("Insira um número para o tempo desejado!", 1);
                return;
            }
        }
    }
}