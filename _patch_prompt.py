from pathlib import Path
p = Path(r'WelCome/Assets/Scripts/World/WorldInteractionPrompt.cs')
t = p.read_text(encoding='utf-8')
t = t.replace('        private SpriteRenderer promptRenderer;\n        private WorldCameraScaleCompensation cameraScaleCompensation;\n', '''        [SerializeField, InspectorName("按键文字")] private TextMesh keyLabel;\n        private SpriteRenderer promptRenderer;\n        private MeshRenderer keyRenderer;\n        private WorldCameraScaleCompensation cameraScaleCompensation;\n''')
t = t.replace('''            promptRenderer = GetComponent<SpriteRenderer>();
            cameraScaleCompensation = GetComponent<WorldCameraScaleCompensation>();
            promptRenderer.sortingOrder = PromptSortingOrder;
            promptRenderer.enabled = false;''', '''            promptRenderer = GetComponent<SpriteRenderer>();
            cameraScaleCompensation = GetComponent<WorldCameraScaleCompensation>();
            if (keyLabel == null)
            {
                Transform child = transform.Find("按键文字");
                if (child != null)
                    keyLabel = child.GetComponent<TextMesh>();
            }
            keyRenderer = keyLabel != null ? keyLabel.GetComponent<MeshRenderer>() : null;
            promptRenderer.sortingOrder = PromptSortingOrder;
            promptRenderer.enabled = false;
            SetKeyVisible(false);''')
t = t.replace('''            promptRenderer.sortingOrder = PromptSortingOrder;
            promptRenderer.enabled = true;
        }''', '''            promptRenderer.sortingOrder = PromptSortingOrder;
            promptRenderer.enabled = true;
            if (keyRenderer != null)
            {
                keyRenderer.sortingLayerID = promptRenderer.sortingLayerID;
                keyRenderer.sortingOrder = PromptSortingOrder + 1;
            }
            SetKeyVisible(true);
        }

        public void SetKeyText(string keyText)
        {
            if (keyLabel != null && !string.IsNullOrEmpty(keyText))
                keyLabel.text = keyText;
        }''')
t = t.replace('''            if (promptRenderer != null)
                promptRenderer.enabled = false;
        }''', '''            if (promptRenderer != null)
                promptRenderer.enabled = false;
            SetKeyVisible(false);
        }

        private void SetKeyVisible(bool visible)
        {
            if (keyLabel != null)
                keyLabel.gameObject.SetActive(visible);
        }''')
p.write_text(t, encoding='utf-8', newline='\n')
print('script updated')
