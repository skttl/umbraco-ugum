# Umbraco 19 RC

`main` targets Umbraco and uSync `19.0.0-rc1`. The Umbraco 18 code is preserved on `v18/main` at `fe45313`.

## Build

Use .NET 11 SDK, Node.js 24.13 or newer, and npm 11 or newer.

```sh
cd src/uGum/Client
npm ci
npm run build
cd ../../..
dotnet build src/uGum.sln --configuration Release
dotnet pack src/uGum/uGum.csproj --configuration Release --output artifacts
```

The backoffice entry point uses `authContext.configureClient(client)` for cookie authentication and Umbraco's response interceptors. It no longer supplies a bearer token. The client generator uses `/umbraco/openapi/ugum.json`.

uGum does not use Examine or the removed search APIs. Its custom property value model remains `GumletValue`.

## Test site

Back up the database before starting the upgraded site. An upgraded database cannot be opened by Umbraco 18. Switching Git branches does not roll back the database.

The existing `src/uGum.TestSite/uSync/v18` exports are retained as upgrade input. Do not import old Data Type exports over an upgraded database: they can restore editor aliases that Umbraco 19 migrated. Export the migrated schema with uSync 19 after upgrading a disposable copy of the database. Regenerate published models if the schema changes.

Before release, check backoffice login, asset-status polling, video upload and replacement, deletion, and published playback against that disposable site and a test Gumlet workspace. Building does not verify these flows.

Application settings and the database have not been inspected or changed. Review the imaging HMAC key's canonical Base64 format and renamed or removed settings before running the site.

## Validation on 12 November 2026

The Release solution build, TypeScript/Vite build, NuGet pack, and actionlint check passed. The solution build reported no warnings or errors. The package contains the .NET 11 assembly and built backoffice assets. No database upgrade or live Gumlet operations were run.

`npm audit` reported 14 dependency vulnerabilities: one moderate, 12 high, and one critical. The critical finding is in the transitive `dompurify` dependency. These findings remain unresolved; no forced dependency upgrades were applied.

## References

- [Umbraco 19 RC announcement](https://umbraco.com/blog/umbraco-19-release-candidate/)
- [Umbraco 19 breaking changes](https://docs.umbraco.com/umbraco-cms/19.latest/get-started/upgrading-and-migrating/version-specific)
