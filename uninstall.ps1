$installdir = $env:localappdata + "\Programs\BulkRename"
$basepath = "Registry::HKEY_CLASSES_ROOT\Directory\Background\shell\BulkRename"

if (Test-Path -Path $installdir) {
    Remove-Item -Path $installdir -Recurse
}

if (Test-Path -Path $basepath) {
    Remove-Item -Path $basepath -Recurse
}
