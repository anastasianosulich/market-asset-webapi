# Test .Net task for retrieving latest market assets' info

Created webapi has 2 endpoints:
* `api/market/assets` - returns all known assets with corresponding mapping as in the 3rd party API; retrieves data from its own database.
* `api/market/price` - has query parameter **assetIds** and returns the latest price of the requested asset(-s).

Webapi service internally subscribes to all assets' real data via websocket suubscriptions and therefore updates its own table `Prices` upon websocket updates to give the user asset(-s) price info when needed.
Due to not knowing all the details about how one should utilize the 3rd party API, I decided to request all assets from 3rd party and update the internal table accordingly if there are any changes in the assets' data in the 3rd upon each my webapi startup.

When a user requests the latest price information for certain assets, we first check our database for the required record. If it does not exist, we then request the latest price information by calling the third-party `/api/bars/v1/bars/count-back` endpoint with the default parameters `barsCount = 1; periodicity = 'minute'`.
Also I found the issue when sending such a request for certain assets the 3rd party API returns "Your subscription does not permit use of SIP data". In that case webapi receives an exception and an empty response will be sent to a user.

To deploy the solution on local machine you need to go into the project folder (where `.sln` file resides) and run command `docker up -d`. After that the endpoint should be accessible via **https://localhost:5001/swagger/index.html** url (I used swagger to access endpoints).
To remove containers run `docker-compose down` command.
