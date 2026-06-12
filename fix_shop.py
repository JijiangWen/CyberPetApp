import os

filepath = 'Models/Shop.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    text = f.read()

reps = [
    ('new Food("纯净水", 0, 0, 0), 5', 'new Food("纯净水", 0, 0, 0), 2'),
    ('new Food("普通猫粮", 15, 2, 2), 10', 'new Food("干瘪的猫粮", 10, 1, 0), 5'),
    ('new Food("高级猫粮", 25, 5, 3), 15', 'new Food("混合肉干猫粮", 20, 5, 5), 15'),
    ('new Food("金枪鱼罐头", 35, 15, 5), 20', 'new Food("鲜肉营养罐头", 35, 10, 10), 30'),
    ('new Food("猫薄荷包", 0, 0, 50), 30', 'new Food("浓缩猫薄荷", 0, 0, 60), 50'),
    ('new Food("能量饮料", 0, 40, 0), 25', 'new Food("赛博能量液", 0, 50, -5), 45'),
    ('"基础饱腹"', '"廉价的基础饱腹"'),
    ('"喂食器/手动喂"', '"性价比口粮"'),
    ('"高饱腹高精力"', '"高饱腹、恢复精力"'),
    ('"手动快乐+50 或装入喂食器"', '"大幅提升快乐（+60）"'),
    ('"手动精力+40"', '"快速恢复精力（+50）"'),
]

for old, new in reps:
    text = text.replace(old, new)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(text)

