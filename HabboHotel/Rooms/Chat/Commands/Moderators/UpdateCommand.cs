using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Turfs;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Food;
using Plus.HabboHotel.Items.Crafting;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Farming;
using Plus.HabboRoleplay.Gambling;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators
{
    class UpdateCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_update"; }
        }

        public string Parameters
        {
            get { return "%variável%"; }
        }

        public string Description
        {
            get { return "Recarregar uma parte específica do hotel."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Você deve incluir uma coisa para atualizar, e.x. :atualizar catalogo", 1);
                return;
            }

            string UpdateVariable = Params[1];
            switch (UpdateVariable.ToLower())
            {
                case "cata":
                case "catalog":
                case "catalogue":
				case "catalogo":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_catalog"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_catalog'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetCatalog().Init(PlusEnvironment.GetGame().GetItemManager());
                        PlusEnvironment.GetGame().GetClientManager().SendMessage(new CatalogUpdatedComposer());
                        Session.SendWhisper("Catálogo atualizado com sucesso.", 1);
                        break;
                    }

                case "items":
                case "furni":
                case "furniture":
				case "mobis":
				case "mobi":
				case "mobilias":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_furni"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_furni'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetItemManager().Init();
                        Session.SendWhisper("Itens/Mobis atualizados com sucesso.", 1);
                        break;
                    }

                case "models":
				case "modelos":
				case "model":
				case "quartos":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_models"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_models'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetRoomManager().LoadModels();
                        Session.SendWhisper("Modelos de quartos atualizados com sucesso.", 1);
                        break;
                    }

                case "promotions":
				case "promocoes":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_promotions"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_promotions'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetLandingManager().LoadPromotions();
                        Session.SendWhisper("Landing view promotions successfully updated.", 1);
                        break;
                    }

                case "youtube":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_youtube"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_youtube'", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetTelevisionManager().Init();
                        Session.SendWhisper("A lista de televisões do Youtube foi atualizada com sucesso. ", 1);
                        break;
                    }

                case "filter":
				case "filtro":
				case "filtros":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_filter"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_filter'", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetChatManager().GetFilter().Init();
                        Session.SendWhisper("Definições de filtro atualizadas com sucesso.", 1);
                        break;
                    }

                case "navigator":
				case "navegador":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_navigator"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_navigator'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetNavigator().Init();
                        Session.SendWhisper("Itens do navegador atualizados com sucesso.", 1);
                        break;
                    }

                case "ranks":
                case "rights":
                case "permissions":
				case "cargos":
				case "permissao":
				case "permissoes":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_permissions"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_rights'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetPermissionManager().Init();

                        foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().GetPermissions() == null)
                                continue;

                            Client.GetHabbo().GetPermissions().Init(Client.GetHabbo());
                        }

                        Session.SendWhisper("Definições de Cargo atualizadas com sucesso.", 1);
                        break;
                    }

                case "config":
                case "settings":
				case "configuracoes":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_configuration"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_configuration'.", 1);
                            break;
                        }

                        PlusEnvironment.ConfigData = new ConfigData();
                        Session.SendWhisper("Configuração do servidor atualizada com sucesso.", 1);
                        break;
                    }

                case "bans":
				case "ban":
				case "banimentos":
				case "banidos":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_bans"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_bans'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetModerationManager().ReCacheBans();
                        Session.SendWhisper("A lista de banidos foi atualizada com sucesso.", 1);
                        break;
                    }

                case "quests":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_quests"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_quests'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetQuestManager().Init();
                        Session.SendWhisper("Definições de Tarefas atualizadas com êxito.", 1);
                        break;
                    }

                case "achievements":
				case "conquistas":
				case "conquista":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_achievements"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_achievements'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetAchievementManager().LoadAchievements();
                        Session.SendWhisper("Definições de conquistas atualizadas com êxito.", 1);
                        break;
                    }

                case "clothing":
				case "visuais":
				case "roupas":
				case "visual":
				case "roupa": 
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_clothing"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_clothing'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetCatalog().GetClothingManager().Init();
                        Session.SendWhisper("Mobiliário de vestuário e preços recarregados.", 1);
                        break;
                    }

                case "moderation":
				case "moderacao":
				case "mods":
				case "moderadores":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_moderation"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_moderation'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetModerationManager().Init();
                        PlusEnvironment.GetGame().GetClientManager().ModAlert("As predefinições de moderação foram atualizadas. Recarregue o cliente para ver as novas predefinições.");

                        Session.SendWhisper("Configuração de moderação atualizada com sucesso.", 1);
                        break;
                    }

                case "tickets":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_tickets"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_tickets'.", 1);
                            break;
                        }

                        if (PlusEnvironment.GetGame().GetModerationTool().Tickets.Count > 0)
                            PlusEnvironment.GetGame().GetModerationTool().Tickets.Clear();

                        PlusEnvironment.GetGame().GetClientManager().ModAlert("Os ingressos foram atualizados. Recarregue o cliente.");
                        Session.SendWhisper("Os ingressos foram atualizados com sucesso.", 1);
                        break;
                    }

                case "vouchers":
				case "brindes":
				case "codigos":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_vouchers"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_vouchers'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetCatalog().GetVoucherManager().Init();
                        Session.SendWhisper("Vouchers/Códigos brinde atualizado com sucesso.", 1);
                        break;
                    }

                case "polls":
				case "perguntas":
				case "quiz":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_polls"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_polls'.", 1);
                            break;
                        }

                        int PollLoaded;
                        PlusEnvironment.GetGame().GetPollManager().Init(out PollLoaded);
                        Session.SendWhisper("Polls successfully updated.", 1);
                        break;
                    }

                case "gamecenter":
				case "centrodejogos":
				case "cjogos":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_game_center"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_game_center'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetGameDataManager().Init();
                        Session.SendWhisper("Cache do Centro de Jogos atualizado com sucesso.", 1);
                        break;
                    }

                case "pet_locale":
				case "lpet":
				case "petl":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_pet_locale"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_pet_locale'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetChatManager().GetPetLocale().Init();
                        Session.SendWhisper("Pet locale cache successfully updated.", 1);
                        break;
                    }

                case "locale":
				case "local":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_locale"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_locale'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetLanguageLocale().Init();
                        Session.SendWhisper("Local cache atualizado com sucesso.", 1);
                        break;
                    }

                case "mutant":
				case "mutante":
				case "mutantes":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_anti_mutant"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_anti_mutant'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetAntiMutant().Init();
                        Session.SendWhisper("Anti mutante recarregado com sucesso.", 1);
                        break;
                    }

                case "bots":
				case "bot":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_bots"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_bots'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetBotManager().Init();
                        Session.SendWhisper("O gerenciador de Bots foi recarregado com sucesso", 1);
                        break;
                    }

                case "bots_speech":
                case "bots speech":
                case "speech":
                case "speeches":
                case "response":
                case "responses":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_bots"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_bots'", 1);
                            break;
                        }

                        RoleplayBotManager.FetchCachedSpeeches();
                        Session.SendWhisper("Bots speech and responses successfully reloaded", 1);
                        break;
                    }

                case "rewards":
				case "premios":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_rewards"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_rewards'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetRewardManager().Reload();
                        Session.SendWhisper("O Gerenciador de recompensas foi recarregado com sucesso.", 1);
                        break;
                    }

                case "chat_styles":
				case "echats":
				case "estilos":
				case "estilo":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_chat_styles"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_chat_styles'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetChatManager().GetChatStyles().Init();
                        Session.SendWhisper("Estilos de bate-papo recarregados com sucesso.", 1);
                        break;
                    }

                case "badges":
                case "badge_definitions":
				case "emblemas":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_badge_definitions"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_badge_definitions'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetBadgeManager().Init();
                        Session.SendWhisper("Definições de emblemas recarregadas com sucesso.", 1);
                        break;
                    }

                case "rpdata":
                case "roleplaydata":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_roleplay_data"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_roleplaydata'.", 1);
                            break;
                        }

                        RoleplayData.Initialize();
                        RoleplayManager.UpdateRPData();
                        Session.SendWhisper("Dados do Roleplay recarregados com sucesso.", 1);
                        break;
                    }

                case "blacklist":
				case "listanegra":
				case "ln":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_blacklist"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_blacklist'.", 1);
                            break;
                        }

                        BlackListManager.Initialize();
                        Session.SendWhisper("Lista negra recarregada com sucesso.", 1);
                        break;
                    }

                case "farming":
				case "agricultura":
				case "agricolas":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_farming"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_farming'.", 1);
                            break;
                        }

                        FarmingManager.Initialize();
                        Session.SendWhisper("Artigos agrícolas recarregados com sucesso.", 1);
                        break;
                    }

                case "events":
                case "games":
				case "jogos":
				case "eventos":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_events"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_events'.", 1);
                            break;
                        }

                        RoleplayGameManager.Initialize();
                        Session.SendWhisper("Eventos recarregados com sucesso.", 1);
                        break;
                    }

                case "corps":
                case "jobs":
                case "corporations":
                case "gangs":
				case "gangues":
				case "empregos":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_jobs"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_jobs'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetGroupManager().Initialize();
                        Session.SendWhisper("Trabalhos e Gangues recarregados com sucesso.", 1);
                        break;
                    }

                case "turfs":
                case "turfcaptures":
                case "gangcaptures":
				case "territorio":
				case "territorios":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_turfs"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_turfs'.", 1);
                            break;
                        }

                        TurfManager.Initialize();
                        Session.SendWhisper("Territórios e Zona de Captura recarregados com sucesso.", 1);
                        break;
                    }

                case "weapons":
                case "guns":
				case "armas":
				case "arma":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_weapons"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_weapons'.", 1);
                            break;
                        }

                        WeaponManager.Initialize();

                        #region Refresh User Weapons

                        lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
                        {
                            foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                if (Client == null || Client.GetHabbo() == null || Client.GetRoleplay() == null)
                                    continue;

                                if (Client.GetRoleplay().EquippedWeapon == null)
                                    continue;

                                Client.GetRoleplay().EquippedWeapon = null;

                                Client.GetRoleplay().OwnedWeapons = null;
                                Client.GetRoleplay().OwnedWeapons = Client.GetRoleplay().LoadAndReturnWeapons();

                                Client.SendWhisper("Um administrador atualizou as armas, sua arma foi retirada, equipe de novo!", 1);
                            }
                        }

                        #endregion

                        Session.SendWhisper("Armas recarregadas com sucesso.", 1);
                        break;
                    }

                case "food":
                case "drinks":
				case "bebidas":
				case "bebida":
				case "alimentos":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_food"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_food'.", 1);
                            break;
                        }

                        FoodManager.Initialize();
                        Session.SendWhisper("Alimentos e bebidas recarregados com sucesso.", 1);
                        break;
                    }

                case "houses":
                case "house":
				case "casas":
				case "casa":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_houses"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_houses'.", 1);
                            break;
                        }

                        PlusEnvironment.GetGame().GetHouseManager().Init();
                        Session.SendWhisper("Casas recarregadas com sucesso.", 1);
                        break;
                    }

                case "crafting":
				case "construir":
				case "construcao":
				case "craft":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_crafting"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_crafting'.", 1);
                            break;
                        }

                        CraftingManager.Initialize();
                        Session.SendWhisper("Áreas de construção recarregadas com sucesso..", 1);
                        break;
                    }

                case "lottery":
				case "loteria":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_lottery"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_lottery'.", 1);
                            break;
                        }

                        LotteryManager.Initialize();
                        Session.SendWhisper("Loteria recarregados com sucesso.", 1);
                        break;
                    }

                case "todo":
				case "tarefas":
				case "tarefa":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_todo"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_todo'.", 1);
                            break;
                        }

                        ToDoManager.Initialize();
                        Session.SendWhisper("Lista de Tarefas recarregada com sucesso.", 1);
                        break;
                    }

                case "bounty":
                case "bl":
                case "bounties":
				case "recompensa":
				case "recompensas":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_bounty"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_bounty'.", 1);
                            break;
                        }

                        BountyManager.Initialize();
                        Session.SendWhisper("Lista de recompensas recarregada com sucesso.", 1);
                        break;
                    }

                case "court":
                case "jury":
				case "juiz":
				case "politica":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_court"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_court'.", 1);
                            break;
                        }

                        RoleplayManager.CourtVoteEnabled = false;
                        RoleplayManager.InnocentVotes = 0;
                        RoleplayManager.GuiltyVotes = 0;

                        RoleplayManager.CourtJuryTime = 0;
                        RoleplayManager.CourtTrialIsStarting = false;
                        RoleplayManager.CourtTrialStarted = false;
                        RoleplayManager.Defendant = null;
                        RoleplayManager.InvitedUsersToJuryDuty.Clear();

                        Session.SendWhisper("Juiz atualizado com sucesso.", 1);
                        break;
                    }

                case "chat":
                case "chats":
                case "chatroom":
                case "chatrooms":
				case "batepapos":
				case "batepapo":
				case "whatsapps":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_websocket_chat"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_websocket_chat'.", 1);
                            break;
                        }

                        HabboRoleplay.Web.Util.ChatRoom.WebSocketChatManager.Initialiaze();
                        Session.SendWhisper("Salas de chat atualizadas com sucesso.", 1);
                        break;
                    }

                case "gambling":
				case "texash":
				case "texas":
                case "texasholdem":
                    {
                        if (!Session.GetHabbo().GetPermissions().HasCommand("command_update_gambling"))
                        {
                            Session.SendWhisper("Opa, você não tem permissão em 'command_update_gambling'.", 1);
                            break;
                        }

                        TexasHoldEmManager.Initialize();
                        Session.SendWhisper("Jogos do Texas Holdem atualizados com sucesso.", 1);
                        break;
                    }

                default:
                    Session.SendWhisper("'" + UpdateVariable + "' não é uma coisa válida para recarregar.", 1);
                    break;
            }
        }
    }
}
