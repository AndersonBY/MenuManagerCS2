﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager
{
    internal static class Control
    {
        public static List<PlayerInfo> menus = new List<PlayerInfo>();
        private static MenuManagerCore hPlugin;

        public static void AddMenu(CCSPlayerController player, ButtonMenu inst)
        {
            for (int i = 0; i < menus.Count; i++)
                if (menus[i].GetPlayer() == player)
                {
                    menus.Remove(menus[i]);
                    i++;
                }

            var menu = new PlayerInfo(player, inst);
            menus.Add(menu);
        }

        public static void AddMenuAll(ButtonMenu inst)
        {
            var players = Utilities.GetPlayers();
            foreach (var player in players)
            {
                if (player != null && player.IsValid && !player.IsBot && !player.IsHLTV && player.Connected == PlayerConnectedState.PlayerConnected)
                    AddMenu(player, inst);
            }
        }

        public static void Clear()
        {
            menus.RemoveAll(x => true);
        }

        public static void OnPluginTick()
        {
            if (menus.Count > 0)
            {
                //foreach(var menu in menus)
                for (int i = 0; i < menus.Count; i++)
                {
                    var menu = menus[i];
                    if (menu == null)
                    {
                        menus.RemoveAt(i);
                        i--;
                        continue;
                    }
                    var player = menu.GetPlayer();
                    if (!Misc.IsValidPlayer(player))
                    {
                        menus.RemoveAt(i);
                        i--;
                        continue;
                    }
                    var buttons = player.Buttons;

                    if (player.Pawn.Value != null)
                    {
                        player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
                        Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 1);
                        Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
                    }

                    if (!menu.IsEqualButtons(buttons.ToString()))
                    {

                        if (buttons.HasFlag(PlayerButtons.Forward))
                            menu.MoveUp();
                        else if (buttons.HasFlag(PlayerButtons.Back))
                            menu.MoveDown();
                        else if (buttons.HasFlag(PlayerButtons.Moveleft))
                            menu.MoveUp(7);
                        else if (buttons.HasFlag(PlayerButtons.Moveright))
                            menu.MoveDown(7);
                        else if (buttons.HasFlag(PlayerButtons.Use))
                            menu.OnSelect();

                        if (buttons.HasFlag(PlayerButtons.Reload) || menu.Closed())
                        {
                            menu.Close(true);
                            if (player.Pawn.Value != null)
                            {
                                player.Pawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
                                Schema.SetSchemaValue(player.Pawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
                                Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_MoveType");
                            }
                            menus.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    menu.GetPlayer().PrintToCenterHtml(menu.GetText());
                }
            }
        }

        public static void PlaySound(CCSPlayerController player, string sound)
        {
            player.ExecuteClientCommand("play " + sound);
        }

        public static void CloseMenu(CCSPlayerController player)
        {
            CounterStrikeSharp.API.Modules.Menu.MenuManager.CloseActiveMenu(player);
            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i].GetPlayer() == player)
                {
                    menus[i].Close();
                }
            }
        }

        internal static void Init(MenuManagerCore _hPlugin)
        {
            hPlugin = _hPlugin;
        }

        internal static MenuManagerCore GetPlugin()
        {
            return hPlugin;
        }
    }
}
