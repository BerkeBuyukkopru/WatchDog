Add-Type -AssemblyName System.IO.Compression.FileSystem

function Read-Docx($path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($path)
    $entry = $zip.Entries | Where-Object { $_.FullName -eq 'word/document.xml' }
    $stream = $entry.Open()
    $reader = New-Object System.IO.StreamReader($stream)
    $xmlString = $reader.ReadToEnd()
    $reader.Close()
    $stream.Close()
    $zip.Dispose()
    
    $text = ([regex]::Matches($xmlString, '<w:t[^>]*>(.*?)</w:t>') | ForEach-Object { $_.Groups[1].Value }) -join ' '
    return $text
}

$ttd = Read-Docx 'c:\Users\facu\Desktop\Staj Projesi\WatchDog\documents\WatchDog TTD.docx'
$gtd = Read-Docx 'c:\Users\facu\Desktop\Staj Projesi\WatchDog\documents\WatchDog GTD.docx'

$ttd | Out-File -FilePath 'c:\Users\facu\Desktop\Staj Projesi\WatchDog\ttd.txt' -Encoding utf8
$gtd | Out-File -FilePath 'c:\Users\facu\Desktop\Staj Projesi\WatchDog\gtd.txt' -Encoding utf8
