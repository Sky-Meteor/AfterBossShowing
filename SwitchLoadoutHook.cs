using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AfterBossShowing
{
    public class SwitchLoadoutHook : ModSystem
    {
        public override void Load()
        {
            On_Player.TrySwitchingLoadout += On_Player_TrySwitchingLoadout;
        }

        private void On_Player_TrySwitchingLoadout(On_Player.orig_TrySwitchingLoadout orig, Player self, int loadoutIndex)
        {
            ABSPlayer mp = self.GetModPlayer<ABSPlayer>();
            if (mp.ShowingInv && !mp.ForceSwitchingLoadout && loadoutIndex >= 0 && loadoutIndex <= 2)
            {
                if (mp.LoadoutNeeded[loadoutIndex] != false)
                    return;
                mp.LoadoutNeeded[loadoutIndex] = true;
                SoundEngine.PlaySound(SoundID.MenuTick, self.Center);
            }
            else
            {
                orig.Invoke(self, loadoutIndex);
            }
        }

        public override void Unload()
        {
            On_Player.TrySwitchingLoadout -= On_Player_TrySwitchingLoadout;
        }
    }
}