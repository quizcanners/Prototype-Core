using QuizCanners.Inspect;
using QuizCanners.Lerp;
using QuizCanners.RayTracing;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class C_SmokeEffectOnImpact : MonoBehaviour, IPEGI
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private C_RayRendering_DynamicPrimitive _primitive;

        private readonly LinkedLerp.ShaderFloat _visibility = new("_Visibility", 1);
        private readonly LinkedLerp.ShaderColor _color = new("_Color", Color.white, maxSpeed: 10);

        private MaterialPropertyBlock block;
        private LogicWrappers.Request _propertyBlockDirty = new LogicWrappers.Request();
        private bool _animating = false;
        private float smokeDensity;

        float _visibilityValue;

        public float Visibility 
        {
            get => _visibilityValue;  
            set 
            {
                _visibilityValue = value;
                CheckBlock();
                _visibility.CurrentValue = Mathf.SmoothStep(0,1, value);
                _visibility.Property.SetLatestValueOn(block);
                _propertyBlockDirty.CreateRequest();
            }
        }

        public Color Color
        {
            get => _color.CurrentValue;
            set
            {
                CheckBlock();
                _color.CurrentValue = value;
                _color.Property.SetLatestValueOn(block);
                _propertyBlockDirty.CreateRequest();
            }
        }

        public float Size 
        {
            get => transform.localScale.x;
            set => transform.localScale = Vector3.one * value;
        }

        public void TryConsume(C_SmokeEffectOnImpact other) 
        {
            var dist = (other.transform.position - transform.position).magnitude;
            float sizes = Size + other.Size;

            if (sizes*0.5 > dist) 
            {
                if (Size > other.Size)
                {
                    if (Visibility > 0.5f)
                       ConsumeToSelf(other);
                }
                else if (other.Visibility>0.5f)
                    other.ConsumeToSelf(this);
            }
        }

        private void ConsumeToSelf(C_SmokeEffectOnImpact other) 
        {
            smokeDensity += other.smokeDensity * 0.8f;
            other.smokeDensity *= 0.2f;
        }

        public void PlayAnimateFromDot(float density = 0.02f) 
        {
            Size = 0.3f;
            Visibility = 1;
            smokeDensity += density;
            _color.CurrentValue = new Color(0.5f, 0.4f, 0.3f);
            _animating = true;
            _primitive.gameObject.SetActive(density > 1f);
        }

        internal void Refresh()
        {
            smokeDensity = 0;
            PlayAnimateFromDot();
            CheckBlock();
            _primitive.gameObject.SetActive(false);
            _primitive.transform.localScale = Vector3.zero;
            meshRenderer.SetPropertyBlock(block);
        }

        private void PlayFromBigCloud(float size)
        {
            Size = size;
            smokeDensity += size;
            Visibility = 0;
            _color.CurrentValue = Color.white;
            _animating = true;
        }
    
        private void CheckBlock() 
        {
            if (block == null)
                block = new MaterialPropertyBlock();
        }

        void LateUpdate()
        {
            if (meshRenderer && _propertyBlockDirty.TryUseRequest()) 
            {
                CheckBlock();
                meshRenderer.SetPropertyBlock(block);
            }

            if (_animating)
            {
                Size += Time.deltaTime  * (1 + Mathf.Pow(smokeDensity, 1.4f)) * 5;
                float deSize = 1f / Size;
                float targetVisibility = Mathf.Clamp(smokeDensity / (Size * Size) , 0, max: deSize);
                bool isFading = targetVisibility < Visibility;
                Visibility = LerpUtils.LerpBySpeed(
                    from: Visibility, 
                    to: Mathf.Clamp01(targetVisibility), 
                    speed: (1 + Size* Visibility + Mathf.Abs(targetVisibility - Visibility)) / (isFading ? (1 + smokeDensity) : Size),
                    unscaledTime: false);

                Color = LerpUtils.LerpBySpeed(Color, Color.white, 1, unscaledTime: false);
                float fadeSpeed = Size * Size * Visibility * 0.1f * (1 + Singleton.TryGetValue<Pool_SmokeEffects, float>(s=> s.InstancesCount, 10));
                smokeDensity = Mathf.Max(0, smokeDensity - fadeSpeed * Time.deltaTime);

                if (_primitive.gameObject.activeSelf)
                {
                    _primitive.transform.localScale = Size * new Vector3(1f, 0.25f, 1f);
                    _primitive.Color = new Color(Visibility, Visibility, Visibility * Visibility);

                    if (Visibility < 0.1f)
                        _primitive.gameObject.SetActive(false);
                }

                if (smokeDensity < 0.01f && Visibility < 0.01f) 
                {
                    Pool.Return(this);
                    _animating = false;
                }
            }
        }

        #region Inspector

        public void Inspect()
        {
            var changed = pegi.ChangeTrackStart();

            pegi.Nl();

            "Mesh Rederer".PegiLabel(90).Edit_IfNull(ref meshRenderer, gameObject).Nl();

            var vis = Visibility;
            "Visibility".PegiLabel(width: 60).Edit_01(ref vis).Nl().OnChanged(()=> Visibility = vis);

            var size = Size;
            "Size".PegiLabel(width: 50).Edit(ref size, 0.01f, 5f).Nl().OnChanged(()=> Size = size);

            var col = Color;
            "Color".PegiLabel(width: 50).Edit(ref col).Nl().OnChanged(() => Color = col);

            if ((_animating ? Icon.Pause : Icon.Play).Click())
                _animating = !_animating;
            "Density".PegiLabel(60).Edit(ref smokeDensity).Nl();

            "Animate".PegiLabel().Click(()=> PlayAnimateFromDot(0.5f));

            "Big Cloud".PegiLabel().Click().Nl().OnChanged(()=> PlayFromBigCloud(10));

            if (changed)
                _propertyBlockDirty.CreateRequest();
        }

        #endregion
    }

    [PEGI_Inspector_Override(typeof(C_SmokeEffectOnImpact))] internal class C_SmokeEffectOnImpactDrawer : PEGI_Inspector_Override { }
}
