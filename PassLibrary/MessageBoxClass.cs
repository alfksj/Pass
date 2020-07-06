using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public class MessageBoxClass
{
    delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int hook, HookProc callback, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr GetDlgItem(IntPtr hDlg, DialogResult nIDDlgItem);

    [DllImport("user32.dll")]
    static extern bool SetDlgItemText(IntPtr hDlg, DialogResult nIDDlgItem, string lpString);

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    static IntPtr g_hHook;

    static string yes;
    static string cancel;
    static string no;



    /// <summary>
    /// 메시지 박스를 띄웁니다.
    /// </summary>
    /// <param name="text">텍스트 입니다.</param>
    /// <param name="caption">캡션 입니다.</param>
    /// <param name="yes">예 문자열 입니다.</param>
    /// <param name="no">아니오 문자열 입니다.</param>
    /// <param name="cancel">취소 문자열 입니다.</param>
    /// <returns></returns>
    public static DialogResult Show(string text, string caption, string yes, string no, string cancel)
    {
        MessageBoxClass.yes = yes;
        MessageBoxClass.cancel = cancel;
        MessageBoxClass.no = no;
        g_hHook = SetWindowsHookEx(5, new HookProc(HookWndProc), IntPtr.Zero, GetCurrentThreadId());
        return MessageBox.Show(text, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.None);
    }
    public static DialogResult Show(string text, string caption, string yes, string no)
    {
        MessageBoxClass.yes = yes;
        MessageBoxClass.no = no;
        g_hHook = SetWindowsHookEx(5, new HookProc(HookWndProc), IntPtr.Zero, GetCurrentThreadId());
        return MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.None);
    }

    static int HookWndProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        IntPtr hChildWnd;

        if (nCode == 5)
        {
            hChildWnd = wParam;

            if (GetDlgItem(hChildWnd, DialogResult.Yes) != null)
                SetDlgItemText(hChildWnd, DialogResult.Yes, yes);

            if (GetDlgItem(hChildWnd, DialogResult.No) != null)
                SetDlgItemText(hChildWnd, DialogResult.No, no);

            if (GetDlgItem(hChildWnd, DialogResult.Cancel) != null)
                SetDlgItemText(hChildWnd, DialogResult.Cancel, cancel);

            UnhookWindowsHookEx(g_hHook);
        }
        else
            CallNextHookEx(g_hHook, nCode, wParam, lParam);

        return 0;
    }
}