from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/WorldCameraScaleCompensation.cs')
t = p.read_text(encoding='utf-8')
old = '''        private Vector3 baseScale;
        private float referenceOrthographicSize;
        private bool hasBaseScale;'''
new = '''        private Vector3 baseScale;
        private float referenceOrthographicSize;
        private bool hasBaseScale;
        private bool hasReferenceSize;'''
assert old in t
t = t.replace(old, new, 1)
old2 = '''            if (targetCamera != null && targetCamera.orthographic)
            {
                if (referenceOrthographicSize <= 0f)
                    referenceOrthographicSize = targetCamera.orthographicSize;

                if (referenceOrthographicSize > 0f)
                    zoomCompensation = targetCamera.orthographicSize / referenceOrthographicSize;
            }'''
new2 = '''            if (targetCamera != null && targetCamera.orthographic && referenceOrthographicSize > 0f)
                zoomCompensation = targetCamera.orthographicSize / referenceOrthographicSize;'''
assert old2 in t
t = t.replace(old2, new2, 1)
old3 = '''            if (targetCamera != null && targetCamera.orthographic && referenceOrthographicSize <= 0f)
                referenceOrthographicSize = targetCamera.orthographicSize;'''
new3 = '''            if (!hasReferenceSize && targetCamera != null && targetCamera.orthographic)
            {
                referenceOrthographicSize = targetCamera.orthographicSize;
                hasReferenceSize = referenceOrthographicSize > 0f;
            }'''
assert old3 in t
t = t.replace(old3, new3, 1)
p.write_text(t, encoding='utf-8', newline='\n')
print('updated')
