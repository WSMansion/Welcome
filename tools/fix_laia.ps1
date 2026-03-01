Add-Type -AssemblyName System.Drawing

$assetsDir = "C:\Users\Administrator\.cursor\projects\s-My-Project-WelCome\assets"
$outDir = "s:\My_Project\WelCome\WelCome\Assets\Resources\Characters"
$GreenThreshold = 80
$GreenRatio = 1.2

$src = [System.Drawing.Bitmap]::new((Join-Path $assetsDir "Laia_greenscreen.png"))
Write-Host "Source: $($src.Width) x $($src.Height)"

# Pre-crop left 28% to isolate the full-body character
$cropW = [int]($src.Width * 0.28)
$rect = [System.Drawing.Rectangle]::new(0, 0, $cropW, $src.Height)
$cropped = $src.Clone($rect, $src.PixelFormat)
$src.Dispose()
Write-Host "Pre-cropped to left portion: $($cropped.Width) x $($cropped.Height)"

# Find tight bounds around character
$w = $cropped.Width; $h = $cropped.Height
$minX = $w; $maxX = 0; $minY = $h; $maxY = 0
for ($y = 0; $y -lt $h; $y += 2) {
    for ($x = 0; $x -lt $w; $x += 2) {
        $px = $cropped.GetPixel($x, $y)
        $r = [int]$px.R; $g = [int]$px.G; $b = [int]$px.B
        $isGreen = ($g -gt $GreenThreshold) -and ($g -gt ($r * $GreenRatio)) -and ($g -gt ($b * $GreenRatio))
        if (-not $isGreen) {
            if ($x -lt $minX) { $minX = $x }
            if ($x -gt $maxX) { $maxX = $x }
            if ($y -lt $minY) { $minY = $y }
            if ($y -gt $maxY) { $maxY = $y }
        }
    }
}
$pad = 6
$minX = [Math]::Max(0, $minX - $pad)
$minY = [Math]::Max(0, $minY - $pad)
$maxX = [Math]::Min($w - 1, $maxX + $pad)
$maxY = [Math]::Min($h - 1, $maxY + $pad)
$bw = $maxX - $minX + 1; $bh = $maxY - $minY + 1
Write-Host "Character bounds: X=$minX Y=$minY W=$bw H=$bh"

$charRect = [System.Drawing.Rectangle]::new($minX, $minY, $bw, $bh)
$charImg = $cropped.Clone($charRect, $cropped.PixelFormat)
$cropped.Dispose()

# Remove green and save
$cw = $charImg.Width; $ch = $charImg.Height
$out = [System.Drawing.Bitmap]::new($cw, $ch, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
for ($y = 0; $y -lt $ch; $y++) {
    for ($x = 0; $x -lt $cw; $x++) {
        $px = $charImg.GetPixel($x, $y)
        $r = [int]$px.R; $g = [int]$px.G; $b = [int]$px.B
        $isGreen = ($g -gt $GreenThreshold) -and ($g -gt ($r * $GreenRatio)) -and ($g -gt ($b * $GreenRatio))
        if ($isGreen) {
            $out.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0))
        } else {
            $maxRB = [Math]::Max($r, $b)
            if ($g -gt $maxRB -and $g -gt 60) {
                $spillFactor = [Math]::Min(1.0, ($g - $maxRB) / 100.0)
                $newAlpha = [int]([Math]::Max(0, 255 - ($spillFactor * 200)))
                $newG = [int]([Math]::Max(0, $g - ($g - $maxRB) * 0.7))
                $out.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($newAlpha, $r, $newG, $b))
            } else {
                $out.SetPixel($x, $y, $px)
            }
        }
    }
}

$outPath = Join-Path $outDir "Laia_sprite.png"
$out.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "Saved: $outPath ($cw x $ch)"
$out.Dispose()
$charImg.Dispose()
