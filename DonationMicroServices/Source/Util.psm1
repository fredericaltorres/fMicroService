
function Write-HostColor([string]$message, $color = "Cyan") {

    Write-Host $message -ForegroundColor $color
}

function JsonParse([string]$json) {

    [array]$jsonContent = $json | ConvertFrom-Json
    return ,$jsonContent
}

# Execute the block of code 60 times and wait 6 seconds in between each try
# If the block fail 60 time we will have to wait 6 minutes
# We have a 6 minutes time out by default
function Retry([string]$message, [ScriptBlock] $block, [int]$wait = 6, [int]$maxTry = 60) { 

    $try = 0

    Write-Host "$message" -ForegroundColor Cyan -NoNewline

    while($true) {

        Write-Host "." -ForegroundColor Cyan -NoNewline

        try {

            $ok = & $block
            if($ok) {

                Write-Host "`r`n[PASSED]$message`r`n" -ForegroundColor Green
                return $true
            }
            Start-Sleep -s $wait
            $try += 1
            if($try -eq $maxTry) {

                Write-Error "[FAILED]Timeout: $message"
                break # Fail time out
            }
        }
        catch {            
            
            $ErrorMessage = $_.Exception.Message
            $FailedItem = $_.Exception.ItemName
            # Write-Error
            Write-Output "`r`nRetry caugth an error for: $message, error: $ErrorMessage"
            Start-Sleep -s $wait
        }
    }
    Write-Output "Retry failed and return $false for: $message"
    return $false
}

function validateUrl($url) {

    Retry "Verifying url: $url succeed" {

        $httpResponse = (Invoke-RestMethod -Uri $url) # This will throw an error if the url does not respond
        $dataType = $httpResponse.GetType().Name
        switch($dataType) {
            String {
                if($httpResponse.ToLowerInvariant().Contains("<html")) {
                    return $true
                }
                else {
                    $m = "Url:$url does not return html"
                    Write-Error $m
                    return $false
                }
            }
            PSCustomObject {
                # This is probably a JSON response that has been parsed by PowerShell
                return $true 
            }
        }
    } -wait 10 -maxTry 3
}

<#
$context = @{
    ENVIRONMENT = "prod";
    APP_VERSION = "1.0.2"
}
#>

function processFile($context, $fileName, $newFileName = $null) {

    $content = Get-Content $fileName
    if($newFileName -eq $null) {

        $newFileName = [System.IO.Path]::Combine($env:TEMP, [System.IO.Path]::GetFileName($fileName))
    }

    foreach($key in $context.keys) {

        $value = $context[$key]
        $content = $content.Replace("`${$key}", $value)
        $content = $content.Replace("`${$($key)_UNDERSCORE}", $value.replace(".", "-"))
    }
    $content | Set-Content $newFileName | Out-Null
    return $newFileName
}

function GetProjectName() {
<#
    .Synopsis
        Returns the name of the .NET Core project name from the current folder
#>
    $project = gci -path . -rec -Include *.csproj # Assume there is only one csproj in the current directory
    if($project -eq $null) {
        throw "Cannot find C# project file in current folder"
    }
    else {
        if($project.GetType().Name -eq "FileInfo") { # it is an array, we loaded more than one .csproj            
            return $project.Name
        }
        else {
            throw "Found $($project.Length) C# project files in current folder"
        }
    }
}

function GetAppNameFromProject() {
<#
    .Synopsis
        Returns the name of the .NET Core project name from the current folder with no extension and in lowercase
#>
    return [System.IO.Path]::GetFileNameWithoutExtension((GetProjectName)).ToLowerInvariant()
}

function GetProjectVersion() {
<#
    .Synopsis
        Returns the version of the .NET Core project from the current folder.
        The default version 1.0.0 will not work. Set the version using the IDE.
#>
    $projectName = GetProjectName
    [xml]$doc = get-content($projectName)
    $version = $doc.Project.PropertyGroup.Version
    return $version
}

function getCurrentScriptPath() {

    return [System.IO.Path]::GetDirectoryName($script:MyInvocation.MyCommand.Path)
}


function updateJsonFileWithProperty([string]$jsonFileName, [string]$propertyName, $propertyValue) {

    if(Test-Path $jsonFileName) {
        $json = Get-Content ($jsonFileName)
    }
    else {
        $json = "{}"
    }

    $jsonParsed = JsonParse($json)
    $dic = @{}; # Load the parsed json into a dictionary - only support a JSON object containing a list of name/value
    foreach($e in $jsonParsed) {
        $meta = Get-Member -InputObject $e -MemberType NoteProperty
        if($meta -ne $null) {
            $value = $e | Select-Object -ExpandProperty $meta[0].Name
            $dic.Add($meta[0].Name, $value)
        }
    }
    if($dic.ContainsKey($propertyName)) {
        $dic.Remove($propertyName)
    }
    $dic.Add($propertyName, $propertyValue);
    ConvertTo-Json -InputObject $dic | Set-Content ($jsonFileName)
}

Export-ModuleMember -Function updateJsonFileWithProperty
Export-ModuleMember -Function getCurrentScriptPath
Export-ModuleMember -Function GetAppNameFromProject
Export-ModuleMember -Function GetProjectName
Export-ModuleMember -Function GetProjectVersion
Export-ModuleMember -Function processFile
Export-ModuleMember -Function validateUrl
Export-ModuleMember -Function Retry
Export-ModuleMember -Function JsonParse
Export-ModuleMember -Function Write-HostColor
