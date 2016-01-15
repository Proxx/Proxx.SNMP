#Variables

$BinaryPath="$PSScriptRoot\..\..\Proxx.SNMP.dll"
$RootPath="$PSScriptRoot\..\"
$TemplatePath="$PSScriptRoot\..\Properties\"
$AssemblyInfo="AssemblyInfo.cs"


#Get current build version
[Version] $CurrentVersion = ([PSObject] ($([reflection.assembly]::LoadFile($BinaryPath)).ToString() -split ", " | ? { $_ -like "Version*" } | ConvertFrom-StringData)).Version.ToString()
Write-Host "Current version is $CurrentVersion"
[Version] $NewVersion = "{0}.{1}.{2}.{3}" -f $CurrentVersion.Major, $CurrentVersion.Minor, $CurrentVersion.Build, $($CurrentVersion.Revision + 1)
Write-Host "New version is $NewVersion"

(Get-Content -Path "$TemplatePath\$AssemblyInfo.Template" -Raw) -f $([DateTime]::Now.Year), $NewVersion | Out-File -FilePath "$TemplatePath\$AssemblyInfo" -Encoding utf8
