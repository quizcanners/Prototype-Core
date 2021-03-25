using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;
using QuizCanners.Lerp;


namespace QuizCanners.IsItGame
{

    public partial class SpecialEffectShadersService
    {
        [Serializable]
        public class EffectsMousePositionManager : IPEGI, IPEGI_ListInspect
        {
            [SerializeField] private bool _enabled = true;

            [NonSerialized] protected float mouseDownStrengthOneDirectional;
            [NonSerialized] protected float mouseDownStrength = 0.1f;
            [NonSerialized] protected bool downClickFullyShown = true;
            [NonSerialized] protected Vector2 mouseDownPosition;

            protected readonly ShaderProperty.Feature UseMousePosition = new ShaderProperty.Feature("_qcPp_FEED_MOUSE_POSITION");
            protected readonly ShaderProperty.VectorValue mousePosition = new ShaderProperty.VectorValue("_qcPp_MousePosition");
            protected readonly ShaderProperty.VectorValue mouseDynamics = new ShaderProperty.VectorValue("_qcPp_MouseDynamics");

            public bool Enabled 
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                    UseMousePosition.Enabled = value;
                }
            }

            public void ManagedOnEnable() => Enabled = _enabled;

            public void ManagedLateUpdate()
            {
                if (!_enabled)
                    return;
                
                bool down = Input.GetMouseButton(0);

                if (down || mouseDownStrength > 0)
                {
                    bool downThisFrame = Input.GetMouseButtonDown(0);

                    if (downThisFrame)
                    {
                        //mouseDownStrength = 0;
                        //mouseDownStrengthOneDirectional = 0;
                        downClickFullyShown = false;
                    }

                    mouseDownStrengthOneDirectional = LerpUtils.LerpBySpeed(mouseDownStrengthOneDirectional,
                        down ? 0 : 1,
                        down ? 4f : (3f - mouseDownStrengthOneDirectional * 3f));

                    mouseDownStrength = LerpUtils.LerpBySpeed(mouseDownStrength,
                        downClickFullyShown ? 0 :
                        (down ? 0.9f : 1f),
                        (down) ? 2 : (downClickFullyShown ? 0.75f : 2.5f));

                    if (mouseDownStrength > 0.99f)
                        downClickFullyShown = true;

                    if (down)
                        mouseDownPosition = Input.mousePosition.XY() / new Vector2(Screen.width, Screen.height);

                    mousePosition.GlobalValue = mouseDownPosition.ToVector4(mouseDownStrength, ((float)Screen.width) / Screen.height);
                    mouseDynamics.GlobalValue = new Vector4(mouseDownStrengthOneDirectional, 0, 0, 0);
                }
                
            }

            #region Inspector
            public void Inspect()
            {
                if ("Mouse Position to shader".toggleIcon(ref _enabled, hideTextWhenTrue: true).nl())
                    Enabled = _enabled;

                if (_enabled)
                {
                    "Define:".write_ForCopy(width: 90, UseMousePosition.ToString()).nl();
                    "float4".write_ForCopy(width: 90, mousePosition.ToString()).nl();

                    (mousePosition.ToString() + ".z - mouseDownStrength").nl();
                    (mousePosition.ToString() + ".w - width / height").nl();

                    "float4".write_ForCopy(width: 90, mouseDynamics.ToString()).nl();
                    (mouseDynamics.ToString() + ".x - one directional strength").nl();
                }
            }

            public void InspectInList(ref int edited, int index)
            {
                if (pegi.toggleIcon(ref _enabled))
                    Enabled = _enabled;

                if ("Mouse Position".ClickLabel() || icon.Enter.Click())
                    edited = index;
            }
            #endregion
        }
    }
}