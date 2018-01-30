using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangInviteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_invite"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Convida o usuário alvo para sua gangue."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);
            #endregion

            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que você gostaria de convidar para sua gangue!", 1);
                return;
            }

            if (Gang == null)
            {
                Session.SendWhisper("Você não tem uma gangue para convidar alguém!", 1);
                return;
            }

            if (Gang.Id <= 1000)
            {
                Session.SendWhisper("Você não tem uma gangue para convidar alguém!", 1);
                return;
            }

            if (!GroupManager.HasGangCommand(Session, "gconvidar"))
            {
                Session.SendWhisper("Você não é um cargo alto suficiente para usar esse comando!", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode convidar alguém para sua gangue enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode convidar alguém para sua gangue enquanto está preso!", 1);
                return;
            }

            GameClients.GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            if (Session == TargetClient)
            {
                Session.SendWhisper("Você não pode se convidar para sua próprio gangue!", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().GangId > 1000)
            {
                Session.SendWhisper("Desculpe, esse usuário já é parte de uma gangue!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().OfferManager.ActiveOffers.ContainsKey("gangue"))
            {
                var Invite = TargetClient.GetRoleplay().OfferManager.ActiveOffers["gangue"];
                var InviteGang = GroupManager.GetGang(Invite.Cost);

                if (InviteGang != Gang)
                    Session.SendWhisper("Desculpe, este usuário já foi convidado para '" + InviteGang.Name + "'! Espere ele responder!", 1);
                else
                    Session.SendWhisper("Você já convidou este usuário para sua gangue! Espere ele responder!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("gconvidar"))
                return;
            #endregion

            #region Execute
            Session.GetRoleplay().CooldownManager.CreateCooldown("gconvidar", 1000, 5);
            Session.Shout("*Convida " + TargetClient.GetHabbo().Username + " para a sua gangue '" + Gang.Name + "'*", 4);
            TargetClient.SendWhisper("Você foi convidado para entrar na gangue '" + Gang.Name + "'! Digite ':aceitar gangue' para entrar!", 1);
            TargetClient.GetRoleplay().OfferManager.CreateOffer("gangue", Session.GetHabbo().Id, Gang.Id);
            #endregion
        }
    }
}