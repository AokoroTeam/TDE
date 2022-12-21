using Michsky.UI.ModernUIPack;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using static Michsky.UI.ModernUIPack.WindowManager;

namespace Aokoro.UI
{
    [ExecuteInEditMode, DefaultExecutionOrder(-90)]
    public class WindowManager : UIManager
    {
        public Transform WindowsParent;
        [SerializeField, Dropdown(nameof(WindowsNames))]
        private string defaultWindow;
        public string DefaultWindow => defaultWindow;

        [SerializeField, ReadOnly]
        private Michsky.UI.ModernUIPack.WindowManager baseWindowManager;

        private List<string> WindowsNames() => baseWindowManager != null ? 
            baseWindowManager.windows.Select(ctx => ctx.windowName).ToList() :
            new List<string>() { "No WindowManager" };


        private void OnValidate()
        {
            baseWindowManager = GetComponent<Michsky.UI.ModernUIPack.WindowManager>();
        }

        protected override void Awake()
        {
            OnValidate();
            
            base.Awake();
        }

        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(DefaultWindow))
                OpenWindow(defaultWindow);
        }

        public void OpenWindow(string windowName) => baseWindowManager.OpenWindow(windowName);
        public void ShowCurrentWindow() => baseWindowManager.ShowCurrentWindow();
        public void HideCurrentWindow() => baseWindowManager.HideCurrentWindow();
       
        
        public WindowItem CurrentWindow() => baseWindowManager.windows[baseWindowManager.currentWindowIndex];
        public WindowItem AddWindow(string windowName, GameObject windowObject)
        {
            WindowItem window = new WindowItem();

            window.windowName = windowName;
            window.windowObject = windowObject;

            baseWindowManager.windows.Add(window);

            return window;
        }
        public WindowItem GetWindow() => GetWindow(DefaultWindow);
        public WindowItem GetWindow(string windowName)
        {
            int index = baseWindowManager.windows.FindIndex(ctx => ctx.windowName == windowName);
            return index == -1 ? null : baseWindowManager.windows[index];
        }
    }
}