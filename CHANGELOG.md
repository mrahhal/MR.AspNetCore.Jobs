# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
Nothing yet...

## [0.10.0] - 2018-07-20
### Added
- JobsOptions extension to load CronJobRegistry from an assembly. [#15](https://github.com/mrahhal/MR.AspNetCore.Jobs/pull/15)

## [0.9.0] - 2018-06-06
### Changed
- Starting Jobs should now be done before the webhost runs, and not inside Startup's Configure method. Check the Basic sample to learn more.
- Update dependency on aspnetcore to 2.1.

## [0.8.0] - 2017-11-02
### Changed
- Support Asp.Net Core 2.0. [#6](https://github.com/mrahhal/MR.AspNetCore.Jobs/pull/6)

## [0.7.0] - 2017-06-08
### Changed
- SqlServer: Set migration history's table name and schema.

### Fixed
- Services not being disposed correctly after a job execution. [#4](https://github.com/mrahhal/MR.AspNetCore.Jobs/issues/4)

## [0.6.0] - 2017-04-22
### Changed
- SqlServer: Move to using EFCore to manage internal migrations and connections to the database.

[Unreleased]: https://github.com/mrahhal/MR.AspNetCore.Jobs/compare/0.10.0...HEAD
[0.10.0]: https://github.com/mrahhal/MR.AspNetCore.Jobs/compare/0.9.0...0.10.0
[0.9.0]: https://github.com/mrahhal/MR.AspNetCore.Jobs/compare/0.8.0...0.9.0
[0.8.0]: https://github.com/mrahhal/MR.AspNetCore.Jobs/compare/0.7.0...0.8.0
[0.7.0]: https://github.com/mrahhal/MR.AspNetCore.Jobs/compare/0.6.0...0.7.0
[0.6.0]: https://github.com/mrahhal/MR.AspNetCore.Jobs/compare/0.5.0...0.6.0
