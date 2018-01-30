using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class CallDeliveryCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_call_delivery"; }
        }

        public string Parameters
        {
            get { return "%item%"; }
        }

        public string Description
        {
            get { return "Chama o remetente a entregar um determinado item."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor, insira o item que você gostaria de entregar!", 1);
                return;
            }

            if (RoleplayManager.CalledDelivery)
            {
                Session.SendWhisper("O remetente está muito ocupado agora mesmo! Por favor, tente novamente mais tarde.", 1);
                return;
            }

            bool DeliveryCame = false;

            string Item = Params[1];

            switch (Item.ToLower())
            {
                #region Weapons
                case "glock":
                case "magnum":
                case "mp5":
				case "pistola":
                    {
                        if (!Room.DeliveryEnabled)
                        {
                            Session.SendWhisper("O remetente não entrega a este quarto!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para chamar o homem de entrega!", 1);
                            break;
                        }

                        if (!GroupManager.HasJobCommand(Session, "weapon") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você não trabalha na loja de armas!", 1);
                            break;
                        }

                        var Weapon = WeaponManager.getWeapon(Item.ToLower());

                        if (Weapon == null)
                        {
                            Session.SendWhisper("Por algum motivo, esta arma não pôde ser encontrada", 1);
                            break;
                        }

                        if (Weapon.Stock > 0)
                        {
                            Session.SendWhisper("Aguarde até o " + Weapon.PublicName + " acabar o estoque para chamar o entregador!", 1);
                            break;
                        }

                        RoleplayBot Bot = RoleplayBotManager.GetCachedBotByAI(RoleplayBotAIType.DELIVERY);

                        if (Bot == null)
                        {
                            Session.SendWhisper("Nenhum mecanismo de entrega foi encontrado, entre em contato com um membro da equipe!", 1);
                            break;
                        }

                        RoleplayManager.UserWhoCalledDelivery = Session.GetHabbo().Id;
                        RoleplayManager.CalledDelivery = true;
                        RoleplayManager.DeliveryWeapon = Weapon;

                        new Thread(() =>
                        {
                            if (Session.GetRoomUser() != null)
                            {
                                Session.Shout("*Pega seu telefone e chama o remetente, ordenando um novo estoque de " + Weapon.PublicName + "*", 4);
                                Session.GetRoomUser().ApplyEffect(EffectsList.CellPhone);
                            }

                            Thread.Sleep(3000);

                            if (Session.GetRoomUser() != null)
                                Session.GetRoomUser().ApplyEffect(0);
                        }).Start();

                        var BotUser = RoleplayBotManager.GetDeployedBotById(Bot.Id);
                        new Thread(() =>
                        {
                            Thread.Sleep(15000);

                            
                            RoleplayBot DeliverrBot = RoleplayBotManager.GetCachedBotByAI(RoleplayBotAIType.DELIVERY);

                            if (!DeliveryCame)
                            {
                                if (DeliverrBot == null)
                                {
                                    Session.SendWhisper("Não é possível obter o bot de entrega, tente novamente mais tarde!", 1);
                                    Thread.CurrentThread.Abort();
                                    return;
                                }
                                else
                                {
                                    RoleplayBotManager.DeployBotByAI(RoleplayBotAIType.DELIVERY, "default", Room.Id);
                                    DeliveryCame = true;
                                }
                            }

                            while (Room != null && Room.GetRoomItemHandler() != null && Room.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.DELIVERY_BOX).ToList().Count <= 0)
                            {
                                Thread.Sleep(10);
                            }

                            Thread.Sleep(2000);
                            RoleplayManager.CalledDelivery = false;
                        }).Start();
                        break;
                    }
                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("Isso não é um item entregue!", 1);
                        break;
                    }
                    #endregion
            }
        }
    }
}