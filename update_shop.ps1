
$content = Get-Content -Path "Models/Shop.cs" -Raw -Encoding UTF8
$content = $content -replace 'new Food\("??…", 0, 0, 0\), 5', 'new Food("??…", 0, 0, 0), 2'
$content = $content -replace 'new Food\("•’Ê”Lâì", 15, 2, 2\), 10', 'new Food("Š±?“I”Lâì", 10, 1, 0), 5'
$content = $content -replace 'new Food\("‚?”Lâì", 25, 5, 3\), 15', 'new Food("¬‡“÷Š±”Lâì", 20, 5, 5), 15'
$content = $content -replace 'new Food\("‹à??ã£?", 35, 15, 5\), 20', 'new Food("?“÷??ã£?", 35, 10, 10), 30'
$content = $content -replace 'new Food\("”L”–‰×•ï", 0, 0, 50\), 30', 'new Food("??”L”–‰×", 0, 0, 60), 50'
$content = $content -replace 'new Food\("”\—Ê?—¿", 0, 40, 0\), 25', 'new Food("?””\—Ê‰t", 0, 50, -5), 45'
Set-Content -Path "Models/Shop.cs" -Value $content -Encoding UTF8

