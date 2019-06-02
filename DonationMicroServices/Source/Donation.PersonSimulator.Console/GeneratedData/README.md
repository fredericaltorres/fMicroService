# Data generation for the donation simulator

The row donation data was generated using [mockaroo.com](https://mockaroo.com)

- [User Donation Schema Generator UI](https://mockaroo.com/schemas/173907)
- [mockaroo.UserDonations.schema.json file](./mockaroo.UserDonations.schema.json)

# Generate data

The powershell [generateData.ps1](./generateData.ps1) generate 10 files
- donation0.json, donation1.json, ..., donation9.json
- Each file contain 50 000 donation transactions
- For a total for 500 000 donation transaction

# About the generator API
- I use the web site https://api.mockaroo.com
- I use the free service, which can only return 5000 row per call and is limited to 200 call per day.

# How to generate 50 000 donations group in 10 files each containing 5 000 random generation
```
curl "https://api.mockaroo.com/api/223f6d70?count=100&key=3adfa220" > donation.SmallSample.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation0.json

```
