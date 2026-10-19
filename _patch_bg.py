from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/WorldInteractionPrompt.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('[RequireComponent(typeof(SpriteRenderer), typeof(WorldCameraScaleCompensation))]', '[RequireComponent(typeof(WorldCameraScaleCompensation))]')
t = t.replace('''            promptRenderer = GetComponent<SpriteRenderer>();
            cameraScaleCompensation = GetComponent<WorldCameraScaleCompensation>();''', '''            Transform background = transform.Find("bg");
            promptRenderer = background != null ? background.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>();
            cameraScaleCompensation = GetComponent<WorldCameraScaleCompensation>();''')
p.write_text(t, encoding='utf-8', newline='\n')
print('script updated')
