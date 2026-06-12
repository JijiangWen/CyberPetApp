import os

replacements = [
    # Spot names
    ('"静溪"', '"镇外溪流"'),
    ('静溪', '镇外溪流'),
    ('"浅塘"', '"废弃鱼塘"'),
    ('浅塘', '废弃鱼塘'),
    ('"雾海深渊"', '"近海礁石"'),
    ('雾海深渊', '近海礁石'),
    ('"芦苇湾"', '"芦苇湿地"'),
    ('芦苇湾', '芦苇湿地'),
    ('"夜光引渠"', '"地下暗河"'),
    ('夜光引渠', '地下暗河'),
    ('"暗涌裂谷"', '"深水海湾"'),
    ('暗涌裂谷', '深水海湾'),

    # Fish names
    ('神话·镜湖神鲤', '异变·巨型水虎鱼'),
    ('神话·翠影鳗王', '异变·装甲溪鳗'),
    ('镜湖灵鲤', '大水花锦鲤'),
    ('黄金锦鲤', '纯色金化鲤'),
    ('翠鳞银鳟', '老油条鳟鱼'),
    ('溪涧银龙', '大个体溪哥'),
    
    ('神话·浅塘幻鳞', '异变·沼泽吞噬者'),
    ('塘主金鲤', '塘主·独眼老鲤'),
    ('琉璃鳞鱼', '色彩变异塘鱼'),
    ('浅塘银鳟', '大肚皮白鲢'),

    ('神话·雾海古神鱿', '异变·深海大王乌贼'),
    ('雾海巨鱿', '大型软丝鱿'),
    ('雾海锦鳞', '斑斓石斑'),

    ('苇荡幽灵鲶', '泥沼巨斑鲶'),
    ('苇歌传说鲤', '湿地霸主鲤'),
    ('神话·芦苇幽歌', '异变·毒沼巨鳄'),

    ('运河幽灵鲶', '盲眼洞螈'),
    ('引渠金鳍', '地下金丝鲃'),
    ('神话·引渠幻龙', '异变·白化巨螈'),

    ('神话·暗涌裂谷兽', '异变·巨型魔鬼鱼'),
    ('裂谷传说鳕', '深湾老船长巨鳕'),

    # Material names - adjusting some low tier ones
    ('竹片', '废弃木料'),
    ('水草', '普通水草'),
    ('碳纤维丝', '强化纤维'),

    # Update Lure internal IDs too (optional, but keep consistency)
    ('lure_mirror_koi', 'lure_mutant_piranha'),
    ('lure_creek_eel', 'lure_mutant_eel'),
    ('lure_shallow_pond', 'lure_swamp_devourer'),
    ('lure_abyss_core', 'lure_giant_squid'),
    ('lure_reed_song', 'lure_poison_croc'),
    ('lure_canal_dragon', 'lure_albino_salamander'),
    ('lure_rift_beast', 'lure_mutant_ray'),
]

dirs_to_check = ['Models', 'Services', 'Components']

for root_dir in dirs_to_check:
    for root, dirs, files in os.walk(root_dir):
        for f in files:
            if f.endswith('.cs') or f.endswith('.razor') or f.endswith('.md'):
                filepath = os.path.join(root, f)
                with open(filepath, 'r', encoding='utf-8') as file:
                    content = file.read()
                
                new_content = content
                for old, new in replacements:
                    new_content = new_content.replace(old, new)
                
                if new_content != content:
                    with open(filepath, 'w', encoding='utf-8') as file:
                        file.write(new_content)
                    print(f'Updated {filepath}')

