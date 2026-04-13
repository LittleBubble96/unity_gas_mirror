using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using VSEngine.GAS;

namespace GAS.Editor.AbilityLogic
{
    [CustomTimelineEditor(typeof(AbilityLogicMarker))]
    public class AbilityLogicMarkerEditor : MarkerEditor
    {
        const float k_OverlayAlpha = 0.5f;

        public override void DrawOverlay(IMarker marker, MarkerUIStates uiState, MarkerOverlayRegion region)
        {
            base.DrawOverlay(marker, uiState, region);
            if (marker is AbilityLogicMarker abilityLogicMarker)
            {
                if (abilityLogicMarker.LogicType.HasFlag(ClipLogicType.CommitAbility))
                {
                    //在标记上显示提交技能的图标
                    Rect iconRect = new Rect(region.markerRegion.x - 4, region.timelineRegion.y, 16, 16);
                    //换个颜色Color.cyan
                    GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent("SignalAsset Icon").image, ScaleMode.ScaleToFit, true, 0, Color.cyan, 0, 0);
                    DrawLineOverlay(Color.cyan, region);
                }
                if (abilityLogicMarker.LogicType.HasFlag(ClipLogicType.EndAbility))
                {
                    //在标记上显示结束技能的图标
                    Rect iconRect = new Rect(region.markerRegion.x - 4, region.timelineRegion.y, 16, 16);
                    //换个颜色Color.red
                    GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent("SignalAsset Icon").image, ScaleMode.ScaleToFit, true, 0, Color.red,0,0);
                    DrawLineOverlay(Color.red, region);
                }
            }
            
        }
        
        static void DrawLineOverlay(Color color, MarkerOverlayRegion region)
        {
            // Calculate a rectangle that uses the full timeline region's height and marker width
            Rect overlayLineRect = new Rect(region.markerRegion.x + 2f,
                region.markerRegion.y,
                5,
                region.timelineRegion.height);

            // Set the color with an extra alpha value adjustment, then draw the rectangle
            Color overlayLineColor = new Color(color.r, color.g, color.b, color.a * k_OverlayAlpha);
            EditorGUI.DrawRect(overlayLineRect, overlayLineColor);
        }
    }
}