﻿using System;
using System.Runtime.InteropServices;

namespace Doubts.WebFramework.Internal.Imports
{
    public class UxTheme
    {
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, String pszSubAppName, String pszSubIdList);
    }
    public class User32
    {
        public static float GetOriginalDeviceScaleFactor(IntPtr hWnd)
        {
            var hMonitor = MonitorFromWindow(hWnd, (uint)MonitorFromWindowFlags.MONITOR_DEFAULTTONEAREST);

            if ((System.Environment.OSVersion.Version.Major >= 8 && System.Environment.OSVersion.Version.Minor >= 1) || System.Environment.OSVersion.Version.Major > 8)
            {
                int x;
                int y;

                GetDpiForMonitor(hMonitor, MonitorDpiType.MDT_DEFAULT, out x, out y);

                return x / 96f;
            }
            else
            {
                return 1.0f;
            }


        }
        [DllImport("Shcore.dll")]
        public static extern int GetDpiForMonitor(IntPtr hMonitor, MonitorDpiType dpiType, out int dpiX, out int dpiY);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

        //[DllImport("Shcore.dll")]
        //public static extern int GetScaleFactorForMonitor(IntPtr hMonitor, ref DeviceScaleFactor pScale);

        //[DllImport("Shcore.dll")]
        //public static extern int GetScaleFactorForMonitor(IntPtr hMonitor, ref int pScale);

        [DllImport("user32.dll")]
        public static extern int FillRect(IntPtr hDC, [In] ref RECT lprc, IntPtr hbr);


        [DllImport("user32.dll")]
        public static extern bool InflateRect(ref RECT lprc, int dx, int dy);
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetricFlags smIndex);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDCEx(IntPtr hwnd, IntPtr hrgnclip, uint fdwOptions);

        [DllImport("user32.dll")]
        public static extern void DisableProcessWindowsGhosting();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        public static void InvalidateWindow(IntPtr hWnd)
        {
            RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, (int)(RedrawWindowFlags.RDW_FRAME | RedrawWindowFlags.RDW_UPDATENOW | RedrawWindowFlags.RDW_INVALIDATE | RedrawWindowFlags.RDW_ERASE));
        }


        public static void SendFrameChanged(IntPtr hWnd)
        {
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOCOPYBITS |
                SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOREPOSITION |
                SetWindowPosFlags.SWP_NOSENDCHANGING | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public extern static int OffsetRect(ref RECT lpRect, int x, int y);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public extern static bool SetWindowPos(int hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int x, int y, int width, int height, SetWindowPosFlags flags);

        /// <summary>
        /// ShowWindow function of USER32
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(int hWnd, int nCmdShow);

        /// <summary>
        /// ShowWindow function of USER32
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// ShowWindow function of USER32
        /// </summary>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, short cmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int CloseWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int SetParent(int hWndChild, int hWndParent);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt16 RegisterClassW([In] ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowExW(
            UInt32 dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            UInt32 dwStyle,
            Int32 x,
            Int32 y,
            Int32 nWidth,
            Int32 nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
            );

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetWindowLong(IntPtr hWnd, GetWindowLongFlags nIndex);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowLong(IntPtr hWnd, GetWindowLongFlags nIndex, uint newLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, uint cursor);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetCursorPos(ref POINT lpPoint);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, ShowWindowStyles cmdShow);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetSystemMenu(IntPtr windowHandle, bool bReset);

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        public static uint HiWord(IntPtr ptr)
        {
            if (((uint)ptr & 0x80000000) == 0x80000000)
            {
                return ((uint)ptr >> 16);
            }

            return ((uint)ptr >> 16) & 0xffff;
        }

        /// <summary>
        /// Returns the LOW word from an IntPtr
        /// </summary>
        /// <param name="ptr">IntPtr</param>
        /// <returns>LOW Word uint</returns>
        public static uint LoWord(IntPtr ptr)
        {
            return (uint)(ptr.ToInt32() & 0xFFFF);
        }
    }

    public class Gdi32
    {
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(PenStyle fnPenStyle, int nWidth, uint crColor);

        [DllImport("gdi32.dll")]
        public static extern uint SetBkColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll")]
        internal extern static int ExcludeClipRect(IntPtr hdc, int x1, int y1, int x2, int y2);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush([In] uint color);

        [DllImport("gdi32.dll")]
        public static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject", SetLastError = true)]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);
    }
}
