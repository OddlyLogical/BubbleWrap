# Original deterministic synthesis; no sampled or third-party sound material.
$directory = Join-Path $PSScriptRoot '../Assets/Audio'
New-Item -ItemType Directory -Force $directory | Out-Null
for ($variant = 1; $variant -le 3; $variant++) {
    $random = [Random]::new(717 + $variant)
    $rate = 48000; $count = 4800
    $samples = [double[]]::new($count); $peak = 0.0; $lastNoise = 0.0
    for ($i=0; $i -lt $count; $i++) {
        $t = $i / [double]$rate
        $noise = $random.NextDouble()*2-1
        $high = $noise - $lastNoise; $lastNoise = $noise
        $attack = [Math]::Min(1, $t / 0.0003)
        $snap = $high * [Math]::Exp(-$t * (260 + $variant*20)) * 0.5
        $body = [Math]::Sin(2*[Math]::PI*((850+$variant*130)*$t-2200*$t*$t))*[Math]::Exp(-$t*100)*0.45
        $rustle = $noise * [Math]::Exp(-$t*65) * 0.09
        $samples[$i] = ($snap+$body+$rustle)*$attack
        $peak=[Math]::Max($peak,[Math]::Abs($samples[$i]))
    }
    $file = [IO.File]::Create((Join-Path $directory "pop$variant.wav"))
    $writer = [IO.BinaryWriter]::new($file)
    try {
        $writer.Write([Text.Encoding]::ASCII.GetBytes('RIFF'));$writer.Write([int](36+$count*2))
        $writer.Write([Text.Encoding]::ASCII.GetBytes('WAVEfmt '));$writer.Write([int]16)
        $writer.Write([int16]1);$writer.Write([int16]1);$writer.Write([int]$rate);$writer.Write([int]($rate*2));$writer.Write([int16]2);$writer.Write([int16]16)
        $writer.Write([Text.Encoding]::ASCII.GetBytes('data'));$writer.Write([int]($count*2))
        foreach($value in $samples){$writer.Write([int16]($value/$peak*29000))}
    } finally { $writer.Dispose();$file.Dispose() }
}
