using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Aokoro.UI;

namespace Aokoro.UI
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasSafeArea : UIItem
    {
        public RectTransform SafeAreaRect;

        private Rect lastSafeArea = Rect.zero;
        
        void Start()
        {
            lastSafeArea = Screen.safeArea;
            ApplySafeArea();
        }

        void ApplySafeArea()
        {
            if (SafeAreaRect == null)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= canvas.pixelRect.width;
            anchorMin.y /= canvas.pixelRect.height;
            anchorMax.x /= canvas.pixelRect.width;
            anchorMax.y /= canvas.pixelRect.height;

            SafeAreaRect.anchorMin = anchorMin;
            SafeAreaRect.anchorMax = anchorMax;
        }

        protected override void OnUpdate()
        {
            if (lastSafeArea != Screen.safeArea)
            {
                lastSafeArea = Screen.safeArea;
                ApplySafeArea();
            }
        }
    }
}
