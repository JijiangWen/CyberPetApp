import re
import sys

sys.stdout.reconfigure(encoding='utf-8')

with open('Models/FishingSpotCatalog.Generated.cs', encoding='utf-8') as f:
    text = f.read()

matches = re.findall(r'new FishTemplate\(\s*"([^"]+)"', text)
fish_names = sorted(list(set(matches)))
for name in fish_names:
    print(name)
