from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/WorldCameraScaleCompensation.cs')
t = p.read_text(encoding='utf-8')
old = '''        private Vector3 baseScale;
        private float referenceOrthographicSize;
        private bool hasBaseScale;
        private bool hasReferenceSize;'''
new = '''        private static float sharedReferenceOrthographicSize;

        private Vector3 baseScale;
        private bool hasBaseScale;'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''        private void Awake()
        {'''
new2 = '''        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSharedReference()
        {
            sharedReferenceOrthographicSize = 0f;
        }

        private void Awake()
        {'''
assert old2 in t
t = t.replace(old2, new2, 1)
t = t.replace('if (targetCamera != null && targetCamera.orthographic && referenceOrthographicSize > 0f)\n                zoomCompensation = targetCamera.orthographicSize / referenceOrthographicSize;', 'if (targetCamera != null && targetCamera.orthographic && sharedReferenceOrthographicSize > 0f)\n                zoomCompensation = targetCamera.orthographicSize / sharedReferenceOrthographicSize;')
old3 = '''            if (!hasReferenceSize && targetCamera != null && targetCamera.orthographic)
            {
                referenceOrthographicSize = targetCamera.orthographicSize;
                hasReferenceSize = referenceOrthographicSize > 0f;
            }'''
new3 = '''            if (sharedReferenceOrthographicSize <= 0f && targetCamera != null && targetCamera.orthographic)
                sharedReferenceOrthographicSize = targetCamera.orthographicSize;'''
assert old3 in t, 'cache missing'
t = t.replace(old3, new3, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
