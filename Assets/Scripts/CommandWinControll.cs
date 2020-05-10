using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class CommandWinControll : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();


    public void OnButtonHide()
    {
        ShowWindow(GetActiveWindow(), 2);
    }

    
    public void OnButtonExit()
    {
        Application.Quit();
    }
}
