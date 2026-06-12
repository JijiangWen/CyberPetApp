import json
import re

# Read original
with open('tools/gen_fishing_spots.py', 'r', encoding='utf-8') as f:
    text = f.read()

# Replace grand names with grounded names in early tiers
replacements = [
    # Spot 1: 静溪 -> 镇外溪流 (Town Outskirts Creek)
    ('"静溪"', '"镇外溪流"'),
    ('神话·镜湖神鲤', '异变·巨型水虎鱼'),
    ('神话·翠影鳗王', '异变·装甲溪鳗'),
    ('镜湖灵鲤', '大水花锦鲤'),
    ('黄金锦鲤', '纯色金化鲤'),
    ('翠鳞银鳟', '老油条鳟鱼'),
    ('溪涧银龙', '大个体溪哥'),
    ('lure_mirror_koi', 'lure_mutant_piranha'),
    ('lure_creek_eel', 'lure_mutant_eel'),
    
    # Spot 2: 浅塘 -> 废弃鱼塘 (Abandoned Fish Pond)
    ('"浅塘"', '"废弃鱼塘"'),
    ('神话·浅塘幻鳞', '异变·沼泽吞噬者'),
    ('塘主金鲤', '塘主·独眼老鲤'),
    ('琉璃鳞鱼', '色彩变异塘鱼'),
    ('浅塘银鳟', '大肚皮白鲢'),
    ('lure_shallow_pond', 'lure_swamp_devourer'),

    # Spot 3: 雾海深渊 -> 近海礁石 (Coastal Reefs)
    ('"雾海深渊"', '"近海礁石"'),
    ('雾海鲈鱼', '近海海鲈'),
    ('雾海鲫鱼', '咸水鲷鱼'),
    ('雾海浮游群', '近海虾群'),
    ('神话·雾海古神鱿', '异变·深海大王乌贼'),
    ('雾海巨鱿', '大型软丝鱿'),
    ('雾海锦鳞', '斑斓石斑'),
    ('lure_abyss_core', 'lure_giant_squid'),
    
    # Spot 4: 芦苇湾 -> 芦苇湿地 (Reed Swamp)
    ('"芦苇湾"', '"芦苇湿地"'),
    ('苇荡幽灵鲶', '泥沼巨斑鲶'),
    ('苇歌传说鲤', '湿地霸主鲤'),
    ('神话·芦苇幽歌', '异变·毒沼巨鳄'),
    ('lure_reed_song', 'lure_poison_croc'),
    
    # Spot 5: 夜光引渠 -> 地下暗河 (Underground River)
    ('"夜光引渠"', '"地下暗河"'),
    ('运河幽灵鲶', '盲眼洞螈'),
    ('引渠金鳍', '地下金丝鲃'),
    ('神话·引渠幻龙', '异变·白化巨螈'),
    ('lure_canal_dragon', 'lure_albino_salamander'),
    
    # Spot 6: 暗涌裂谷 -> 深水海湾 (Deep Water Gulf)
    ('"暗涌裂谷"', '"深水海湾"'),
    ('神话·暗涌裂谷兽', '异变·巨型魔鬼鱼'),
    ('裂谷传说鳕', '深湾老船长巨鳕'),
    ('lure_rift_beast', 'lure_mutant_ray'),
    
    # Let's keep T6-T10 names a bit more fantasy since it's end game
    # "极光冰湾", "沉船墓场", "珊瑚暗流", "远礁外海", "深渊回廊", "星潮海沟", "虚空钓域"
]

for old, new in replacements:
    text = text.replace(old, new)

with open('tools/gen_fishing_spots.py', 'w', encoding='utf-8') as f:
    f.write(text)
