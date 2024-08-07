﻿// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace EasySaveAsAdmin.PluginInfrastructure
{
    internal static class PluginBase
    {
        internal static NppData NppData;
        internal static readonly FuncItems FuncItems = new FuncItems();
        private static readonly List<string> UntranslatedFuncItemNames = new List<string>();
        public static List<string> GetUntranslatedFuncItemNames() => UntranslatedFuncItemNames.ToList();

        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), false);
        }

        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut)
        {
            SetCommand(index, commandName, functionPointer, shortcut, false);
        }

        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, bool checkOnInit)
        {
            SetCommand(index, commandName, functionPointer, new ShortcutKey(), checkOnInit);
        }

        internal static void SetCommand(int index, string commandName, NppFuncItemDelegate functionPointer, ShortcutKey shortcut, bool checkOnInit)
        {
            FuncItem funcItem = new FuncItem();
            funcItem._cmdID = index;
            funcItem._itemName = commandName;
            if (functionPointer != null)
                funcItem._pFunc = new NppFuncItemDelegate(functionPointer);
            
            if (shortcut._key != 0)
                funcItem._pShKey = shortcut;
            funcItem._init2Check = checkOnInit;
            UntranslatedFuncItemNames.Add(commandName);
            FuncItems.Add(funcItem);
        }

        /// <summary>
        /// if a menu item (for your plugin's drop-down menu) has a checkmark, check/uncheck it, and call its associated funcId.
        /// </summary>
        /// <param name="funcId">the index of the menu item of interest</param>
        /// <param name="isChecked">whether the menu item should be checked</param>
        public static void CheckMenuItem(int funcId, bool isChecked)
        {
            Win32.CheckMenuItem(
                Win32.GetMenu(NppData._nppHandle),
                FuncItems.Items[funcId]._cmdID,
                Win32.MF_BYCOMMAND | (isChecked ? Win32.MF_CHECKED : Win32.MF_UNCHECKED));
        }

        //private static IntPtr GetThisPluginMenuHandle()
        //{
        //    IntPtr mainMenuHandle = Win32.GetMenu(nppData._nppHandle);
        //    int pluginMenuIndex = 10;
        //    IntPtr allPluginsMenuHandle = Win32.GetSubMenu(mainMenuHandle, pluginMenuIndex);
        //    int itemCount = Win32.GetMenuItemCount(allPluginsMenuHandle);
        //    if (itemCount < 0)
        //        return IntPtr.Zero;
        //    for (int ii = 0; ii < itemCount; ii++)
        //    {
        //        var mii = new Win32.MenuItemInfo(Win32.MenuItemMask.MIIM_STRING | Win32.MenuItemMask.MIIM_SUBMENU);
        //        mii.cch = 
        //    }
        //}

        private static IntPtr MenuItemInfoToHGlobal(Win32.MenuItemInfo mii)
        {
            IntPtr miiPtr = Marshal.AllocHGlobal((int)mii.cbSize);
            Marshal.StructureToPtr(mii, miiPtr, false);
            return miiPtr;
        }

        private static bool SetMenuItemText(IntPtr hMenu, int index, string newText)
        {
            if (index >= FuncItems.Items.Count || index < 0 || newText is null)
                return false;
            var mii = new Win32.MenuItemInfo(Win32.MenuItemMask.MIIM_STRING | Win32.MenuItemMask.MIIM_FTYPE);
            IntPtr newTextPtr = Marshal.StringToHGlobalAnsi(newText);
            mii.dwTypeData = newTextPtr;
            IntPtr miiPtr = MenuItemInfoToHGlobal(mii);
            bool res = Win32.SetMenuItemInfo(hMenu, (uint)index, true, miiPtr);
            Marshal.FreeHGlobal(miiPtr);
            Marshal.FreeHGlobal(newTextPtr);
            return res;
        }

        private static unsafe bool TryGetMenuItemText(IntPtr hMenu, int index, out string str)
        {
            str = null;
            if (index < 0) // we assume the user has already checked the menu item count using Win32.GetMenuItemCount(hMenu)
                return false;
            var mii = new Win32.MenuItemInfo(Win32.MenuItemMask.MIIM_STRING | Win32.MenuItemMask.MIIM_STATE);
            IntPtr miiPtr = MenuItemInfoToHGlobal(mii);
            if (Win32.GetMenuItemInfo(hMenu, (uint)index, true, miiPtr))
            {
                mii = (Win32.MenuItemInfo)Marshal.PtrToStructure(miiPtr, typeof(Win32.MenuItemInfo));
                mii.cch++;
                byte[] textBuffer = new byte[mii.cch];
                fixed (byte * textPtr = textBuffer)
                {
                    IntPtr textPtrSafe = (IntPtr)textPtr;
                    mii.dwTypeData = textPtrSafe;
                    Marshal.StructureToPtr(mii, miiPtr, true);
                    Win32.GetMenuItemInfo(hMenu, (uint)index, true, miiPtr);
                    str = Marshal.PtrToStringAnsi(textPtrSafe);
                }
            }
            Marshal.FreeHGlobal(miiPtr);
            return !(str is null);
        }

        private static unsafe bool TryGetSubMenuWithName(IntPtr hMenu, string subMenuName, out IntPtr hSubMenu, out int idSubMenu)
        {
            idSubMenu = -1;
            hSubMenu = IntPtr.Zero;
            int itemCount = Win32.GetMenuItemCount(hMenu);
            if (itemCount < 0)
                return false;
            for (int ii = 0; ii < itemCount; ii++)
            {
                if (!TryGetMenuItemText(hMenu, ii, out string menuItemName))
                    return false;
                if (menuItemName == subMenuName)
                {
                    idSubMenu = ii;
                    hSubMenu = Win32.GetSubMenu(hMenu, ii);
                    return true;
                }
            }
            return false;
        }

        private static IntPtr _allPluginsMenuHandle = IntPtr.Zero;
        private static int _thisPluginIdxInAllPluginsMenu = -1;
        private static IntPtr _thisPluginMenuHandle = IntPtr.Zero;

        /// <summary>
        /// if we can get a valid handle to this plugin's drop-down menu, set thisPluginMenuHandle to that handle and return true.<br></br>
        /// Else return false and set thisPluginMenuHandle to IntPtr.Zero.
        /// </summary>
        /// <param name="pluginMenuName">the name of the plugins submenu of the Notepad++ main menu. Normally this is "&amp;Plugins;", but it will vary depending on the Notepad++ UI language</param>
        /// <param name="thisPluginMenuHandle"></param>
        /// <returns></returns>
        private static unsafe bool TryGetThisPluginMenu(string pluginMenuName, out IntPtr thisPluginMenuHandle)
        {
            thisPluginMenuHandle = IntPtr.Zero;
            if (_thisPluginMenuHandle != IntPtr.Zero && _allPluginsMenuHandle != IntPtr.Zero && _thisPluginIdxInAllPluginsMenu >= 0
                && TryGetMenuItemText(_allPluginsMenuHandle, _thisPluginIdxInAllPluginsMenu, out string pluginName)
                && pluginName == EasySaveAsAdminMain.PluginName)
            {
                thisPluginMenuHandle = _thisPluginMenuHandle;
                return true;
            }
            if (!TryGetSubMenuWithName(Win32.GetMenu(NppData._nppHandle), pluginMenuName, out _allPluginsMenuHandle, out _))
                return false;
            if (!TryGetSubMenuWithName(_allPluginsMenuHandle, EasySaveAsAdminMain.PluginName, out _thisPluginMenuHandle, out _thisPluginIdxInAllPluginsMenu))
                return false;
            thisPluginMenuHandle = _thisPluginMenuHandle;
            return true;
        }

        public static bool TryGetThisPluginMenuItemText(string pluginMenuName, int index, out string text)
        {
            if (!TryGetThisPluginMenu(pluginMenuName, out IntPtr hMenu))
            {
                text = null;
                return false;
            }
            return TryGetMenuItemText(hMenu, index, out text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginMenuName">the name of the menu for all plugins. This is normally "&amp;Plugins", but will vary depending on Notepad++ UI language</param>
        /// <param name="index"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        public static bool SetThisPluginMenuItemText(string pluginMenuName, int index, string newText)
        {
            if (!TryGetThisPluginMenu(pluginMenuName, out IntPtr hMenu))
                return false;
            return SetMenuItemText(hMenu, index, newText);
        }

        public static bool ChangePluginMenuItemNames(string pluginMenuName, List<string> newNames)
        {
            if (newNames.Count != FuncItems.Items.Count || !TryGetThisPluginMenu(pluginMenuName, out IntPtr hMenu))
                return false;
            for (int ii = 0; ii < newNames.Count; ii++)
            {
                string newName = newNames[ii];
                if (newName != "---" && !SetMenuItemText(hMenu, ii, newName))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// if a menu item (for your plugin's drop-down menu) has a checkmark:<br></br>
        /// - if it's checked, uncheck it<br></br>
        /// - if it's unchecked, check it.
        /// Either way, call its associated funcId.
        /// </summary>
        /// <param name="funcId">the index of the menu item of interest</param>
        /// <param name="isChecked">whether the menu item is currently checked</param>
        internal static void CheckMenuItemToggle(int funcId, ref bool isCurrentlyChecked)
        {
            // toggle value
            isCurrentlyChecked = !isCurrentlyChecked;
            CheckMenuItem(funcId, isCurrentlyChecked);
        }

        internal static IntPtr GetCurrentScintilla()
        {
            int curScintilla;
            Win32.SendMessage(NppData._nppHandle, (uint) NppMsg.NPPM_GETCURRENTSCINTILLA, 0, out curScintilla);
            return (curScintilla == 0) ? NppData._scintillaMainHandle : NppData._scintillaSecondHandle;
        }


        static readonly Func<IScintillaGateway> gatewayFactory = () => new ScintillaGateway(GetCurrentScintilla());

        public static Func<IScintillaGateway> GetGatewayFactory()
        {
            return gatewayFactory;
        }
    }
}
