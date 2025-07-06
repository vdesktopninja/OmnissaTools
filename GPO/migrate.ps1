#Requires -Modules GroupPolicy

# --- USER CONFIGURATION ---
# Provide the full path to the LGPO.exe file
$LgpoExePath = "C:\LGPO\LGPO.exe"
# --------------------------

# --- STEP 1 & 2: Get GPO names from the user ---
$sourceGpoName = Read-Host "Enter the name of the source GPO (e.g., 'Old VMware Policy')"
$newGpoName = Read-Host "Enter the name for the new GPO (e.g., 'New Omnissa Policy')"

# Check if the source GPO exists
Try {
    $sourceGpo = Get-GPO -Name $sourceGpoName -ErrorAction Stop
}
Catch {
    Write-Error "Error: Source GPO '$sourceGpoName' not found. Aborting script."
    return
}

# --- STEP 3: Copy settings to the new GPO ---
Write-Host "Creating new GPO '$newGpoName' and copying settings..."
Try {
    # Remove a pre-existing GPO with the same name to avoid conflicts
    Get-GPO -Name $newGpoName -ErrorAction SilentlyContinue | Remove-Gpo -Confirm:$false -ErrorAction SilentlyContinue
    
    # Use Copy-Gpo to create the new GPO and copy settings in one step
    Copy-Gpo -SourceName $sourceGpoName -TargetName $newGpoName -CopyAcl -ErrorAction Stop
    
    # After a successful copy, get the new GPO object
    $newGpo = Get-GPO -Name $newGpoName

    Write-Host "GPO copied successfully." -ForegroundColor Green
}
Catch {
    Write-Error "An error occurred while copying the GPO. Make sure you have the required permissions."
    Write-Error $_.Exception.Message
    return
}

# --- STEP 4: Prepare the environment and get the Registry.pol file ---
$tempPath = New-Item -ItemType Directory -Path (Join-Path $env:TEMP "GpoEdit-$(Get-Random)")
$registryPolSourcePath = ""

$gpoSysvolMachinePath = "\\$($env:USERDNSDOMAIN)\SYSVOL\$($env:USERDNSDOMAIN)\Policies\{$($newGpo.Id)}\Machine"
$gpoSysvolUserPath = "\\$($env:USERDNSDOMAIN)\SYSVOL\$($env:USERDNSDOMAIN)\Policies\{$($newGpo.Id)}\User"

if (Test-Path -Path (Join-Path -Path $gpoSysvolMachinePath -ChildPath "Registry.pol")) {
    $registryPolSourcePath = Join-Path -Path $gpoSysvolMachinePath -ChildPath "Registry.pol"
}
elseif (Test-Path -Path (Join-Path -Path $gpoSysvolUserPath -ChildPath "Registry.pol")) {
    $registryPolSourcePath = Join-Path -Path $gpoSysvolUserPath -ChildPath "Registry.pol"
}
else {
    Write-Warning "Registry.pol file not found in the new GPO. The source GPO might not contain registry settings. Exiting."
    Remove-Item -Path $tempPath.FullName -Recurse -Force
    return
}

$registryPolTempPath = Join-Path -Path $tempPath.FullName -ChildPath "Registry.pol"
$registryTxtPath = Join-Path -Path $tempPath.FullName -ChildPath "Registry.txt"
Write-Host "Downloading Registry.pol file from the new GPO..."
Copy-Item -Path $registryPolSourcePath -Destination $registryPolTempPath

# --- STEP 5: Convert the .pol file to .txt using LGPO.exe ---
# Check if the LGPO.exe file exists at the specified path
if (-not (Test-Path -Path $LgpoExePath -PathType Leaf)) {
    Write-Error "LGPO.exe not found at the specified path: $LgpoExePath"
    Write-Error "Please update the `$LgpoExePath` variable at the beginning of the script."
    Remove-Item -Path $tempPath.FullName -Recurse -Force
    return
}

Write-Host "Converting Registry.pol file to text format..."
& $LgpoExePath /parse /m $registryPolTempPath | Out-File $registryTxtPath

# --- STEP 6: Modify the text file content using the transformation map ---
Write-Host "Modifying the text file content using the transformation map..."

# Final and complete transformation map
$pathMappings = @{
    "Software\Policies\VMware, Inc.\VMware VDM\VMware Unity"      = 'Software\Policies\Omnissa\Horizon\Unity';
    "SOFTWARE\POLICIES\VMware, Inc.\VMware VDM\URLRedirection"      = 'SOFTWARE\POLICIES\Omnissa\Horizon\URLRedirection';
    "SOFTWARE\VMware, Inc.\VMware VDM\UnityShell\Run"               = 'SOFTWARE\Omnissa\Horizon\UnityShell\Run';
    "SOFTWARE\Policies\VMware, Inc.\VMware Blast\Config"            = 'SOFTWARE\Policies\Omnissa\Horizon\Blast\Config';
    "SOFTWARE\Policies\VMware, Inc.\VMware VDM\ScannerRedirection"  = 'SOFTWARE\Policies\Omnissa\Horizon\ScannerRedirection';
    "SOFTWARE\Policies\VMware, Inc.\VMware VDM\SerialCOM"           = 'SOFTWARE\Policies\Omnissa\Horizon\SerialCOM';
    "SOFTWARE\Policies\VMware, Inc.\VMware VDM\UNCRedirection"      = 'SOFTWARE\Policies\Omnissa\Horizon\UNCRedirection';
    "Software\Policies\VMware, Inc.\VMware AppTap"                  = 'Software\Policies\Omnissa\Horizon\AppTap';
    "Software\Policies\VMware, Inc.\VMware BrowserRedir"            = 'Software\Policies\Omnissa\Horizon\BrowserRedir';
    "Software\Policies\VMware, Inc.\VMware GEOREDIR"                = 'Software\Policies\Omnissa\Horizon\GEOREDIR';
    "Software\Policies\VMware, Inc.\VMware HTML5MMR"                = 'Software\Policies\Omnissa\Horizon\Html5mmr';
    "Software\Policies\VMware, Inc.\VMware HTML5SERVER"             = 'Software\Policies\Omnissa\Horizon\HTML5SERVER';
    "Software\Policies\VMware, Inc.\VMware tsdr"                    = 'Software\Policies\Omnissa\Horizon\tsdr';
    "Software\Policies\VMware, Inc.\VMware WebRTCRedir"             = 'Software\Policies\Omnissa\Horizon\WebRTCRedir';
    "SYSTEM\CurrentControlSet\Services\vmwicpdr"                    = 'SYSTEM\CurrentControlSet\Services\hznicpdr';
    "Software\Policies\VMware, Inc.\VMware VDM"                     = 'Software\Policies\Omnissa\Horizon';
    "Software\VMware, Inc.\VMware VDM"                              = 'Software\Omnissa\Horizon';
}

$fileContent = Get-Content -Path $registryTxtPath -Raw
# Sorting keys by length (longest first) prevents incorrect, partial replacements
$sortedKeys = $pathMappings.Keys | Sort-Object -Property Length -Descending

foreach ($oldPath in $sortedKeys) {
    $newPath = $pathMappings[$oldPath]
    # Using [regex]::Escape is crucial for correctly handling paths with '\'
    $fileContent = $fileContent -replace [regex]::Escape($oldPath), $newPath
}

$fileContent | Set-Content -Path $registryTxtPath -Force
Write-Host "Changes to the text file have been saved." -ForegroundColor Green

# --- STEP 7: Convert the .txt file back to .pol format ---
Write-Host "Converting the modified .txt file back to .pol format..."

# Use the /r and /w switches according to the user's LGPO version
& $LgpoExePath /r $registryTxtPath /w $registryPolTempPath

# --- STEP 8: Copy the modified .pol file to the GPO folder ---
$destinationPath = Split-Path -Path $registryPolSourcePath -Parent
Write-Host "Copying the modified Registry.pol file to the destination in SYSVOL..."
Copy-Item -Path $registryPolTempPath -Destination $destinationPath -Force

# Cleanup
Write-Host "Cleaning up temporary files..."
Remove-Item -Path $tempPath.FullName -Recurse -Force

Write-Host ""
Write-Host "OPERATION COMPLETED SUCCESSFULLY!" -ForegroundColor Cyan
Write-Host "GPO '$newGpoName' has been created and modified based on '$sourceGpoName'." -ForegroundColor Cyan
