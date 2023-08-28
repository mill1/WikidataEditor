# WikidataEditor
Web application that implements CRUD actions on Wikidata items.

The .NET Core application facilitates creating, reading, updating, and deleting Wikidata items via a basic front end. The Wikibase REST API is targeted through GET, PUT, and POST requests.

### Item targets
Next elements of a Wikidata item can be changed:
* Description
* Label
* Aliases
* Statements ***1**

### Wikidata item core data
In addition an experimental solution is implemented to fetch aggregated item data from the API. By means of the appsetting.json specific properties can be configured as 'core data' regarding specific entities.

Implemented entities so far:
1. [Humans](https://www.wikidata.org/wiki/Q5)
2. [Disambiguation pages](https://www.wikidata.org/wiki/Q4167410)
3. [Astronomical object types](https://www.wikidata.org/wiki/Q17444909)

***1**

Statement updates are customized to serve my specific tasks regarding Wikipedia.
