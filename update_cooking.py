import re
with open('Models/CookingRecipe.cs', 'r', encoding='utf-8') as f:
    text = f.read()

reps = [
    ('生鱼牁E', '简易生鱼片'),
    ('溪烤小鲤', '炭烤小鱼'),
    ('渁E溪虾', '清炖河虾'),
    ('炭烤虹鳁E', '炭烤大肉鱼'),
    ('香E石鲁E', '香煎鱼排'),
    ('迷雾烤鱼', '海盐烤鱼'),
    ('黁E鱼掁E', '黄金酥脆鱼排'),
    ('雾海全席', '海鲜大拼盘'),
    ('冰纹刺身', '极寒冰鲜刺身'),
    ('传说鱼宴', '传说海鲜盛宴'),
    ('龙涎羹', '深海鱼骨浓汤'),
    ('极E御膳', '虚空星辰晚宴')
]

for old, new in reps:
    text = text.replace(old, new)

with open('Models/CookingRecipe.cs', 'w', encoding='utf-8') as f:
    f.write(text)
