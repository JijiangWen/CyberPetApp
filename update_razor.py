import sys
import re

path = 'Components/Pages/Home.razor.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

old_str = '''    private async Task DisassembleFish(Fish fish)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () => result = await _gearMaterialService.DisassembleFishAsync(player!, fish));
        feedMessage = result.msg;
        if (result.ok) await _playerService.SaveProgressAsync(player!);
    }'''

new_str = '''    private async Task DisassembleFish(Fish fish)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () =>
        {
            result = _gearMaterialService.DisassembleFish(player!, fish);
            if (result.ok) await _playerService.SaveProgressAsync(player!);
        });
        feedMessage = result.msg;
    }'''

content = content.replace(old_str, new_str)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)

print('Updated Home.razor.cs')
