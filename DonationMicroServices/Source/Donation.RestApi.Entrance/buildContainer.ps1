Write-Host "First go up to folder before building container"
pushd
cd ..
cd ..
docker build -t donationrestapientrance -f .\Source\Donation.RestApi.Entrance\Dockerfile .

Pause
popd

<#
    docker run -d -p 80:80 --name donationrestapientrance donationrestapientrance
#>
