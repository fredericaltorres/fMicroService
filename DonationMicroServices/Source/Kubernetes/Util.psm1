

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
            Write-Output "`r`nRetry caugth an error for:$message"
            $ErrorMessage = $_.Exception.Message
            $FailedItem = $_.Exception.ItemName
            Write-Error $ErrorMessage
            Start-Sleep -s $wait
        }
    }
    Write-Output "Retry failed and return $false for: $message"
    return $false
}


function urlMustReturnHtml($url) {

    Retry "Verifying url: $url returns html" {

        $homePage = (Invoke-RestMethod -Uri $url).ToLowerInvariant()
        if($homePage.Contains("<html")) {
            return $true
        }
        else {
            $m = "Url:$url does not return html" 
            Write-Error $m
            return $false
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
    }
    $content | Set-Content $newFileName | Out-Null
    return $newFileName
}


Export-ModuleMember -Function processFile
Export-ModuleMember -Function urlMustReturnHtml
Export-ModuleMember -Function Retry
Export-ModuleMember -Function JsonParse
Export-ModuleMember -Function Write-HostColor
