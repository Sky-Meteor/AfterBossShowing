using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace AfterBossShowing
{
    public class ABSConfig : ModConfig
    {
        public static ABSConfig Instance;

        public override void OnLoaded()
        {
            Instance = this;
        }

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("$Mods.AfterBossShowing.Configs.ABSConfig.ShowInvHeader")]
        [Range(1, 114514)]
        [DefaultValue(5)]
        public int ShowInvSpeed;

        [Range(0, 114514)]
        [DefaultValue(20)]
        public int ShowInvDuration;

        [DefaultValue(true)]
        public bool AutoCloseInv;

        [DefaultValue(true)]
        public bool ShowVanityDye;

        [Header("$Mods.AfterBossShowing.Configs.ABSConfig.ShowModsHeader")]
        [Range(2, 114514)]
        [DefaultValue(2)]
        public int ShowModsTypingSpeed;

        [Range(2, 114514)]
        [DefaultValue(2)]
        public int ShowModsScrollingSpeed;

        [Range(0, 114514)]
        [DefaultValue(20)]
        public int ShowModsDuration;
    }
}
