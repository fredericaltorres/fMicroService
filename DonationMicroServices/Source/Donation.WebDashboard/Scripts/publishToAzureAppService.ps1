[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
	[Alias('a')]
	[ValidateSet('publish')]
    [string]$action = 'publish',

	[Parameter(Mandatory=$false)]
	[string]$UserName = 'jsonbuser2',

	[Parameter(Mandatory=$false)]
	[string]$Password = "machi123!machi123!",
	
	[Parameter(Mandatory=$false)]
	[string]$PublishProfiles = "Properties\PublishProfiles\DonationWebDashboard - Web Deploy.pubxml"
)
cls

## https://weblog.west-wind.com/posts/2016/Jun/06/Publishing-and-Running-ASPNET-Core-Applications-with-IIS

switch($action) {

	publish { 
        
        Write-Host "Deploy Donation.WebDashboard to Azure using publishing profile" -ForegroundColor Yellow
		dotnet publish .\Donation.WebDashboard.csproj -c Release `
			/p:UserName="$UserName" `
			/p:Password="$Password" `
			/p:PublishProfile="$PublishProfiles"
    }
}

