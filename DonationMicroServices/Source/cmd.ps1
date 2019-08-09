cls
Write-Host "Remove current system and data"
# .\DeploymentUtilityMaster.ps1 -a deleteDeployment -app all

.\DeploymentUtilityMaster.ps1 -a deleteDeployment -app Donation.RestApi.Entrance
.\DeploymentUtilityMaster.ps1 -a deleteDeployment -app Donation.PersonSimulator.Console
.\DeploymentUtilityMaster.ps1 -a deleteDeployment -app Donation.QueueProcessor.Console
.\DeploymentUtilityMaster.ps1 -a initData
"Done"



pause 

Write-Host "Deploying..."
.\DeploymentUtilityMaster.ps1 -a deploy -app Donation.RestApi.Entrance
.\DeploymentUtilityMaster.ps1 -a deploy -app Donation.QueueProcessor.Console


.\DeploymentUtilityMaster.ps1 -a deploy -app Donation.PersonSimulator.Console
"Done"

Start-Sleep -s 30


Blog about this

[08/05/2019 02:47:44]posting 10 donationsPostDonation failed, ex:System.Net.Http.HttpRequestException: 
    Connection reset by peer ---> System.Net.Sockets.SocketException: Connection reset by peer
   at System.Net.Http.ConnectHelper.ConnectAsync(String host, Int32 port, CancellationToken cancellationToken)
   --- End of inner exception stack trace ---
   at System.Net.Http.ConnectHelper.ConnectAsync(String host, Int32 port, CancellationToken cancellationToken)
   at System.Threading.Tasks.ValueTask`1.get_Result()
   at System.Net.Http.HttpConnectionPool.CreateConnectionAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at System.Threading.Tasks.ValueTask`1.get_Result()
   at System.Net.Http.HttpConnectionPool.WaitForCreatedConnectionAsync(ValueTask`1 creationTask)
   at System.Threading.Tasks.ValueTask`1.get_Result()
   at System.Net.Http.HttpConnectionPool.SendWithRetryAsync(HttpRequestMessage request, Boolean doRequestAuth, CancellationToken cancellationToken)
   at System.Net.Http.RedirectHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at System.Net.Http.HttpClient.FinishSendAsyncBuffered(Task`1 sendTask, HttpRequestMessage request, CancellationTokenSource cts, Boolean disposeCts)
   at fDotNetCoreContainerHelper.HttpHelper.PostJson(Uri uri, String json, String bearerToken) in /src/Source/fAzureCore/fDotNetCoreContainerHelper/HttpHelper.cs:line 16
   at Donation.PersonSimulator.Console.Program.PostDonation(DonationDTO donation, String donationEndPointIP, String donationEndPointPort, String jsonWebToken, Int32 recursiveCallCount) in /src/Source/Donation.PersonSimulator.Console/Program.cs:line 96

Fix error dril down
