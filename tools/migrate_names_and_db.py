#!/usr/bin/env python3
import os
import sys

# Mappings of Old Name -> New Name
MAPPINGS = {
    # Materials (Backpack items / Alchemy materials)
    "河边烂木条": "河边烂木条",
    "工业碳纤维": "工业碳纤维",
    "沧海蓝晶石": "沧海蓝晶石",
    "缠绕水草": "缠绕水草",
    "生锈齿轮组": "生锈生锈齿轮组",
    "工业粘合树脂": "工业粘合树脂",
    "工业精密轴承": "工业工业精密轴承",
    "轻质钛合金框架": "轻质轻质钛合金框架架",
    "工业尼龙单丝": "工业尼龙单丝",
    "研磨细鱼鳞粉": "研磨细研磨细鱼鳞粉",
    "五彩神话鳞粉": "五彩五彩神话鳞粉",
    "地下荧光孢子粉": "地下荧光孢子粉",
    "星陨粗铁胚": "星陨粗铁胚",
    "裂隙虚空原核": "裂隙裂隙虚空原核",
    "沧溟古鱼之髓": "沧溟古鱼之髓",
    "红珊瑚碎屑": "红珊瑚碎屑",
    "不融极地冰晶": "不融极地冰晶",
    "深渊巨兽粘液": "深渊巨兽粘液",
    "百年沉船铁皮": "百年沉船铁皮",
    "落星陨铁晶核": "落星陨铁晶核",
    "古树龙涎香": "古树龙涎香",
    "裂隙虚空丝线": "裂隙虚空丝线",
    "老韧芦苇丝": "老韧芦苇丝",
    "热液喷口矿渣": "热液喷口矿渣",
    "月汐重力碎屑": "月汐重力碎屑",
    "虚空幽浮微粒": "虚空幽浮微粒",
    "旧零件废料": "旧零件废料",
    "破损硬磁盘": "破损硬磁盘",
    "老旧流通券": "老旧流通券",

    # Expedition Zones
    "荒地旧废墟": "荒地旧废墟",
    "报废服务器旧址": "报废服务器旧址",
    "老城区黑市暗巷": "老城区老城区黑市暗巷",

    # Shop Items / Foods
    "凉白开水": "凉白开水",
    "临期促销装猫粮": "临期促销装猫粮",
    "“猫乐滋”混合肉干粮": "“猫乐滋”混合肉干粮",
    "“猫主子”吞拿鱼大肉罐": "“猫主子”吞拿鱼大肉罐",
    "“神仙快乐水”浓缩薄荷液": "“神仙快乐水”浓缩薄荷液",
    "“超能红牛”精力补充液": "“超能红牛”精力补充液",

    # Cooking Recipes
    "野溪杂鱼刺身": "野溪杂鱼刺身",
    "篝火烤溪鱼": "篝火烤溪鱼",
    "葱油清炖河虾": "葱油清炖河虾",
    "香草盐焗虹鳟": "香草盐焗虹鳟",
    "黄油香煎鲈鱼": "黄油香煎鲈鱼",
    "雾墙果木烟熏鱼": "雾墙果木烟熏鱼",
    "避风塘脆皮鱼排": "避风塘脆皮鱼排",
    "雾海海鲜杂烩大餐": "雾海海鲜杂烩大餐",
    "北极冰片刺身": "北极冰片刺身",
    "满汉全席·至尊鱼皇宴": "满汉全席·至尊鱼皇宴",
    "沧溟玉液羹": "沧溟玉液羹",
    "极光圣殿至尊御膳": "极光圣殿至尊御膳",

    # Fishing Rods
    "随手捡的柳树条": "随手捡的柳树条",
    "外皮发青的自制竹竿": "外皮发青的自制竹竿",
    "二手跳蚤市场玻璃钢竿": "二手跳蚤市场玻璃钢竿",
    "“迪卡王”入门路亚竿": "“迪卡王”入门路亚竿",
    "“小溪流浪者”独节路亚": "“小溪流浪者”独节路亚",
    "“老兵”碳纤斑驳直柄竿": "“老兵”碳纤斑驳直柄竿",
    "“匠心”手工缠线高碳竿": "“匠心”手工缠线高碳竿",
    "“极昼”深水抗压实心竿": "“极昼”深水抗压实心竿",
    "“夜行者”磨砂哑光隐匿竿": "“夜行者”磨砂哑光隐匿竿",
    "“猎手”折叠伸缩直柄竿": "“猎手”折叠伸缩直柄竿",
    "“龙骨”仿生碳化直柄竿": "“龙骨”仿生碳化直柄竿",
    "“沧溟”特制重底锚竿": "“沧溟”特制重底锚竿",
    "“暗涌”高频感度插接竿": "“暗涌”高频感度插接竿",
    "“钛星”记忆钛合金竿": "“钛星”记忆钛合金竿",
    "“不融冰”极寒碳纤维竿": "“不融冰”极寒碳纤维竿",
    "“高能重力”偏振抛投竿": "“高能重力”偏振抛投竿",
    "“海怪肋骨”重碳插接巨竿": "“海怪肋骨”重碳插接巨竿",
    "“极光追猎者”微导环竿": "“极光追猎者”微导环竿",
    "“万象归一”碳纤维终极枪柄": "“万象归一”碳纤维终极枪柄",
    "“裂隙”高维编织便携旅竿": "“裂隙”高维编织便携旅竿",
    "“利维坦之握”海钓金枪重竿": "“利维坦之握”海钓金枪重竿",
    "“太弦”反重力微导架抛投竿": "“太弦”反重力微导架抛投竿",

    # Fishing Reels
    "生锈的手拨铁线轴": "生锈的手拨铁线轴",
    "“两元店”塑料玩具轮": "“两元店”塑料玩具轮",
    "“沙沙作响”的二手水滴轮": "“沙沙作响”的“沙沙作响”的二手水滴轮",
    "“开拓者”入门级鼓轮": "“开拓者”入门级鼓轮",
    "“大力士”重型改装鼓轮": "“大力士”重型改装鼓轮",
    "“水镜”双卸力纺车轮": "“水镜”双卸力纺车轮",
    "“深蓝”密封防水防咸纺车轮": "“深蓝”密封防水防咸纺车轮",
    "“游鹰”双离心制动鼓轮": "“游鹰”双离心制动鼓轮",
    "“声呐反馈”电子计数鼓轮": "“声呐反馈”电子计数鼓轮",
    "“重压”大扭矩齿轮减速水滴": "“重压”大扭矩齿轮减速水滴",
    "“荧流”油压阻尼纺车轮": "“荧流”油压阻尼纺车轮",
    "“陨星”钛镂空轻量化轮": "“陨星”钛镂空轻量化轮",
    "“极地”防寒抗低温齿轮箱": "“极地”防寒抗低温齿轮箱",
    "“裂变”双线杯速比调节轮": "“裂变”双线杯速比调节轮",
    "“沧溟”万级深海大型纺车": "“沧溟”万级深海大型纺车",
    "“无声”碳纤多盘刹车轮": "“无声”碳纤多盘刹车轮",
    "“极限”航天级阳极氧化飞线轮": "“极限”航天级阳极氧化飞线轮",
    "“脉冲交互”微调双煞水滴": "“脉冲交互”微调双煞水滴",
    "“怒潮”数字闭环电绞轮": "“怒潮”数字闭环电绞轮",
    "“天枢”磁悬浮无摩擦线轴轮": "“天枢”磁悬浮无摩擦线轴轮",

    # Fishing Lines
    "外婆缝被子的红棉线": "外婆缝被子的红棉线",
    "“地摊货”粗尼龙单丝线": "“地摊货”粗尼龙单丝线",
    "“大力马”4编基础PE线": "“大力马”4编基础PE线",
    "“幽影”夜光涂层线": "“幽影”夜光涂层线",
    "“隐形”氟碳碳素前导线": "“隐形”氟碳碳素前导线",
    "“顺滑”8编高密PE线": "“顺滑”8编高密PE线",
    "“包钢”防咬金属钢丝前导": "“包钢”防咬金属钢丝前导",
    "“重力”高比重沉水尼龙线": "“重力”高比重沉水尼龙线",
    "“微脉冲”电感应传导线": "“微脉冲”电感应传导线",
    "“巨兽”极限抗磨多股编织": "“巨兽”极限抗磨多股编织",
    "“极速”低风阻抛投PE线": "“极速”低风阻抛投PE线",
    "“星屑”高感度混合前导线": "“星屑”高感度混合前导线",
    "“防冻”氟素树脂涂层线": "“防冻”氟素树脂涂层线",
    "“回声”震动放大碳素线": "“回声”震动放大碳素线",
    "“海蛇皮”复合耐磨合股线": "“海蛇皮”复合耐磨合股线",
    "“偏光”深渊水色隐蔽前导": "“偏光”深渊水色隐蔽前导",
    "“极细”十二编超顺滑PE线": "“极细”十二编超顺滑PE线",
    "“零延展”纳米级碳晶线": "“零延展”纳米级碳晶线",
    "“空天”高分子超密分子线": "“空天”高分子超密分子线",
    "“永恒”钛晶编织超耐磨巨线": "“永恒”钛晶编织超耐磨巨线",

    # Fishing Lures
    "压扁的生锈铁勺": "压扁的生锈铁勺",
    "“大尾巴”橡胶红面包虫": "“大尾巴”橡胶红面包虫",
    "“夜魔”简易自发光塑料管": "“夜魔”简易自发光塑料管",
    "“歪嘴”手工涂装浮水米诺": "“歪嘴”手工涂装浮水米诺",
    "“肥水”沙蚕软饵配铅头钩": "“肥水”沙蚕软饵配铅头钩",
    "“反光”鳞片压制沉水VIB": "“反光”鳞片压制沉水VIB",
    "“红头阿玛尼”经典米诺": "“红头阿玛尼”经典米诺",
    "“幽灵小飞贼”水面波扒": "“幽灵小飞贼”水面波扒",
    "“激流斩”重盐沉水棒贝软虫": "“激流斩”重盐沉水棒贝软虫",
    "“金蝉脱壳”避障防挂雷蛙": "“金蝉脱壳”避障防挂雷蛙",
    "“亮闪闪”金属旋转复合亮片": "“亮闪闪”金属旋转复合亮片",
    "“颤流”偏心超低频VIB": "“颤流”偏心超低频VIB",
    "“破冰者”深海慢摇铁板": "“破冰者”深海慢摇铁板",
    "“多节斑马”缓沉铅笔饵": "“多节斑马”缓沉铅笔饵",
    "“疯狂乌贼”发光触须拖钓饵": "“疯狂乌贼”发光触须拖钓饵",
    "“闷响”内置钨珠避障小胖子": "“闷响”内置钨珠避障小胖子",
    "“极速羽毛”低阻防缠绕飞蝇": "“极速羽毛”低阻防缠绕飞蝇",
    "“幻影”多节仿生活性多段鱼": "“幻影”多节仿生活性多段鱼",
    "“终极撕裂者”深海巨型波扒": "“终极撕裂者”深海巨型波扒",
    "“星海之光”变频自发光仿生饵": "“星海之光”变频自发光仿生饵",

    # Gem Recipes & Target Lure displayNames / descriptions
    "溪河激口聚能石": "溪河激口聚能石",
    "深海抗阻御浪石": "深海抗阻御浪石",
    "彩光聚宝护符石": "彩光聚宝护符石",
    "极寒防断负重石": "极寒防断负重石",
    "颤音敏锐传感器": "颤音敏锐传感器",
    "“溪流诱惑”特制饵": "“溪流诱惑”特制饵",
    "“翠影”特化溪爬虫": "“翠影”特化溪爬虫",
    "“海怪眼珠”发光活饵": "“海怪眼珠”发光活饵",
    "“引渠幽灵”亮晶晶": "“引渠幽灵”亮晶晶",
    "“极光霜骨”不融冰饵": "“极光霜骨”不融冰饵",
    "“沧龙撕裂者”高腥活饵": "“沧龙撕裂者”高腥活饵",
    "“金鳞海皇”特调香饵": "“金鳞海皇”特调香饵",
    "“废塘幻影”荷心饵": "“废塘幻影”荷心饵",
    "“苇草低吟”软尾饵": "“苇草低吟”软尾饵",
    "“地心熔岩”矿渣饵": "“地心熔岩”矿渣饵",
    "“沉船亡魂”锈迹饵": "“沉船亡魂”锈迹饵",
    "“红珊瑚心”活性饵": "“红珊瑚心”活性饵",
    "“深渊荧光”粘液饵": "“深渊荧光”粘液饵",
    "“引力潮汐”星潮饵": "“引力潮汐”星潮饵",
    "“虚空裂隙”吞噬饵": "“虚空裂隙”吞噬饵",
    "“终焉鲸歌”共鸣饵": "“终焉鲸歌”共鸣饵",

    # Spot 1 fish
    "溪边小白条": "溪边小白条",
    "土麦穗鱼": "土麦穗鱼",
    "河湾青壳虾": "河湾青壳虾",
    "溪底石蟹": "溪底石蟹",
    "野柳根子": "野柳根子",
    "宽鳍马口鱼": "宽鳍马口鱼",
    "滑溜大泥鳅": "滑溜大泥鳅",
    "野花翅子(虹鳟)": "野花翅子(虹鳟)",
    "溪边黄石爬子": "溪边黄石爬子",
    "精明老花鳟": "精明老花鳟",
    "大鳍红马口(溪哥)": "大鳍红马口(溪哥)",
    "野池红鳞锦鲤": "野池红鳞锦鲤",
    "金背鲤仙": "金背鲤仙",
    "异变·“镜湖水虎兽”": "异变·“镜湖水虎兽”",
    "异变·“铁骨溪鳗”": "异变·“铁骨溪鳗”",

    # Spot 2 fish
    "烂泥塘鲫鱼": "烂泥塘鲫鱼",
    "烂泥塘鲫鱼": "烂泥塘鲫鱼",
    "腐草泥河虾": "腐草泥河虾",
    "塘湾青皮黑鱼": "塘湾青皮黑鱼",
    "小昂刺鱼(黄骨鱼)": "小昂刺鱼(黄骨鱼)",
    "浮萍野草鱼": "浮萍野草鱼",
    "麦穗杂鱼仔": "麦穗杂鱼仔",
    "烂泥塘大青鱼": "烂泥塘大青鱼",
    "烂泥塘大青鱼": "烂泥塘大青鱼",
    "烂草塘毛蟹": "烂草塘毛蟹",
    "红肚皮罗非鱼": "红肚皮罗非鱼",
    "胖头鳙鱼(剁椒鱼头)": "胖头鳙鱼(剁椒鱼头)",
    "塘主·“独眼老草鱼”": "塘主·“独眼老草鱼”",
    "塘主·“独眼老草鱼”": "塘主·“独眼老草鱼”",
    "异变·“淤泥吞噬者”": "异变·“淤泥吞噬者”",

    # Spot 3 fish
    "浪击海鲈鱼": "浪击海鲈鱼",
    "礁影红加吉鱼": "礁影红加吉鱼",
    "沙蚕爬虫": "沙蚕爬虫",
    "小透明鱿鱼仔": "小透明鱿鱼仔",
    "礁石小红虾": "礁石小红虾",
    "小海鳗苗": "小海鳗苗",
    "荧光墨鱼": "荧光墨鱼",
    "乱石堆电鳗": "乱石堆电鳗",
    "小银剪子(银鳍枪鱼)": "小银剪子(小银剪子(银鳍枪鱼))",
    "烈焰红仙子": "火红珊瑚雀鲷",
    "大灯笼安康鱼": "大灯笼安康鱼",
    "深水魔鬼鳐": "深水魔鬼鳐",
    "巨型软丝鱿": "巨型软丝鱿",
    "斑斓大石斑": "斑斓大石斑",
    "异变·“雾海古神鱿”": "异变·“雾海古神鱿”",

    # Spot 4 fish
    "老芦苇青鲫": "老芦苇青鲫",
    "浅滩青虾": "浅滩青虾",
    "芦苇根小泥鳅": "芦苇根小泥鳅",
    "风纹白鲫": "风纹白鲫",
    "湿地圆螃蟹": "湿地圆螃蟹",
    "芦花游鲦": "芦花游鲦",
    "野性湿地鲈": "野性湿地鲈",
    "苇荡鳜鱼": "苇荡鳜鱼",
    "逆流银大鲑": "逆流银大鲑",
    "泥沼黄斑大鲶": "泥沼黄斑大鲶",
    "湿地黄姑子": "湿地黄姑子",
    "湿地老青鲩": "湿地老青鲩",
    "异变·“毒沼鳄王”": "异变·“毒沼鳄王”",

    # Spot 5 fish
    "盲眼发光鲤": "盲眼发光鲤",
    "暗河玻璃蝌蚪": "暗河玻璃蝌蚪",
    "暗河透明虾": "暗河透明虾",
    "暗河长须鲫": "暗河长须鲫",
    "暗河黄腊丁": "暗河黄腊丁",
    "夜行黑棘鲈": "夜行黑棘鲈",
    "五彩荧光大虾": "五彩荧光大虾",
    "霁光玻璃鱼": "霁光玻璃鱼",
    "引水五彩鲑": "引水五彩鲑",
    "地底粉红鲵": "地底粉红鲵",
    "七彩霓虹鲷": "七彩霓虹鲷",
    "暗河金丝鲃": "暗河金丝鲃",
    "异变·“白化巨螈”": "异变·“白化巨螈”",

    # Spot 6 fish
    "岩缝爬岩鱼": "岩缝爬岩鱼",
    "深渊火山虾": "深渊火山虾",
    "热液口磷虾": "热液口磷虾",
    "裂隙盲鲫": "裂隙盲鲫",
    "深水海鳗苗": "深水海鳗苗",
    "深海石九公": "深海石九公",
    "蓝枪鱼": "蓝枪鱼",
    "裂谷无眼鲶": "裂谷无眼鲶",
    "热液大口黑鱼": "热液大口黑鱼",
    "白玉长寿鳗": "白玉长寿鳗",
    "金目鲷": "金目鲷",
    "深湾老船长鳕鱼": "深湾老船长鳕鱼",
    "异变·“裂谷飞蝠”": "异变·“裂谷飞蝠”",

    # Spot 7 fish
    "破冰雪鳞鲫": "破冰雪鳞鲫",
    "极地白虾": "极地白虾",
    "冰吻沙丁鱼": "冰吻沙丁鱼",
    "霜斑鲱鱼": "霜斑鲱鱼",
    "透明冰晶鱼": "透明透明冰晶鱼",
    "北极王鲑": "北极王鲑",
    "五彩极光鳟": "五彩五彩极光鳟",
    "霜纹白鲂": "霜纹白鲂",
    "冰川蛇鳕": "冰川蛇鳕",
    "北极重水鲈": "北极重水鲈",
    "白化冰鳕": "白化冰鳕",
    "深渊巨口宽咽鱼": "深渊巨口宽咽鱼",
    "极光冰川巨鳎": "极光冰川巨鳎",
    "神话·“极光霜龙”": "神话·“极光霜龙”",

    # Spot 8 fish
    "沉船缝沙丁": "沉船缝沙丁",
    "铁皮锈斑虾": "铁皮锈斑虾",
    "幽灵发光浮游": "幽灵发光浮游",
    "沉船黑斑鳕": "沉船黑斑鳕",
    "锈斑扁鲫": "锈斑扁鲫",
    "沉船黑电鳗": "沉船黑电鳗",
    "黑火透光乌贼": "黑火透光乌贼",
    "暗流刺盖鱼": "暗流刺盖鱼",
    "深海皱鳃鲨": "深海皱鳃鲨",
    "墓场带鱼王": "墓场带鱼王",
    "百年老船壳龟": "百年老船壳龟",
    "沉船幽灵鳕": "沉船幽灵鳕",
    "神话·“沉船亡魂”": "神话·“沉船亡魂”",

    # Spot 9 fish
    "红海葵小丑鱼": "红海葵小丑鱼",
    "珊瑚缝雀尾虾": "珊瑚缝雀尾虾",
    "五彩玻璃雀鲷": "五彩玻璃雀鲷",
    "红点玻璃墨鱼": "红点玻璃墨鱼",
    "珊瑚沙丁": "珊瑚沙丁",
    "红眉斑石斑": "红眉斑石斑",
    "烈焰红仙子": "烈焰红仙子",
    "珊瑚白星裸胸鳝": "珊瑚白星裸胸鳝",
    "红星狼鲈": "红星狼鲈",
    "玳瑁巨海龟": "玳瑁巨海龟",
    "红花金枪鱿": "红花金枪鱿",
    "红珊瑚金眼鲷": "红珊瑚金眼鲷",
    "神话·“珊瑚心海”": "神话·“珊瑚心海”",

    # Spot 10 fish
    "礁盘黄鸡鱼": "礁盘黄鸡鱼",
    "蓝背沙丁鱼": "蓝背沙丁鱼",
    "外海浮游磷虾": "外海浮游磷虾",
    "礁影石九公": "礁影石九公",
    "深水海鳝": "深水海鳝",
    "蓝鳍金枪鱼": "蓝鳍金枪鱼",
    "大马鲛鱼": "大马鲛鱼",
    "飞翼蝠鲼": "飞翼蝠鲼",
    "黑皮旗鱼": "黑皮旗鱼",
    "巨型红鱿鱼": "巨型红鱿鱼",
    "大洋金鳞鲷": "大洋金鳞鲷",
    "大白鲨": "大白鲨",
    "神话·“远海沧龙”": "神话·“远海沧龙”",
    "神话·“金鳞海皇”": "神话·“金鳞海皇”",

    # Spot 11 fish
    "裂隙小鳚": "裂隙小鳚",
    "深渊透明小介虫": "深渊透明小介虫",
    "透明凝胶水母": "透明凝胶水母",
    "深渊琵琶鱼": "深渊琵琶鱼",
    "黑首阿氏鲈": "黑首阿氏鲈",
    "回廊电箭鳗": "回廊电箭鳗",
    "凝胶玻璃鱿": "凝胶玻璃鱿",
    "深海巨齿鱼": "深海巨齿鱼",
    "黑鬼安康鱼": "黑鬼安康鱼",
    "回廊蝠鳐": "回廊蝠鳐",
    "格陵兰睡鲨": "格陵兰睡鲨",
    "神话·“深渊巡礼者”": "神话·“深渊巡礼者”",

    # Spot 12 fish
    "潮汐玻璃鲱": "潮汐玻璃鲱",
    "潮汐玻璃虾": "潮汐玻璃虾",
    "星光浮游": "星光浮游",
    "海沟扁头鱼": "海沟扁头鱼",
    "星点刺盖鲈": "星点刺盖鲈",
    "星海大带鱼": "星海大带鱼",
    "星斑裸胸鳝": "星斑裸胸鳝",
    "海沟深邃巨口鱼": "海沟深邃巨口鱼",
    "潮汐蝠鳐": "潮汐蝠鳐",
    "星潮皱鳃鲨": "星潮皱鳃鲨",
    "星海巨鳞鳕": "星海巨鳞鳕",
    "神话·“星潮巨兽”": "神话·“星潮巨兽”",

    # Spot 13 fish
    "虚影介虫": "虚影介虫",
    "虚无玻璃虾": "虚无玻璃虾",
    "虚空发光鲫": "虚空发光鲫",
    "虚空棘鲷": "虚空棘鲷",
    "虚空电箭鳗": "虚空电箭鳗",
    "虚幻透明鱿": "虚幻透明鱿",
    "虚空巨口鱼": "虚空巨口鱼",
    "虚空飞蝠": "虚空飞蝠",
    "虚空幽灵鲨": "虚空幽灵鲨",
    "终焉红棘鲷": "终焉红棘鲷",
    "虚空巨鲸影": "虚空巨鲸影",
    "神话·“虚空钓主”": "神话·“虚空钓主”",
    "神话·“终焉鲸歌”": "神话·“终焉鲸歌”",

    # Migratory fish
    "跨洋银裸胸鳝": "跨洋银裸胸鳝",
    "归潮蝠鲼": "归潮蝠鲼",
    "星尘巡浪鱼": "星尘巡浪鱼",

    # Remaining edge cases or descriptions
    "异变·“淤泥吞噬者”": "异变·“淤泥吞噬者”",
    "异变·“裂谷飞蝠”": "异变·“裂谷飞蝠”",
    "异变·“毒沼鳄王”": "异变·“毒沼鳄王”",
    "地下暗河": "地下暗河",
    "深水海湾": "深水海湾",
}

# Directories to process
DIRS = ["Models", "Services", "Components", "tools"]
SINGLE_FILES = ["Program.cs"]

def process_file(filepath):
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        print(f"Failed to read {filepath}: {e}")
        return

    new_content = content
    # Order mappings by key length descending to prevent substring issues
    sorted_keys = sorted(MAPPINGS.keys(), key=lambda x: len(x), reverse=True)
    for old in sorted_keys:
        new_val = MAPPINGS[old]
        new_content = new_content.replace(old, new_val)

    if new_content != content:
        try:
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)
            print(f"Replaced strings in: {filepath}")
        except Exception as e:
            print(f"Failed to write {filepath}: {e}")

def main():
    print("Step 1: Renaming strings in C# files, Razor files, Python files, and Markdown files...")
    for folder in DIRS:
        for root, _, files in os.walk(folder):
            for file in files:
                if file.endswith(('.cs', '.razor', '.py', '.md')):
                    # Exclude the generated files if we will rebuild them, but it's safe to process them too
                    process_file(os.path.join(root, file))

    for file in SINGLE_FILES:
        if os.path.exists(file):
            process_file(file)

    print("\nStep 2: Creating C# Database Migration Helper...")
    create_cs_migration_helper()

    print("\nStep 3: Registering DB migration in Program.cs...")
    inject_db_migration_in_program()

    print("\nStep 4: Regenerating FishingSpotCatalog.Generated.cs...")
    import subprocess
    try:
        res = subprocess.run([sys.executable, "tools/gen_fishing_spots.py"], capture_output=True, text=True, check=True)
        print("Successfully ran tools/gen_fishing_spots.py:")
        print(res.stdout)
    except Exception as e:
        print(f"Error running tools/gen_fishing_spots.py: {e}")

def create_cs_migration_helper():
    lines = [
        "using CyberPetApp.Data;",
        "using Microsoft.EntityFrameworkCore;",
        "",
        "namespace CyberPetApp.Services;",
        "",
        "public static class NameMigrationHelper",
        "{",
        "    public static void Migrate(AppDbContext db)",
        "    {",
        "        // Check if database is empty or not created yet to prevent crash",
        "        try",
        "        {",
        "            if (!db.Database.CanConnect()) return;",
        "        }",
        "        catch",
        "        {",
        "            return;",
        "        }",
        "",
        "        // Run SQL updates to migrate user data from old names to new names",
        "        using var transaction = db.Database.BeginTransaction();",
        "        try",
        "        {",
    ]

    # Sort to prevent substring collision issues
    sorted_items = sorted(MAPPINGS.items(), key=lambda x: len(x[0]), reverse=True)
    for old, new in sorted_items:
        if old == new:
            continue
        # We replace in:
        # 1. BackpackItems (ItemName)
        # 2. FishCatchRecords (FishName)
        # 3. Fishes (Name)
        # 4. FishingRods (Name)
        # 5. FishingReels (Name)
        # 6. FishingLines (Name)
        # 7. FishingLures (Name)
        # 8. FeederFoods (Name)
        lines.append(f'            db.Database.ExecuteSqlRaw("UPDATE \\"BackpackItems\\" SET \\"ItemName\\" = {{0}} WHERE \\"ItemName\\" = {{1}}", "{new}", "{old}");')
        lines.append(f'            db.Database.ExecuteSqlRaw("UPDATE \\"FishCatchRecords\\" SET \\"FishName\\" = {{0}} WHERE \\"FishName\\" = {{1}}", "{new}", "{old}");')
        lines.append(f'            db.Database.ExecuteSqlRaw("UPDATE \\"Fishes\\" SET \\"Name\\" = {{0}} WHERE \\"Name\\" = {{1}}", "{new}", "{old}");')
        lines.append(f'            db.Database.ExecuteSqlRaw("UPDATE \\"FishingRods\\" SET \\"Name\\" = {{0}} WHERE \\"Name\\" = {{1}}", "{new}", "{old}");')
        lines.append(f'            db.Database.ExecuteSqlRaw("UPDATE \\"FishingReels\\" SET \\"Name\\" = {{0}} WHERE \\"Name\\" = {{1}}", "{new}", "{old}");')
        lines.append(f'            db.Database.ExecuteSqlRaw("UPDATE \\"FishingLines\\" SET \\"Name\\" = {{0}} WHERE \\"Name\\" = {{1}}", "{new}", "{old}");')
        lines.append(f'            db.Database.ExecuteSqlRaw("UPDATE \\"FishingLures\\" SET \\"Name\\" = {{0}} WHERE \\"Name\\" = {{1}}", "{new}", "{old}");')
        lines.append(f'            db.Database.ExecuteSqlRaw("UPDATE \\"FeederFoods\\" SET \\"Name\\" = {{0}} WHERE \\"Name\\" = {{1}}", "{new}", "{old}");')

    lines.extend([
        "            transaction.Commit();",
        "        }",
        "        catch (System.Exception ex)",
        "        {",
        '            System.Console.WriteLine($"Name migration failed: {ex.Message}");',
        "            transaction.Rollback();",
        "        }",
        "    }",
        "}"
    ])

    helper_path = "Services/NameMigrationHelper.cs"
    with open(helper_path, 'w', encoding='utf-8') as f:
        f.write("\n".join(lines) + "\n")
    print(f"Wrote C# helper to {helper_path}")

def inject_db_migration_in_program():
    program_path = "Program.cs"
    with open(program_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find builder.Build() and inject right after it
    build_str = "var app = builder.Build();"
    inject_code = """var app = builder.Build();

// Run DB Name Migration on Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        CyberPetApp.Services.NameMigrationHelper.Migrate(db);
    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine($"Failed to run DB migration on startup: {ex.Message}");
    }
}"""

    if build_str in content and inject_code not in content:
        content = content.replace(build_str, inject_code)
        with open(program_path, 'w', encoding='utf-8') as f:
            f.write(content)
        print("Successfully injected DB migration startup task in Program.cs")
    else:
        print("Program.cs already has migration or builder.Build() not found.")

if __name__ == "__main__":
    main()
