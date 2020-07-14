$outputdir = ".\bin\Debug\netcoreapp3.1"
$installdir = $env:localappdata + "\Programs\BulkRename"
$exename = "bulk-rename.exe"

$basepath = "Registry::HKEY_CLASSES_ROOT\Directory\Background\shell\BulkRename"
$commanditem = "command"
$name = "Rename with Code"
$command = "`"" + $installdir + "\" + $exename + "`" `"%V`""


if (Test-Path -Path $installdir) {
    Remove-Item -Path $installdir -Recurse
}

New-Item -ItemType directory -Path $installdir
Copy-Item -Path ($outputdir + "\*") -Destination $installdir


if (Test-Path -Path $basepath) {
    Remove-Item -Path $basepath -Recurse
}

New-Item -Path $basepath
New-Item -Path ($basepath + "\" + $commanditem)

Set-Item -Path $basepath -Value $name
Set-Item -Path ($basepath + "\" + $commanditem) -Value $command
