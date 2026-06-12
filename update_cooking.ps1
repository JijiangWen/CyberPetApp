
$content = Get-Content -Path "Models/CookingRecipe.cs" -Raw -Encoding UTF8
$content = $content -replace '"¶??E', '"?ˆÕ¶?•Ğ"'
$content = $content -replace '"Ÿâ?¬?"', '"’Y?¬?"'
$content = $content -replace '"?EŸâ?"', '"´?‰Í?"'
$content = $content -replace '"’Y?“ø?E', '"’Y?‘å“÷?"'
$content = $content -replace '"EÎ?E', '"÷?”r"'
$content = $content -replace '"–À???"', '"ŠC???"'
$content = $content -replace '"?E??E', '"‰©‹àçÆÆ?”r"'
$content = $content -replace '"?ŠC‘SÈ"', '"ŠC?‘å??"'
$content = $content -replace '"™u?hg"', '"?Š¦™u?hg"'
$content = $content -replace '"???‰ƒ"', '"??ŠC?·‰ƒ"'
$content = $content -replace '"?Ÿµã»"', '"[ŠC?œ??"'
$content = $content -replace '"?EŒä‘V"', '"‹•‹ó¯’C?‰ƒ"'
Set-Content -Path "Models/CookingRecipe.cs" -Value $content -Encoding UTF8

