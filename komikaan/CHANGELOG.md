# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

[2.12.0] - 17-12-2025
### Changed
- StopId from Guid to String

[2.12.0] - 17-08-2025
### Changed
- RouteType > ExtendedRouteType

[2.10.1] - 25-05-2025
### Added
- Alert data is passed through
### Changed
- /trips passes enroute data

[2.10.0] - 04-05-2025
### Changed
- NearbyStops now also provides vehicle data

[2.9.0] - 29-03-2025
### Added
- Stop details now returns more mergedstops info
### Changed
- Package updates

[2.8.6] - 13-01-2025
### Changed
- Fix switched around NearbyStops coordinates

[2.8.5] - 05-01-2025
### Added
- Realtime bool to Feed object
- StopTimes int to Feed object

[2.8.4] - 27-12-2024
### Added
- Removed legacy Refit + NS code

[2.8.3] - 27-12-2024
### Changed
- Properly use the NPGSql datasource builder

[2.8.2] - 10-10-2024
- Log lines for stop retrieval

[2.8.1] - 10-10-2024
- Fix platform codes not showing

[2.8.0] - 10-09-2024
- Realtime related changes

[2.7.1] - 25-08-2024
### Added
- Feeds controller
- Cached feeds
### Removed
- Cached all stops

[2.6.0] - 04-08-2024
### Added
-- When collecting stops, also collect routes

[2.4.0] - 04-08-2024
### Added
- Get nearby stops from coordinates
### Removed
- Remove SimpleStops and replace the with GTFSStops

[2.1.1] - 29-07-2024
### Added
- List amount of merged stops for a stop

[2.1.0] - 29-07-2024
### Changed
- Adjust stop time retrieval, pass a datetimeoffset and give a time thats 2 minutes old

[2.0.0] - 13-07-2024
### Added
- GTFS support from multiple sources/countries
### Changed
- lowercase for controller paths
### Removed
- Travel advice related items

[1.7.5] - 24-04-2024
### Changed
- Make sure disruptions are properly detected
- Adapt unit tests to test for these disruptions

[1.7.4] - 24-04-2024
### Changed
- Use DisruptionType Disruption instead of Calamity

[1.7.3] - 24-04-2024
### Changed
- Fix maybe state

[1.7.2] - 24-04-2024
### Changed
- Prevent data retrieval timer from failing due to external API's timing out
- Take in account calamity when calculating journeyexpectations
- Update OpenTelemetry packages
- Update Misc packages

[1.7.1] - 17-03-2024
### Added
- OpenTelemetry support
### Changed 
- Move logic out of controllers

[1.7.0] - 17-03-2024
### Added
- Support for stops along the trip
### Changed
- AffectedStops of disruptions are now SimpleStops's
- NS - If there are no affectedStops defined, unset it and assume it applies to everything

[1.6.0] - 08-03-2024
### Added
- Stop Manager to merge related stops of different suppliers
- A simple mapping system for id mappings of different suppliers
- SimpleStop model
- Added Direciton, LineName and Operator to SimpleRoutePart
### Changed
- Mark a route as "maybe" if:
    - There are routes cancelled for no reason
- Mark a route as "Unknown" if:
    - No routes can be found
- DisruptionController now checks all data suppliers for routes

[1.5.2] - 05-03-2024
### Added
- URL if available for a disruption from NS 
### Changed
- Marked multiple objects as nullable
- Handle NS not providing all data in a calamity

[1.5.1] - 03-03-2024
### Added
- NS Mappings for Bus and Eurostar
### Changed
- Train LegType has been split into RegionalTrain and Train

[1.5.0] - 03-03-2024
### Added
- Added support for new LegType
- Added support for Mapping from a DataSource to the legtype
### Changed
- SimpleRoutePart goes from representing a Station to representing a leg of the journey

[1.4.0] - 02-03-2024
### Added
- Catch API Exceptions from NS
- Added StartDate to SimpleDisruption
- DataSuppliers now have a real StartAsync again

[1.3.0] - 29-02-2024
### Added
- Add track data
### Changed
- Change logic of the possibilities. If all are cancelled, then there is no chance
- Pre-process disruption data for reduced latency on calls

[1.2.3] - 28-02-2024
### Changed
- Dev default port changed
- Take a look at the previous leg for realistic transfer data

[1.2.2] - 28-02-2024
### Changed
- Prod default port to 80

[1.2.1] - 28-02-2024
### Changed
- Change cors policy to include more origins
- Add Log.Logger for initial log lines

[1.2.0] - 28-02-2024
### Added
- Add a readme
### Changed
- Only log full HTTP calls in trace
- Only log partial HTTP calls in debug
- Refactor misc components

[1.1.0] - 28-02-2024
### Added
- Add environment variable support for configuration
### Changed
- Add a data class lib for all models
- Attempt to reformat NS models a bit

[1.0.0] - 28-02-2024
#### Added
- Basic API for pulling disruptions, travel advice and general expectations