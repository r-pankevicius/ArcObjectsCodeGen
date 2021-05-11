##### Wish list

`--namespace` argument for generated code.

`--gdb-version` (`--sde-version`?) argument for picking sample features from a specific version.

Generate tables/records (not only feature classes/features).

Domains

Subtypes

##### Argument parsing
Kind of NIH syndrome, but looked at few command line parses and did not like them:

1. CommandLine (Terse syntax C# command line parser for .NET) Version=2.8.0.0:

    Good: Has Verb and Value concepts for positional arguments.

    Bad: Horrible generated usage text. ParserSettings.EnableDashDash doesn't seem to work, it makes matters worse.

2. Mono.Options, Version=6.0.0.0

    "Everything is an option" concept or I could not see how to use positional arguments.
