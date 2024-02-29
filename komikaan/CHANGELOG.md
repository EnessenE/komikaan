# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

[1.3.0] - 29-02-2024
### Added
- Add track data
### Changed
- Change logic of the possibilities. If all are cancelled, then there is no chance

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