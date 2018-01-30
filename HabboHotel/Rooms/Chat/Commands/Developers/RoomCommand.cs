using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class RoomCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_room"; }
        }

        public string Parameters
        {
            get { return "%type%"; }
        }

        public string Description
        {
            get { return "Dá-lhe a capacidade de ativar ou desativar os comandos básicos da sala."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você deve escolher uma opção de sala para desativar.", 1);
                return;
            }

            string Option = Params[1];
            switch (Option)
            {
                case "list":
				case "confs":
				case "lista":
				case "configuracoes":
                    {
                        StringBuilder List = new StringBuilder();
                        List.Append("---------- Configurações do Quarto ----------\n\n");
                        List.Append("[pets] Virar Pet: " + (Room.PetMorphsAllowed == true ? "ativado" : "desativado") + "\n");
                        List.Append("[puxar] Puxar: " + (Room.PullEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[empurrar] Empurrar: " + (Room.PushEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[puxao] Super puxar: " + (Room.SPullEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[empurrao] Super empurrar: " + (Room.SPushEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[respeito] Respeito: " + (Room.RespectNotificationsEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[efeitos] Efeitos: " + (Room.EnablesEnabled == true ? "ativado" : "desativado") + "\n\n");
                        List.Append("---------- Configurações de RolePlay ----------\n\n");
                        List.Append("[banco] Banco: " + (Room.BankEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[atirar] Atirar: " + (Room.ShootEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[bater] Bater: " + (Room.HitEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[seguro] Zona segura: " + (Room.SafeZoneEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[roubar] Roubar: " + (Room.RobEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[sexo] Comandos de Sexo: " + (Room.SexCommandsEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[territorio] Território: " + (Room.TurfEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[academia] Academia: " + (Room.GymEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[entrega] Entrega: " + (Room.DeliveryEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[tutorial] Tutorial: " + (Room.TutorialEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[dirigir] Dirigir: " + (Room.DriveEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[virtaxi] Vir de taxi: " + (Room.TaxiToEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[irtaxi] Ir de taxi: " + (Room.TaxiFromEnabled == true ? "ativado" : "desativado") + "\n");
                        List.Append("[mensagem] Mensagem de Entrada: " + Room.EnterRoomMessage + "\n");
                        Session.SendNotification(List.ToString());
                        break;
                    }

                case "push":
				case "empurrar":
                    {
                        Room.PushEnabled = !Room.PushEnabled;
                        Room.RoomData.PushEnabled = Room.PushEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `push_enabled` = @PushEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(Room.PushEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("O modo de empurrar agora está " + (Room.PushEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "spush":
				case "sempurrar":
				case "empurrao":
                    {
                        Room.SPushEnabled = !Room.SPushEnabled;
                        Room.RoomData.SPushEnabled = Room.SPushEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spush_enabled` = @PushEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PlusEnvironment.BoolToEnum(Room.SPushEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("O modo Super empurrar agora está " + (Room.SPushEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "spull":
				case "spuxar":
				case "puxao":
                    {
                        Room.SPullEnabled = !Room.SPullEnabled;
                        Room.RoomData.SPullEnabled = Room.SPullEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(Room.SPullEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("O modo Super puxar agora está " + (Room.SPullEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "pull":
				case "puxar":
                    {
                        Room.PullEnabled = !Room.PullEnabled;
                        Room.RoomData.PullEnabled = Room.PullEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PlusEnvironment.BoolToEnum(Room.PullEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("O modo puxar agora está" + (Room.PullEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "enable":
                case "enables":
				case "efeito":
				case "efeitos":
                    {
                        Room.EnablesEnabled = !Room.EnablesEnabled;
                        Room.RoomData.EnablesEnabled = Room.EnablesEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `enables_enabled` = @EnablesEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("EnablesEnabled", PlusEnvironment.BoolToEnum(Room.EnablesEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Modo de enables agora está " + (Room.EnablesEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "respect":
				case "respeito":
                    {
                        Room.RespectNotificationsEnabled = !Room.RespectNotificationsEnabled;
                        Room.RoomData.RespectNotificationsEnabled = Room.RespectNotificationsEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `respect_notifications_enabled` = @RespectNotificationsEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RespectNotificationsEnabled", PlusEnvironment.BoolToEnum(Room.RespectNotificationsEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Notificações de respeito agora está " + (Room.RespectNotificationsEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "bank":
				case "banco":
                    {
                        Room.BankEnabled = !Room.BankEnabled;
                        Room.RoomData.BankEnabled = Room.BankEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `bank_enabled` = @BankEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("BankEnabled", PlusEnvironment.BoolToEnum(Room.BankEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Modo de Banco agora está " + (Room.BankEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "taxito":
				case "virtaxi":
                    {
                        Room.TaxiToEnabled = !Room.TaxiToEnabled;
                        Room.RoomData.TaxiToEnabled = Room.TaxiToEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `taxi_to_enabled` = @TaxiToEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TaxiToEnabled", PlusEnvironment.BoolToEnum(Room.TaxiToEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Taxi agora está " + (Room.TaxiToEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "taxifrom":
				case "irtaxi":
                    {
                        Room.TaxiFromEnabled = !Room.TaxiFromEnabled;
                        Room.RoomData.TaxiFromEnabled = Room.TaxiFromEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `taxi_from_enabled` = @TaxiFromEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TaxiFromEnabled", PlusEnvironment.BoolToEnum(Room.TaxiFromEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Taxi agora está " + (Room.TaxiFromEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "shoot":
				case "atirar":
				case "tiro":
                    {
                        Room.ShootEnabled = !Room.ShootEnabled;
                        Room.RoomData.ShootEnabled = Room.ShootEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `shoot_enabled` = @ShootEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("ShootEnabled", PlusEnvironment.BoolToEnum(Room.ShootEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Tiro agora está " + (Room.ShootEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "hit":
				case "soco":
				case "bater":
                    {
                        Room.HitEnabled = !Room.HitEnabled;
                        Room.RoomData.HitEnabled = Room.HitEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `hit_enabled` = @HitEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("HitEnabled", PlusEnvironment.BoolToEnum(Room.HitEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Soco agora está " + (Room.HitEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "drive":
                case "car":
				case "dirigir":
                    {
                        Room.DriveEnabled = !Room.DriveEnabled;
                        Room.RoomData.DriveEnabled = Room.DriveEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `drive_enabled` = @DriveEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("DriveEnabled", PlusEnvironment.BoolToEnum(Room.DriveEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Dirigir agora está " + (Room.DriveEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "tutorial":
                    {
                        Room.TutorialEnabled = !Room.TutorialEnabled;
                        Room.RoomData.TutorialEnabled = Room.TutorialEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `tutorial_enabled` = @TutorialEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TutorialEnabled", PlusEnvironment.BoolToEnum(Room.TutorialEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Tutorial agora está " + (Room.TutorialEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "rob":
				case "roubar":
                    {
                        Room.RobEnabled = !Room.RobEnabled;
                        Room.RoomData.RobEnabled = Room.RobEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `rob_enabled` = @RobEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RobEnabled", PlusEnvironment.BoolToEnum(Room.RobEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Roubar agora está " + (Room.RobEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "sex":
                case "sexcommands":
				case "sexo":
				case "comandossexo":
                    {
                        Room.SexCommandsEnabled = !Room.SexCommandsEnabled;
                        Room.RoomData.SexCommandsEnabled = Room.SexCommandsEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `sexcommands_enabled` = @SexCommandsEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("SexCommandsEnabled", PlusEnvironment.BoolToEnum(Room.SexCommandsEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Comandos de Sexo agora está " + (Room.SexCommandsEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "turf":
				case "territorio":
                    {
                        Room.TurfEnabled = !Room.TurfEnabled;
                        Room.RoomData.TurfEnabled = Room.TurfEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `turf_enabled` = @TurfEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TurfEnabled", PlusEnvironment.BoolToEnum(Room.TurfEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Território agora está " + (Room.TurfEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "gym":
				case "academia":
				case "malhar":
                    {
                        Room.GymEnabled = !Room.GymEnabled;
                        Room.RoomData.GymEnabled = Room.GymEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `gym_enabled` = @GymEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("GymEnabled", PlusEnvironment.BoolToEnum(Room.GymEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Academia agora está " + (Room.GymEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "delivery":
				case "entrega":
                    {
                        Room.DeliveryEnabled = !Room.DeliveryEnabled;
                        Room.RoomData.DeliveryEnabled = Room.DeliveryEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `delivery_enabled` = @DeliveryEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("DeliveryEnabled", PlusEnvironment.BoolToEnum(Room.DeliveryEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Entrega agora está " + (Room.DeliveryEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "message":
				case "mensagem":
                    {
                        if (Params.Length < 3)
                        {
                            Session.SendWhisper("Você precisa digitar uma mensagem de entrada do quarto!", 1);
                            break;
                        }

                        string Message = CommandManager.MergeParams(Params, 2);

                        Room.EnterRoomMessage = Message;
                        Room.RoomData.EnterRoomMessage = Room.EnterRoomMessage;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `enter_message` = @EnterMessage WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("EnterMessage", Room.EnterRoomMessage);
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("A mensagem de entrada agora é: " + Room.EnterRoomMessage, 1);
                        break;
                    }

                case "safezone":
				case "seguro":
                    {
                        Room.SafeZoneEnabled = !Room.SafeZoneEnabled;
                        Room.RoomData.SafeZoneEnabled = Room.SafeZoneEnabled;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `safezone_enabled` = @SafeZoneEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("SafeZoneEnabled", PlusEnvironment.BoolToEnum(Room.SafeZoneEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de Zona Segura agora está " + (Room.SafeZoneEnabled == true ? "ativado!" : "desativado!"), 1);
                        break;
                    }

                case "pets":
                case "morphs":
				case "animais":
                    {
                        Room.PetMorphsAllowed = !Room.PetMorphsAllowed;
                        Room.RoomData.PetMorphsAllowed = Room.PetMorphsAllowed;
                        PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pet_morphs_allowed` = @PetMorphsAllowed WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PetMorphsAllowed", PlusEnvironment.BoolToEnum(Room.PetMorphsAllowed));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Configurações de pet/transformações agora está " + (Room.PetMorphsAllowed == true ? "ativado!" : "desativado!"), 1);

                        if (!Room.PetMorphsAllowed)
                        {
                            foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                            {
                                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                                    continue;

                                User.GetClient().SendWhisper("O dono da sala desativou a habilidade de usar um animal de estimação em sua casa.", 1);
                                if (User.GetClient().GetHabbo().PetId > 0)
                                {
                                    // Tell the user what is going on.
                                    User.GetClient().SendWhisper("Opa, o dono da sala desativou a habilidade de usar um animal de estimação em sua casa.", 1);

                                    // Change the users Pet Id.
                                    User.GetClient().GetHabbo().PetId = 0;

                                    // Quickly remove the old user instance.
                                    Room.SendMessage(new UserRemoveComposer(User.VirtualId));

                                    // Add the new one, they won't even notice a thing.
                                    Room.SendMessage(new UsersComposer(User));
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }
}
