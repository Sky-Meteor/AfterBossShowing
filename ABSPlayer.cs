using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace AfterBossShowing
{
    public class ABSPlayer : ModPlayer
    {
        public bool ShowingInv;
        public (SlotType, int) ShowInvSlot;
        public SlotType OldSlotType;
        public Vector2 OldSlotPosition;
        public int ShowInvTimer;
        public Vector2 ShowInvMouseStart;
        public int[] ModdedSlotVisible;
        public int VanillaEquipmentsCount;
        public bool?[] LoadoutNeeded;
        public int DefaultLoadout;
        public bool ForceSwitchingLoadout;

        public static AccessorySlotLoader AccessorySlotLoader;
        public static Vector2 InventoryBackSize;

        public bool ShowingMods;
        public int ModlistTextTimer;
        public int ShowingModsEndTimer;
        public string ModlistText;
        public int ModsToScroll;
        public bool ScrollingModlist;

        public const string ModlistFullText = "/modlist";
        
        public override void PostUpdate()
        {
            if (ShowingMods)
                CalculateShowModlist();
        }
        
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;
            if (ABSConfig.Instance.BoundTwoKeys && (AfterBossShowing.ShowInvKey.JustPressed || AfterBossShowing.ShowModsKey.JustPressed))
            {
                if (!ShowingMods && !ShowingInv)
                {
                    ToggleInv();
                    ToggleModlist();
                }
                else if (ShowingInv)
                {
                    ToggleInv();
                }
            }
            else
            {
                if (AfterBossShowing.ShowInvKey.JustPressed)
                {
                    ToggleInv();
                }
                if (AfterBossShowing.ShowModsKey.JustPressed)
                {
                    ToggleModlist();
                }
            }
        }

        private void ToggleModlist()
        {
            ModlistText = "";
            ModlistTextTimer = 0;
            ShowingModsEndTimer = 0;
            ScrollingModlist = false;
            ModsToScroll = ModLoader.Mods.Length - 10 > 0 ? ModLoader.Mods.Length - 10 : 0;
            ShowingMods = !ShowingMods;
            if (!ShowingMods)
                Main.ClosePlayerChat();
            else if (!Main.drawingPlayerChat)
            {
                Main.OpenPlayerChat();
            }
        }

        private void ToggleInv()
        {
            ShowingInv = !ShowingInv;
            if (ShowingInv)
            {
                if (Main.myPlayer == Player.whoAmI && !Main.playerInventory)
                    Player.ToggleInv();
                if (Main.EquipPageSelected != 0)
                {
                    Main.EquipPageSelected = 0;
                    SoundEngine.PlaySound(SoundID.MenuOpen, Player.Center);
                }
                ShowInvSlot = (SlotType.Start, 0);
                OldSlotType = SlotType.Start;
                ShowInvTimer = 0;

                LoadoutNeeded = new bool?[] { false, false, false };
                LoadoutNeeded[Player.CurrentLoadoutIndex] = true;
                ForceSwitchingLoadout = false;
                DefaultLoadout = Player.CurrentLoadoutIndex;
                VanillaEquipmentsCount = 5 + Player.GetAmountOfExtraAccessorySlotsToShow() + 3;
                AccessorySlotLoader = LoaderManager.Get<AccessorySlotLoader>();
                CalculateModdedSlots();
                InventoryBackSize = TextureAssets.InventoryBack.Value.Frame().Size();

                ShowInvMouseStart = new Vector2(PlayerInput.MouseX, PlayerInput.MouseY);
                OldSlotPosition = ShowInvMouseStart;
            }
        }

        public Vector2 PlayerSlotPosition((SlotType, int) slot)
        {
            if (slot.Item1 == SlotType.Start || slot.Item1 == SlotType.End)
                return ShowInvMouseStart;
            float uiScale = Main.UIScale;
            float inventoryScale = 0.85f;
            int x;
            int y;
            int mH = 174 + (int)typeof(Main).GetField("mH", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!; // map height
            int slotNum = slot.Item2;
            int vanillaEquipLack = 10 - VanillaEquipmentsCount;
            switch (slot.Item1)
            {
                case SlotType.Inventory:
                    int i = slotNum % 10;
                    int j = slotNum / 10;
                    x = (int)(20f + i * inventoryScale * 56);
                    y = (int)(20f + j * inventoryScale * 56);
                    break;
                case SlotType.Coin: // should skip
                    return OldSlotPosition;
                case SlotType.Ammo:
                    inventoryScale = 0.6f;
                    x = 534;
                    y = (int)(85f + slotNum * 56 * inventoryScale + 20f);
                    break;
                case SlotType.FunctionalEquipment:
                    x = (int)(Main.screenWidth / uiScale) - 64 - 28;
                    if (slotNum >= 10)
                        slotNum -= vanillaEquipLack;
                    y = (int)(mH + slotNum * 56 * inventoryScale);
                    break;
                case SlotType.VanityEquipment:
                    x = (int)(Main.screenWidth / uiScale) - 64 - 28 - 47;
                    if (slotNum >= 10)
                        slotNum -= vanillaEquipLack;
                    y = (int)(mH + slotNum * 56 * inventoryScale);
                    break;
                case SlotType.DyeEquipment:
                    x = (int)(Main.screenWidth / uiScale) - 64 - 28 - 47 * 2;
                    if (slotNum >= 10)
                        slotNum -= vanillaEquipLack;
                    y = (int)(mH + slotNum * 56 * inventoryScale);
                    break;
                case SlotType.MiscEquipment:
                    x = (int)(Main.screenWidth / uiScale) - 92;
                    y = mH + slotNum * 47;
                    break;
                case SlotType.MiscDyeEquipment:
                    x = (int)(Main.screenWidth / uiScale) - 92 - 47;
                    y = mH + slotNum * 47;
                    break;
                default:
                    return OldSlotPosition;
            }
            return new Vector2(x * uiScale, y * uiScale) + InventoryBackSize * 0.6f * uiScale;
        }
        
        public void CalculateModdedSlots()
        {
            int extraSlots = Player.GetModPlayer<ModAccessorySlotPlayer>().SlotCount;
            ModdedSlotVisible = Array.Empty<int>();
            for (int i = 0; i < extraSlots; i++)
            {
                var moddedSlot = Player.GetModdedSlot(i);
                if ((moddedSlot.IsEnabled() || moddedSlot.IsVisibleWhenNotEnabled()) && !moddedSlot.IsHidden())
                    ModdedSlotVisible = ModdedSlotVisible.Append(i).ToArray();
            }
        }

        public void DetermineSlot()
        {
            ShowInvSlot = (ShowInvSlot.Item1, ++ShowInvSlot.Item2);
            Determine:
            while (Player.SlotHasItem(ShowInvSlot) == null)
            {
                if (ShowInvSlot.Item1 == SlotType.End)
                    return;
                ShowInvSlot = (++ShowInvSlot.Item1, 0);
                if (ShowInvSlot.Item1 == SlotType.FunctionalEquipment)
                {
                    for (int i = 0; i < LoadoutNeeded.Length; i++)
                    {
                        if (LoadoutNeeded[i] == true)
                        {
                            ForceSwitchingLoadout = true;
                            Player.TrySwitchingLoadout(i);
                            ForceSwitchingLoadout = false;
                            LoadoutNeeded[i] = null;
                            OldSlotType = SlotType.Ammo;
                            CalculateModdedSlots();
                            break;
                        }
                    }
                }
                else if (ShowInvSlot.Item1 > (ABSConfig.Instance.ShowVanityDye ? SlotType.DyeEquipment : SlotType.FunctionalEquipment))
                {
                    if (OldSlotType < SlotType.FunctionalEquipment)
                    {
                        ShowInvSlot = (SlotType.FunctionalEquipment, VanillaEquipmentsCount + ModdedSlotVisible.Length - 1);
                        return;
                    }

                    if (LoadoutNeeded.Count(n => n == true) > 0)
                    {
                        ShowInvSlot = (SlotType.FunctionalEquipment - 1, 114514);
                        goto Determine;
                    }
                    else if (Main.EquipPageSelected != 2)
                    {
                        Main.EquipPageSelected = 2;
                        for (int i = 0; i < LoadoutNeeded.Length; i++)
                            LoadoutNeeded[i] = null;
                        SoundEngine.PlaySound(SoundID.MenuOpen, Player.Center);
                    }
                }
            }
            while (Player.SlotHasItem(ShowInvSlot) == false)
            {
                ShowInvSlot = (ShowInvSlot.Item1, ++ShowInvSlot.Item2);
                if (Player.SlotHasItem(ShowInvSlot) == null)
                    goto Determine;
            }

            if (!ABSConfig.Instance.ShowVanityDye && ShowInvSlot.Item1 == SlotType.VanityEquipment)
            {
                ShowInvSlot = (SlotType.MiscEquipment, 0);
                goto Determine;
            }

            if (!ABSConfig.Instance.ShowVanityDye && ShowInvSlot.Item1 == SlotType.MiscDyeEquipment)
                ShowInvSlot = (SlotType.End, 0);
        }

        public Vector2 ShowInventory()
        {
            if (Main.myPlayer == Player.whoAmI && !Main.playerInventory) // stop if player closes inv
            {
                ShowingInv = false;
                return Vector2.Zero; // won't move
            }

            Vector2 position = OldSlotPosition;
            Vector2 destination = PlayerSlotPosition(ShowInvSlot);
            Vector2 distance = destination - position;
            ShowInvTimer++;
            if (ShowInvTimer <= ABSConfig.Instance.ShowInvSpeed)
            {
                position += distance * ShowInvTimer / ABSConfig.Instance.ShowInvSpeed;
            }
            else if (ShowInvTimer >= ABSConfig.Instance.ShowInvSpeed + ABSConfig.Instance.ShowInvDuration)
            {
                position = destination;
                if (ShowInvSlot.Item1 == SlotType.End)
                {
                    ShowingInv = false;
                    Player.TrySwitchingLoadout(DefaultLoadout);
                    Main.EquipPageSelected = 0;
                    SoundEngine.PlaySound(SoundID.MenuOpen, Player.Center);
                    if (ABSConfig.Instance.AutoCloseInv)
                        Player.ToggleInv();
                    return ShowInvMouseStart;
                }

                OldSlotType = ShowInvSlot.Item1;
                OldSlotPosition = destination;
                DetermineSlot();
                ShowInvTimer = 0;
            }
            else
            {
                position = destination;
            }

            return position;
        }
        
        public void CalculateShowModlist()
        {
            if (ScrollingModlist)
            {
                if (ModsToScroll > 0)
                {
                    if (!Main.drawingPlayerChat)
                        Main.OpenPlayerChat();

                    if (++ModlistTextTimer % ABSConfig.Instance.ShowModsScrollingSpeed == 0)
                    {
                        ModsToScroll--;
                        Main.chatMonitor.Offset(1);
                    }
                }
                else if (++ShowingModsEndTimer > ABSConfig.Instance.ShowModsEndDuration)
                {
                    Main.ClosePlayerChat();
                    ModlistTextTimer = 0;
                    ShowingModsEndTimer = 0;
                    ScrollingModlist = false;
                    ShowingMods = false;
                }
                return;
            }

            if (ModlistText == ModlistFullText)
            {
                ModlistText = "";
                ModlistTextTimer = 0;
                ScrollingModlist = true;
                Main.chatText = ModlistText;
                return;
            }
            
            if (!Main.drawingPlayerChat)
            {
                ModlistText = "";
                ModlistTextTimer = 0;
                ScrollingModlist = false;
                ShowingMods = false;
                Main.chatText = ModlistText;
                return;
            }
            
            if (ModlistTextTimer++ % ABSConfig.Instance.ShowModsTypingSpeed == 0 && ModlistText != ModlistFullText)
            {
                ModlistText += ModlistFullText[ModlistTextTimer / ABSConfig.Instance.ShowModsTypingSpeed];
                Main.chatText = ModlistText;
            }
        }

        public override void OnEnterWorld()
        {
            UpdateMainMouseHook.InWorld = true;
            Reset();
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            Reset();
        }
        
        public void Reset()
        {
            ShowingInv = false;
            ShowInvSlot = (SlotType.Start, 0);
            OldSlotType = SlotType.Start;
            ShowInvTimer = 0;
            ModdedSlotVisible = null;
            LoadoutNeeded = null;
            ForceSwitchingLoadout = false;
            DefaultLoadout = 0;

            ShowingMods = false;
            ModlistText = string.Empty;
            ModlistTextTimer = 0;
            ModsToScroll = 0;
            ScrollingModlist = false;
            ShowingModsEndTimer = 0;
        }

        public enum SlotType
        {
            Start,
            Inventory,
            Coin,
            Ammo,
            FunctionalEquipment,
            VanityEquipment,
            DyeEquipment,
            MiscEquipment,
            MiscDyeEquipment,
            End
        }
    }

    public static class ABSExtension
    {
        /// <summary>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="slotType"></param>
        /// <param name="slotNum"></param>
        /// <returns><see langword="null"></see> if end of array is met, otherwise, whether the slot has any item</returns>
        public static bool? SlotHasItem(this Player player, ABSPlayer.SlotType slotType, int slotNum)
        {
            ABSPlayer mp = player.GetModPlayer<ABSPlayer>();
            int moddedIndex = slotNum - 10;
            ModAccessorySlot moddedSlot;
            switch (slotType)
            {
                case ABSPlayer.SlotType.Inventory:
                    if (slotNum >= 50)
                        return null;
                    return !player.inventory[slotNum].IsAir;
                case ABSPlayer.SlotType.Ammo:
                    if (slotNum >= 4)
                        return null;
                    return !player.inventory[slotNum + 50 + 4].IsAir;
                case ABSPlayer.SlotType.FunctionalEquipment:
                    if (slotNum < 8)
                        return !player.armor[slotNum].IsAir;
                    if (slotNum == 8)
                        return (player.IsItemSlotUnlockedAndUsable(8) && !player.armor[8].IsAir) || (player.IsItemSlotUnlockedAndUsable(9) && !player.armor[9].IsAir);
                    if (slotNum == 9)
                        return player.IsItemSlotUnlockedAndUsable(8) && !player.armor[8].IsAir && player.IsItemSlotUnlockedAndUsable(9) && !player.armor[9].IsAir;
                    if (moddedIndex >= mp.ModdedSlotVisible.Length)
                        return null;
                    moddedSlot = player.GetModdedSlot(mp.ModdedSlotVisible[moddedIndex]);
                    return !moddedSlot.FunctionalItem.IsAir && moddedSlot.IsEnabled();
                case ABSPlayer.SlotType.VanityEquipment:
                    if (slotNum < 8)
                        return !player.armor[slotNum + 10].IsAir;
                    if (slotNum == 8)
                        return (player.IsItemSlotUnlockedAndUsable(8) && !player.armor[18].IsAir) || (player.IsItemSlotUnlockedAndUsable(9) && !player.armor[19].IsAir);
                    if (slotNum == 9)
                        return player.IsItemSlotUnlockedAndUsable(8) && !player.armor[18].IsAir && player.IsItemSlotUnlockedAndUsable(9) && !player.armor[19].IsAir;
                    if (moddedIndex >= mp.ModdedSlotVisible.Length)
                        return null;
                    moddedSlot = player.GetModdedSlot(mp.ModdedSlotVisible[moddedIndex]);
                    return !moddedSlot.VanityItem.IsAir && moddedSlot.IsEnabled();
                case ABSPlayer.SlotType.DyeEquipment:
                    if (slotNum < 8)
                        return !player.dye[slotNum].IsAir;
                    if (slotNum == 8)
                        return (player.IsItemSlotUnlockedAndUsable(8) && !player.dye[8].IsAir) || (player.IsItemSlotUnlockedAndUsable(9) && !player.dye[9].IsAir);
                    if (slotNum == 9)
                        return player.IsItemSlotUnlockedAndUsable(8) && !player.dye[8].IsAir && player.IsItemSlotUnlockedAndUsable(9) && !player.dye[9].IsAir;
                    if (moddedIndex >= mp.ModdedSlotVisible.Length)
                        return null;
                    moddedSlot = player.GetModdedSlot(mp.ModdedSlotVisible[moddedIndex]);
                    return !moddedSlot.DyeItem.IsAir && moddedSlot.IsEnabled();
                case ABSPlayer.SlotType.MiscEquipment:
                    if (slotNum >= 5)
                        return null;
                    return !player.miscEquips[slotNum].IsAir;
                case ABSPlayer.SlotType.MiscDyeEquipment:
                    if (slotNum >= 5)
                        return null;
                    return !player.miscDyes[slotNum].IsAir;
            }
            
            return null;
        }

        public static bool? SlotHasItem(this Player player, (ABSPlayer.SlotType, int) slot) => player.SlotHasItem(slot.Item1, slot.Item2);
        
        public static ModAccessorySlot GetModdedSlot(this Player player, int index) => ABSPlayer.AccessorySlotLoader.Get(index, player);
    }
}