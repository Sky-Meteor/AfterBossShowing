using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace AfterBossShowing;

public class UpdateMainMouseHook : ModSystem
{
    public static bool InWorld;
    public override void PreSaveAndQuit()
    {
        InWorld = false;
    }

    public override void Load()
    {
        On_PlayerInput.UpdateMainMouse += On_PlayerInput_UpdateMainMouse;
    }

    private void On_PlayerInput_UpdateMainMouse(On_PlayerInput.orig_UpdateMainMouse orig)
    {
        if (!InWorld || Main.showSplash || !Main.hasFocus)
        {
            orig.Invoke();
            return;
        }
        ABSPlayer p = Main.LocalPlayer.GetModPlayer<ABSPlayer>();
        if (p.ShowingInv)
        {
            Vector2 position = p.ShowInventory();
            PlayerInput.MouseInfoOld = PlayerInput.MouseInfo;
            
            var released = ButtonState.Released;
            PlayerInput.MouseInfo = new MouseState((int)position.X, (int)position.Y, 0, released, released, released, released, released);
            PlayerInput.MouseX = (int)position.X;
            PlayerInput.MouseY = (int)position.Y;
            orig.Invoke();
            PlayerInput.MouseKeys.Clear();
            PlayerInput.Triggers.Current.MouseLeft = false;
            PlayerInput.Triggers.Current.MouseRight = false;
            PlayerInput.CacheMousePositionForZoom();
        }
        else
        {
            orig.Invoke();
        }
    }
}