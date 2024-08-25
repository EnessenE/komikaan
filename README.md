# Komikaan.nl
The repository for the website [komikaan.nl](https://komikaan.nl). <br>

This project is heavily inspired by [OVInfo](https://play.google.com/store/apps/details?id=nl.skywave.ovinfo&hl=en) and [DRGL](https://drgl.nl/). These are applications that show dutch public transport info in real time. It's perfect for users who know the route they are taking and any alternate routes. Per stop they simply show information of when transport is leaving, how delayed it easy and some additional miscellaneous info. The problem with these apps is, these instantly fall apart when you cross the border as they work on semi-proprietary protocols. <br>
We try to <b>not</b> do that. Most places in the world publish a Google Transit Feed Specifation ([GTFS](https://gtfs.org)) version of their schedules. This project aims to grab all those public feeds, force them into a database and make them easily querable. <br>

The main issue with this project is the inconsistency of IDs across feeds from different suppliers. For instance, "Paris Gare du Nord" station is represented differently in various data sets. In the Dutch data set, it appears as a distinct stop serving only high-speed trains towards the Netherlands. In contrast, the Paris region data set lists it as a stop with numerous local and international lines. Additionally, flixBus publishes it with a completely different name and geographic location to make matters worse. <br> 
Because of the need to deduplicate and merge data, we can't handle it manually—there are just too many cross-border trip stops. So, we're working on solving this programmatically.

## Simply flow overview
The following is a really simplistic overview of the current structure of the project:
- API (this project)
    - For connecting to the PostgresQL database
- [FileDetector](https://github.com/EnessenE/komikaan-file-detector)
    - Responsible for querying external data suppliers and checking if a new GTFS file has been published
- Harvester*
    - Responsible for importing GTFS datasets
    - Publishes imported stops to an RabbitMQ queue
- Gardener*
    - Responsible processing each stop to attempt a merge with other relevant stops
    - Responsible for publishing deduplicated stop into the database
- [Irrigator](https://github.com/EnessenE/komikaan-irrigator)
    - Responsible for retrieving and processing GTFS realtime data
    - Responsible for writing the realtime data to the database 
- GTFS-PSQL-Multisourced*
    - Working name for the database where the GTFS ends up in

(*) Projects that are not opensource <b>yet</b>

### Whats up with the language on the website?
The website is a poor mix of Dutch and English at the moment (I promise i18n support) but the backend code is not. Everything is written in English with the rare exception of data models from external supplier. There we are bound to whatever they provide us.
If you end up contributing, please follow provided standards.
# Frontend
The frontend can be found in [EnessenE/komikaan-webapp](https://github.com/EnessenE/komikaan).
# How to run
Simply open the .sln file. If you use Visual Studio then it should guide you along with what else you need.
