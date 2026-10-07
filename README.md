# PhotoPoster

A .NET Worker Service that picks photos from a local folder, writes a caption and hashtags with an LLM vision model, and posts them to social media on a schedule. Instagram comes first. Bluesky, Tumblr and Reddit are stretch goals.

> **Status:** starter skeleton. The host, configuration, models and interfaces are in place. Nothing is implemented yet. The work is broken into tickets in [docs/BACKLOG.md](docs/BACKLOG.md).

## How it will work

```
 ┌────────── Worker (BackgroundService) ──────────┐
 │ wait for next slot ── IScheduleCalculator      │
 │        │                                       │
 │        ▼                                       │
 │  IPostPipeline.RunOnceAsync                    │
 │    1. IPhotoSource.ScanAsync      (folder)     │
 │    2. IPhotoRepository.SyncAsync  (SQLite)     │
 │    3. pick next unposted photo                 │
 │    4. ICaptionGenerator           (LLM)        │
 │    5. save Draft ─► approval (optional)        │
 │    6. IImageHost.UploadAsync      (blob URL)   │
 │    7. ISocialPublisher.PublishAsync            │
 │    8. record result, delete hosted image       │
 └────────────────────────────────────────────────┘
```

## Project layout

```
PhotoPoster.sln
global.json                  # pins the .NET 10 SDK
Directory.Build.props        # settings shared by all projects
src/PhotoPoster/
  Program.cs                 # DI and options setup; TODO lines show where each implementation registers
  Worker.cs                  # the scheduling loop (placeholder 30s tick for now)
  Configuration/             # options classes bound from appsettings.json
  Models/                    # Photo, PostRecord, PostStatus, GeneratedCaption, PhotoFile
  Services/                  # IPhotoSource, IPhotoRepository, ICaptionGenerator, IImageHost
  Publishers/                # ISocialPublisher (+ InstagramPublisher later)
  Scheduling/                # IScheduleCalculator
  Pipeline/                  # IPostPipeline
  (Data/ arrives in PP-201)
tests/PhotoPoster.Tests/     # xUnit. ScheduleOptionsTests is an example to copy.
docs/BACKLOG.md              # epics and tickets
```

Every `TODO(PP-xxx)` in the code points to a ticket in the backlog.

## Running it

```powershell
dotnet build
dotnet test
dotnet run --project src/PhotoPoster
```

For now it logs a tick every 30 seconds. Startup validation fails if `PhotoSource:RootFolder` is empty, so set it in `appsettings.Development.json`. It's already set to a sample path that you should change.

## Secrets

Never put API keys or tokens in `appsettings*.json`. Use user-secrets locally (the project already has a `UserSecretsId`):

```powershell
cd src/PhotoPoster
dotnet user-secrets set "Caption:ApiKey" "<your key>"
dotnet user-secrets set "Instagram:AccessToken" "<your token>"
dotnet user-secrets list
```

These override the matching keys in `appsettings.json` at runtime. They're stored under `%APPDATA%\Microsoft\UserSecrets\`, outside the repo.

## Tech

- .NET 10 (LTS) Worker Service, Generic Host, DI, Options pattern
- EF Core + SQLite for tracking photos and posts (PP-2xx)
- LLM vision API for captions (PP-3xx)
- Azure Blob Storage for temporary public image URLs (PP-503)
- Instagram Graph API (PP-5xx)
