using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using Mouse = Blish_HUD.Controls.Intern.Mouse;

namespace Nekres.Mistwar
{
    internal static class ChatUtil
    {
        private static readonly IReadOnlyDictionary<ModifierKeys, VirtualKeyShort> ModifierLookUp = new Dictionary<ModifierKeys, VirtualKeyShort>
        {
            {ModifierKeys.Alt, VirtualKeyShort.MENU},
            {ModifierKeys.Ctrl, VirtualKeyShort.CONTROL},
            {ModifierKeys.Shift, VirtualKeyShort.SHIFT}
        };

        public static async Task PastText(string text)
        {
            var prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            if (!await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)) return;
            Focus();
            Keyboard.Press(VirtualKeyShort.LCONTROL, true);
            Keyboard.Stroke(VirtualKeyShort.KEY_A, true);
            Thread.Sleep(1);
            Keyboard.Release(VirtualKeyShort.LCONTROL, true);
            Keyboard.Stroke(VirtualKeyShort.DELETE, true);
            Keyboard.Press(VirtualKeyShort.LCONTROL, true);
            Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
            Thread.Sleep(1);
            Keyboard.Release(VirtualKeyShort.LCONTROL, true);
            Keyboard.Stroke(VirtualKeyShort.RETURN);
            UnFocus();
            if (prevClipboardContent == null) return;
            await ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
        }

        public static async Task InsertText(string text)
        {
            var prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            if (!await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)) return;
            Focus();
            Keyboard.Press(VirtualKeyShort.LCONTROL, true);
            Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
            Thread.Sleep(50);
            Keyboard.Release(VirtualKeyShort.LCONTROL, true);
            if (prevClipboardContent == null) return;
            await ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
        }

        private static void Focus()
        {
            UnFocus();

            if (MistwarModule.ModuleInstance.ChatMessageKeySetting.Value.ModifierKeys != ModifierKeys.None)
            {
                Keyboard.Press(ModifierLookUp[MistwarModule.ModuleInstance.ChatMessageKeySetting.Value.ModifierKeys]);
            }
            if (MistwarModule.ModuleInstance.ChatMessageKeySetting.Value.PrimaryKey != Keys.None)
            {
                Keyboard.Press((VirtualKeyShort)MistwarModule.ModuleInstance.ChatMessageKeySetting.Value.PrimaryKey);
                Keyboard.Release((VirtualKeyShort)MistwarModule.ModuleInstance.ChatMessageKeySetting.Value.PrimaryKey);
            }
            if (MistwarModule.ModuleInstance.ChatMessageKeySetting.Value.ModifierKeys != ModifierKeys.None)
            {
                Keyboard.Release(ModifierLookUp[MistwarModule.ModuleInstance.ChatMessageKeySetting.Value.ModifierKeys]);
            }
        }

        private static void UnFocus()
        {
            Mouse.Click(MouseButton.LEFT, GameService.Graphics.WindowWidth - 1);
        }
    }
}
