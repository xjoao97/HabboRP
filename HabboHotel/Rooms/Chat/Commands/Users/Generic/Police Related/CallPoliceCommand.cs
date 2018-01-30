using System;
using System.Linq;
using System.Text;
using Plus.Utilities;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Guides;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Police
{
    class CallPoliceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_related_call_police"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Chama a ajuda da polícia."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para o policial ver!", 1);
                return;
            }

            if (Session.GetRoleplay().Sent911Call)
            {
                Session.SendWhisper("Seu último pedido de ajuda ainda não foi respondido!", 1);
                return;
            }

            if (Groups.GroupManager.HasJobCommand(Session, "guide") && Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você não pode enviar uma chamada de ajuda enquanto estiver trabalhando!", 1);
                return;
            }

            if (Room.TurfEnabled)
            {
                Session.SendWhisper("Você não pode enviar uma chamada para ajuda se você estiver dentro de um território!", 1);
                return;
            }

            if (Session.GetRoleplay().Game != null || Session.GetRoleplay().Team != null)
            {
                Session.SendWhisper("Você não pode enviar uma chamada para ajuda se você estiver dentro de um evento!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode enviar uma chamada para ajuda se estiver preso!", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
            List<GameClient> HandlingCalls = guideManager.HandlingCalls();

            if (Message.Length <= 9)
            {
                Session.SendWhisper("Digite uma mensagem mais descritiva para o policial ver! [10+ Dígitos]", 1);
                return;
            }

            if (HandlingCalls.Count < 1)
            {
                Session.SendMessage(new OnGuideSessionError());
                return;
            }

            CryptoRandom Random = new CryptoRandom();
            GameClient RandomPolice = null;

            if (HandlingCalls.Count > 1)
                RandomPolice = HandlingCalls[Random.Next(0, HandlingCalls.Count)];
            else
                RandomPolice = HandlingCalls[0];

            if (RandomPolice == null)
            {
                Session.SendMessage(new OnGuideSessionError());
                return;
            }

            Session.SendWhisper("Seu pedido de socorro foi enviado!", 1);
            RandomPolice.SendMessage(new OnGuideSessionAttachedComposer(Session.GetHabbo().Id, Message, 15));

            Session.GetRoleplay().SentRealCall = false;
            Session.GetRoleplay().Sent911Call = true;
            Session.GetRoleplay().CallMessage = Message;
            Session.GetRoleplay().GuideOtherUser = RandomPolice;
            RandomPolice.GetRoleplay().GuideOtherUser = Session;
        }
    }
}