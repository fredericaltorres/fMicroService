
$MAX_DONATION_PER_HTTP_REQUEST = 5000

function generateOneSetOf5000Donation() {

    $httpResponse = curl "https://api.mockaroo.com/api/223f6d70?count=$($MAX_DONATION_PER_HTTP_REQUEST)&key=3adfa220"
    $content = $httpResponse.Content
    $content = $content.replace("[{", "{").Replace("}]", "}")
    return $content
}

function generateDonationIndex($index) {

    Write-Host "Generate data for donation $index"
    $outputJsonFile = "./donation$($index).json"
    if(Test-Path $outputJsonFile -PathType Leaf) {
        Remove-Item $outputJsonFile
    }

    $JsonBlock = New-Object Collections.Generic.List[String]

    for($i=0; $i -lt 10; $i++) {

        Write-Host "Downloading data for donation $index / $i"
        $JsonBlock.Add((generateOneSetOf5000Donation))
    }
    $FinalJson = $JsonBlock -join " `r`n,`r`n"
    $FinalJson = "[`r`n $FinalJson `r`n]"
    $FinalJson | Set-Content  $outputJsonFile
}


cls

"Donation Data Generation"

for($index=6; $index -lt 10; $index++) {

    generateDonationIndex $index
}