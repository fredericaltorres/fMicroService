# Data generation for the donation simulator

The row donation data was generated using [mockaroo.com](https://mockaroo.com)

- [User Donation Schema Generator](https://mockaroo.com/schemas/173907)
- [mockaroo.UserDonations.schema.json file](./mockaroo.UserDonations.schema.json)

# How to generate 50 000 donations group in 10 files each containing 5 000 random generation
```
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation0.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation1.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation2.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation3.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation4.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation5.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation6.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation6.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation7.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation8.json
curl "https://api.mockaroo.com/api/223f6d70?count=5000&key=3adfa220" > donation9.json

```
