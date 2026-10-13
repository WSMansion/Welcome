from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/WorldCameraScaleCompensation.cs')
t = p.read_text(encoding='utf-8')
old = '''        [SerializeField, Min(0.05f), InspectorName("屏幕尺寸倍率"),
         Tooltip("以预制体原始尺寸为 1；调大后对象在镜头缩放时仍保持对应的屏幕尺寸。")]
        private float sizeMultiplier = 1f;'''
new = '''        [SerializeField, Min(0.05f), InspectorName("屏幕尺寸倍率"),
         Tooltip("以预制体原始尺寸为 1；调大后对象在镜头缩放时仍保持对应的屏幕尺寸。")]
        private float sizeMultiplier = 1f;

        [SerializeField, Range(0f, 1f), InspectorName("缩放响应"),
         Tooltip("1 表示完全抵消镜头缩放；0 表示完全跟着世界缩放。文字可调低，让尺寸变化更缓和。")]
        private float zoomResponse = 1f;'''
assert old in t
t = t.replace(old, new, 1)
old2 = '            transform.localScale = baseScale * Mathf.Max(0.05f, sizeMultiplier) * zoomCompensation;'
new2 = '''            float easedZoom = Mathf.Lerp(1f, zoomCompensation, Mathf.Clamp01(zoomResponse));
            transform.localScale = baseScale * Mathf.Max(0.05f, sizeMultiplier) * easedZoom;'''
assert old2 in t
t = t.replace(old2, new2, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
