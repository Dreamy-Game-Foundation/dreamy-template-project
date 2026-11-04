using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class UIShineWave : MonoBehaviour
    {
        private const string ShaderName = "Dreamy/UI/Shine Wave";
        private static readonly int ShineColorId = Shader.PropertyToID("_ShineColor");
        private static readonly int ShinePositionId = Shader.PropertyToID("_ShinePosition");
        private static readonly int ShineWidthId = Shader.PropertyToID("_ShineWidth");
        private static readonly int ShineSoftnessId = Shader.PropertyToID("_ShineSoftness");
        private static readonly int ShineDirectionId = Shader.PropertyToID("_ShineDirection");
        private static readonly int ShineAspectId = Shader.PropertyToID("_ShineAspect");
        private static readonly int ShineUvRectId = Shader.PropertyToID("_ShineUvRect");

        [SerializeField] private Graphic target;
        [SerializeField] private Shader shineShader;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField, Min(0f)] private float speed = 0.6f;
        [SerializeField, Range(0f, 1f)] private float phase;
        [SerializeField, Range(-180f, 180f)] private float rotation = 45f;
        [SerializeField] private Color shineColor = new Color(1f, 1f, 1f, 0.7f);
        [SerializeField, Range(0.01f, 1f)] private float width = 0.18f;
        [SerializeField, Range(0.001f, 1f)] private float softness = 0.12f;

        private Material originalMaterial;
        private Material runtimeMaterial;
        private bool isPlaying;
        private bool isOneShot;
        private float oneShotSpeed;
        private IShineWaveRegistry registry;
        private Sprite cachedSprite;
        private Vector4 spriteUvRect = new Vector4(0f, 0f, 1f, 1f);

        public bool IsPlaying => isPlaying;

        private void Reset()
        {
            target = GetComponent<Graphic>();
            shineShader = Shader.Find(ShaderName);
        }

        private void OnEnable()
        {
            registry = FindRegistry();
            if (registry != null)
            {
                registry.Register(this);
                isPlaying = false;
                phase = 1f;
            }
            else
            {
                isPlaying = playOnEnable;
            }

            EnsureMaterial();
            ApplyProperties();
        }

        private void OnDisable()
        {
            registry?.Unregister(this);
            registry = null;
            ReleaseMaterial();
        }

        private void OnDestroy()
        {
            ReleaseMaterial();
        }

        private void OnValidate()
        {
            speed = Mathf.Max(0f, speed);
            phase = Mathf.Clamp01(phase);
            if (isActiveAndEnabled)
            {
                EnsureMaterial();
                ApplyProperties();
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyProperties();
        }

        private void Update()
        {
            if (!isPlaying || runtimeMaterial == null) return;

            float phaseSpeed = isOneShot ? oneShotSpeed : speed;
            phase += phaseSpeed * Time.unscaledDeltaTime;
            if (phase >= 1f)
            {
                if (isOneShot)
                {
                    phase = 1f;
                    isPlaying = false;
                    isOneShot = false;
                }
                else
                {
                    phase = Mathf.Repeat(phase, 1f);
                }
            }

            ApplyProperties();
        }

        public void Play()
        {
            isOneShot = false;
            isPlaying = true;
        }

        public void Stop()
        {
            isPlaying = false;
            isOneShot = false;
        }

        public void SetPhase(float value)
        {
            phase = Mathf.Clamp01(value);
            ApplyProperties();
        }

        public void SetRotation(float value)
        {
            rotation = Mathf.Repeat(value + 180f, 360f) - 180f;
            ApplyProperties();
        }

        public void PlayWave(float duration, float waveRotation)
        {
            SetRotation(waveRotation);
            phase = 0f;
            oneShotSpeed = 1f / Mathf.Max(0.01f, duration);
            isOneShot = true;
            isPlaying = true;
            ApplyProperties();
        }

        private void EnsureMaterial()
        {
            if (runtimeMaterial != null) return;

            if (target == null)
            {
                target = GetComponent<Graphic>();
            }

            Shader shader = shineShader != null ? shineShader : Shader.Find(ShaderName);
            if (target == null || shader == null) return;

            originalMaterial = target.material;
            runtimeMaterial = new Material(shader)
            {
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
            };
            if (originalMaterial != null)
            {
                runtimeMaterial.CopyPropertiesFromMaterial(originalMaterial);
            }

            SynchronizeGraphicTexture();
            target.material = runtimeMaterial;
        }

        private void ApplyProperties()
        {
            if (runtimeMaterial == null) return;

            float aspect = GetAspectRatio();
            float sweepExtent = Mathf.Sqrt(aspect * aspect + 1f) * 0.5f + width + softness;
            SynchronizeGraphicTexture();
            float radians = rotation * Mathf.Deg2Rad;
            Vector4 direction = new Vector4(
                Mathf.Cos(radians),
                Mathf.Sin(radians),
                0f,
                0f);
            float position = Mathf.Lerp(-sweepExtent, sweepExtent, phase);
            ConfigureMaterial(runtimeMaterial, aspect, position, direction);

            if (target == null) return;

            Material renderingMaterial = target.materialForRendering;
            if (renderingMaterial != null && renderingMaterial != runtimeMaterial)
            {
                ConfigureMaterial(renderingMaterial, aspect, position, direction);
            }
        }

        private void ConfigureMaterial(
            Material material,
            float aspect,
            float position,
            Vector4 direction)
        {
            material.SetColor(ShineColorId, shineColor);
            material.SetFloat(ShinePositionId, position);
            material.SetFloat(ShineAspectId, aspect);
            material.SetVector(ShineUvRectId, spriteUvRect);
            material.SetFloat(ShineWidthId, width);
            material.SetFloat(ShineSoftnessId, softness);
            material.SetVector(ShineDirectionId, direction);
        }

        private IShineWaveRegistry FindRegistry()
        {
            foreach (MonoBehaviour behaviour in GetComponentsInParent<MonoBehaviour>(true))
            {
                if (behaviour is IShineWaveRegistry value)
                {
                    return value;
                }
            }

            return null;
        }

        private float GetAspectRatio()
        {
            RectTransform rect = target != null ? target.rectTransform : null;
            if (rect == null || Mathf.Approximately(rect.rect.height, 0f)) return 1f;

            return Mathf.Max(0.01f, rect.rect.width / rect.rect.height);
        }

        private void SynchronizeGraphicTexture()
        {
            if (target == null || runtimeMaterial == null) return;

            Texture texture = target.mainTexture;
            if (runtimeMaterial.mainTexture != texture)
            {
                runtimeMaterial.mainTexture = texture;
            }

            Image image = target as Image;
            Sprite sprite = image != null
                ? image.overrideSprite != null ? image.overrideSprite : image.sprite
                : null;
            if (cachedSprite == sprite) return;

            cachedSprite = sprite;
            spriteUvRect = GetUvRect(sprite);
        }

        private static Vector4 GetUvRect(Sprite sprite)
        {
            if (sprite == null) return new Vector4(0f, 0f, 1f, 1f);

            Vector2[] uv = sprite.uv;
            if (uv == null || uv.Length == 0) return new Vector4(0f, 0f, 1f, 1f);

            Vector2 min = uv[0];
            Vector2 max = uv[0];
            foreach (Vector2 point in uv)
            {
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return new Vector4(
                min.x,
                min.y,
                Mathf.Max(0.0001f, max.x - min.x),
                Mathf.Max(0.0001f, max.y - min.y));
        }

        private void ReleaseMaterial()
        {
            if (runtimeMaterial == null) return;

            if (target != null && target.material == runtimeMaterial)
            {
                target.material = originalMaterial;
            }

            if (Application.isPlaying)
            {
                Destroy(runtimeMaterial);
            }
            else
            {
                DestroyImmediate(runtimeMaterial);
            }

            runtimeMaterial = null;
            originalMaterial = null;
        }
    }
}
