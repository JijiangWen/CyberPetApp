import sys

path = 'Components/Pages/Home.Market.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

import re
content = re.sub(
    r'var newMoney = await _fishingService\.SellFishAsync\(player!\.Id, fish\.Id\);',
    r'var newMoney = await _fishingService.SellFishAsync(player!, fish);',
    content
)

content = re.sub(
    r'await _achievementService\.SyncProgressAsync\(player, fishRecords, HasDeepSeaPermanent\(\)\);\s*await _playerService\.SaveProgressAsync\(player\);',
    r'_ = _achievementService.SyncProgressAsync(player, fishRecords, HasDeepSeaPermanent());',
    content, flags=re.DOTALL
)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)

print('Updated Home.Market.cs')
