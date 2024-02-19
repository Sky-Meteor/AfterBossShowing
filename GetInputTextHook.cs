using Terraria;
using Terraria.ModLoader;

namespace AfterBossShowing
{
    public class GetInputTextHook : ModSystem
    {
        private string Main_GetInputText(On_Main.orig_GetInputText orig, string oldString, bool allowMultiLine)
        {
            string s = orig.Invoke(oldString, allowMultiLine);
            if (Main.drawingPlayerChat && Main.LocalPlayer.GetModPlayer<ABSPlayer>().ShowingMods && s == ABSPlayer.ModlistFullText)
            {
                Main.inputTextEnter = true;
            }
            return s;
        }

        public override void Load()
        {
            On_Main.GetInputText += Main_GetInputText;
        }
        public override void Unload()
        {
            On_Main.GetInputText -= Main_GetInputText;
        }
    }
}