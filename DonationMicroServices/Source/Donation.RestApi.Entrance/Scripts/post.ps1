[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
	[Alias('a')]
	[ValidateSet('post' )]
    [string]$action = "post",

	[Parameter(Mandatory=$false)]
    [int]$count = 1,

    [Parameter(Mandatory=$false)]
    [string]$hostOrIp = "localhost",

    [Parameter(Mandatory=$false)]
    [int]$port = 44399
)

function buildUrl($hostOrIp, $port) {
    return "https://$hostOrIp`:$port/api/Donation"
}
function postNewDonation() {
    $contentType = "Content-Type: application/json; charset=utf-8"
    $guid = [System.Guid]::NewGuid()
    $data = '{ `Guid`:`[GUID]`,`FirstName`:`Sonny`,`LastName`:`Haking`,`Email`:`shaking0@theguardian.com`,`Gender`:`Male`,`Phone`:`310-632-6062`,`IpAddress`:`138.27.230.192`,`Country`:`Indonesia`,`Amount`:`$91.37`,`CC_Number`:`4026367644878790`,`CC_ExpMonth`:12,`CC_ExpYear`:2022,`CC_SecCode`:233}'
    $data = $data.replace("``","""""").replace("""","``""").Replace("[GUID]", $guid)
    $url  = buildUrl $hostOrIp $port #"https://localhost:44399/api/Donation"
    $command = "curl.exe -H `"$contentType`" -X POST --data `"$data`" $url "
    Write-Host $command
    Write-Host ""
    Invoke-Expression $command | Out-Null
}

for($i = 0; $i -lt $count; $i++) {
    postNewDonation
}

# work is ms-dos console but not in powershell
#curl.exe -H "Content-Type: application/json; charset=utf-8" -X POST --data "{""Guid"":""cd7af44d-db7b-4d4c-9157-052ce5f50836"",""FirstName"":""Sonny"",""LastName"":""Haking"",""Email"":""shaking0@theguardian.com"",""Gender"":""Male"",""Phone"":""310-632-6062"",""IpAddress"":""138.27.230.192"",""Country"":""Indonesia"",""Amount"":""$91.37"",""CC_Number"":""4026367644878790"",""CC_ExpMonth"":12,""CC_ExpYear"":2022,""CC_SecCode"":233}" https://localhost:44399/api/Donation
#curl.exe -H "Content-Type: application/json; charset=utf-8" -X POST --data "{ ""FirstName"":""Sonny"" }" https://localhost:44399/api/Donation

<#

curl.exe -H "Content-Type: application/json; charset=utf-8" -X POST --data "{ `"`"FirstName`"`":`"`"Sonny`"`" }" https://localhost:44399/api/Donation
curl.exe -H "Content-Type: application/json; charset=utf-8" -X POST --data "{ `"`"Guid`"`":`"`"cd7af44d-db7b-4d4c-9157-052ce5f50836`"`", `"`"FirstName`"`":`"`"Sonny`"`" ,`"`"LastName`"`":`"`"Haking`"`" }" https://localhost:44399/api/Donation
curl.exe -H "Content-Type: application/json; charset=utf-8" -X POST --data "{ `"`"Guid`"`":`"`"cd7af44d-db7b-4d4c-9157-052ce5f50836`"`",`"`"FirstName`"`":`"`"Sonny`"`",`"`"LastName`"`":`"`"Haking`"`",`"`"Email`"`":`"`"shaking0@theguardian.com`"`",`"`"Gender`"`":`"`"Male`"`",`"`"Phone`"`":`"`"310-632-6062`"`",`"`"IpAddress`"`":`"`"138.27.230.192`"`",`"`"Country`"`":`"`"Indonesia`"`",`"`"Amount`"`":`"`"$91.37`"`",`"`"CC_Number`"`":`"`"4026367644878790`"`",`"`"CC_ExpMonth`"`":12,`"`"CC_ExpYear`"`":2022,`"`"CC_SecCode`"`":233}" https://localhost:44399/api/Donation
curl.exe -H "Content-Type: application/json; charset=utf-8" -X POST --data "{`"`"Guid`"`":`"`"cd7af44d-db7b-4d4c-9157-052ce5f50836`"`",`"`"FirstName`"`":`"`"Sonny`"`",`"`"LastName`"`":`"`"Haking`"`",`"`"Email`"`":`"`"shaking0@theguardian.com`"`",`"`"Gender`"`":`"`"Male`"`",`"`"Phone`"`":`"`"310-632-6062`"`",`"`"IpAddress`"`":`"`"138.27.230.192`"`",`"`"Country`"`":`"`"Indonesia`"`",`"`"Amount`"`":`"`"$91.37`"`",`"`"CC_Number`"`":`"`"4026367644878790`"`",`"`"CC_ExpMonth`"`":12,`"`"CC_ExpYear`"`":2022,`"`"CC_SecCode`"`":233}" https://localhost:44399/api/Donation

#>