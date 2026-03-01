Add-Type -AssemblyName System.Drawing

function Find-CharacterBounds {
    param([System.Drawing.Bitmap]$img, [int]$GreenThreshold = 80, [float]$GreenRatio = 1.2)

    $w = $img.Width
    $h = $img.Height
    $minX = $w; $maxX = 0; $minY = $h; $maxY = 0

    for ($y = 0; $y -lt $h; $y += 2) {
        for ($x = 0; $x -lt $w; $x += 2) {
            $px = $img.GetPixel($x, $y)
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

    return @{ X = $minX; Y = $minY; W = ($maxX - $minX + 1); H = ($maxY - $minY + 1) }
}

function Remove-GreenScreen {
    param(
        [string]$InputPath,
        [string]$OutputPath,
        [int]$GreenThreshold = 80,
        [float]$GreenRatio = 1.2
    )

    $src = [System.Drawing.Bitmap]::new($InputPath)
    Write-Host "Source: $($src.Width) x $($src.Height)"

    $bounds = Find-CharacterBounds -img $src -GreenThreshold $GreenThreshold -GreenRatio $GreenRatio
    Write-Host "Character bounds: X=$($bounds.X) Y=$($bounds.Y) W=$($bounds.W) H=$($bounds.H)"

    $rect = [System.Drawing.Rectangle]::new($bounds.X, $bounds.Y, $bounds.W, $bounds.H)
    $cropped = $src.Clone($rect, $src.PixelFormat)
    $src.Dispose()

    $w = $cropped.Width
    $h = $cropped.Height
    $out = [System.Drawing.Bitmap]::new($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

    for ($y = 0; $y -lt $h; $y++) {
        for ($x = 0; $x -lt $w; $x++) {
            $px = $cropped.GetPixel($x, $y)
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

    $out.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host "Saved: $OutputPath ($w x $h)"
    $out.Dispose()
    $cropped.Dispose()
}

$assetsDir = "C:\Users\Administrator\.cursor\projects\s-My-Project-WelCome\assets"
$outDir = "s:\My_Project\WelCome\WelCome\Assets\Resources\Characters"

Write-Host "=== Processing Que ==="
Remove-GreenScreen `
    -InputPath (Join-Path $assetsDir "Que_greenscreen.png") `
    -OutputPath (Join-Path $outDir "Que_sprite.png")

Write-Host ""
Write-Host "=== Processing Laia ==="
Remove-GreenScreen `
    -InputPath (Join-Path $assetsDir "Laia_greenscreen.png") `
    -OutputPath (Join-Path $outDir "Laia_sprite.png")
