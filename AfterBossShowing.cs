using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace AfterBossShowing
{
	public class AfterBossShowing : Mod
    {
        internal static ModKeybind ShowInvKey;
        internal static ModKeybind ShowModsKey;

        public override void Load()
        {
            ShowInvKey = KeybindLoader.RegisterKeybind(this, "ShowInvKey", Keys.P);
            ShowModsKey = KeybindLoader.RegisterKeybind(this, "ShowModsKey", Keys.L);
        }

        public override void Unload()
        {
            ShowInvKey = null;
            ShowModsKey = null;
        }
    }
}