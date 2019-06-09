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
    [int]$port = 44399,
    [Parameter(Mandatory=$false)]
    [bool]$secure = $true
)

# Command when deploy using a Kubernetes cluster and deployment to pod2 + load balancer
# http://40.70.134.67:80/api/info 
# .\Scripts\post.ps1 -a post -count 1 -hostOrIp 52.167.58.190 -port 80 -secure $false

# Command when web api is deployed in azure via ServicePlan + WebApp
#https://donation-restapi-entrance.azurewebsites.net/api/Info
#.\Scripts\post.ps1 -a post -count 1 -hostOrIp donation-restapi-entrance.azurewebsites.net -port 80 -secure $false
#.\Scripts\post.ps1 -a post -count 1 -hostOrIp donation-restapi-entrance.azurewebsites.net -port 443 -secure $true

# https://localhost:44399/api/Donation
# .\Scripts\post.ps1 -a post -count 1 -hostOrIp localhost -port 44399 -nsecure $true
# .\Scripts\post.ps1 -a post -count 1 -hostOrIp localhost -port 80 -secure $false
#.\Scripts\post.ps1 -a post -count 1 -hostOrIp localhost -port 443 -secure $true

function buildUrl([string]$hostOrIp, [int]$port, [bool]$secure ) {
# "https://localhost:44399/api/Donation"
    $protocol = "http"
    if($secure) {
        $protocol = "https"
    }    
    return "$protocol`://$hostOrIp`:$port/api/Donation"
}
function postNewDonation() {
    $contentType = "Content-Type: application/json; charset=utf-8"
    $guid = [System.Guid]::NewGuid()
    $data = '{ `Guid`:`[GUID]`,`FirstName`:`Sonny`,`LastName`:`Haking`,`Email`:`shaking0@theguardian.com`,`Gender`:`Male`,`Phone`:`310-632-6062`,`IpAddress`:`138.27.230.192`,`Country`:`Indonesia`,`Amount`:`$91.37`,`CC_Number`:`4026367644878790`,`CC_ExpMonth`:12,`CC_ExpYear`:2022,`CC_SecCode`:233}'
    $data = $data.replace("``","""""").replace("""","``""").Replace("[GUID]", $guid)
    $url  = buildUrl $hostOrIp $port $secure
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