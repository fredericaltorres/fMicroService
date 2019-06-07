Write-Host "First go up to folder before building container"
pushd
cd ..
cd ..
docker build -f .\Source\Donation.RestApi.Entrance\Dockerfile .

Pause
popd