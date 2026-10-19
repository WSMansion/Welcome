from pathlib import Path
p = Path(r'WelCome/Library/PackageCache/com.ivanmurzak.unity.mcp@1.2.0/Runtime/Data/ComponentRef.cs')
t = p.read_text(encoding='utf-8')
old = '''        public override string ToString()'''
new = '''        public bool Matches(UnityEngine.Component component, int index)
        {
            if (component == null)
                return false;
            if (InstanceID != UnityEngine.EntityId.None)
                return component.GetInstanceID() == InstanceID;
            if (Index >= 0)
                return Index == index;
            return !string.IsNullOrEmpty(TypeName) && component.GetType().Name == TypeName;
        }

        public override string ToString()'''
assert old in t
p.write_text(t.replace(old, new, 1), encoding='utf-8', newline='\n')
print('method added')
