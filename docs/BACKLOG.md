# PhotoPoster Backlog

The whole project, broken into tickets the way you'd find them on a work board. Each ticket is small enough to finish in one or two evenings and ends with something you can run or test.

## How to work this backlog

1. **Go roughly in order.** Ticket IDs follow the build order, and each ticket lists what it depends on. Epics 1 to 4 have no external accounts in the way, so you can code them while the Meta setup (PP-501) settles.
2. **One ticket, one branch, one PR**, even working solo. Name branches like `feature/PP-101-folder-scanner` and put the ticket ID in commit messages. That gives you a portfolio-quality history.
3. **Optional:** copy the tickets into GitHub Issues or Azure Boards with a Projects board (To do / Doing / Done). Dragging cards is motivating.
4. **Check off acceptance criteria** here as you go, or close the issue.

### Definition of Done (applies to every ticket)

- [ ] Builds with no new warnings
- [ ] Tests added for any logic (I/O and HTTP wrappers can be thinner)
- [ ] `dotnet test` passes locally and in CI (once PP-003 is done)
- [ ] No secrets committed
- [ ] The matching `TODO(PP-xxx)` comments are resolved or removed
- [ ] Merged to `main` through a PR

### Ticket types and points

- **Story:** delivers behaviour you can observe.
- **Task:** setup or plumbing.
- **Spike:** research with a time box. The output is a decision written down (in the ticket or a `docs/decisions/` note), not code.
- **Points** are relative size on a 1, 2, 3, 5, 8 scale. For you, 1 point is roughly one focused evening hour. Anything at 8 should probably be split.

---

## Roadmap at a glance

| Epic | Goal | Checkpoint you can demo |
|---|---|---|
| E0 Setup | Repo, CI and secrets in good shape | Green CI badge on GitHub |
| E1 Photo discovery | Find photos on disk | Log lists the photos in your folder |
| E2 Persistence | Remember what exists and what's posted | SQLite DB fills up and "next photo" picks correctly |
| E3 Captioning | LLM writes the caption and hashtags | `--dry-run` prints a caption for a real photo |
| E4 Scheduling | Runs at the configured times | Worker produces Draft posts on schedule |
| E5 Instagram | Actually publishes | A post appears on your **test** account |
| E6 Approval | Human in the loop | Approve drafts in a browser before they post |
| E7 Go live | Reliable and unattended | Running on your real account as a service |
| E8 Stretch | More platforms and smarts | (pick and choose) |

Suggested sprints, about a week each at a few evenings per week:

- **Sprint 1:** PP-001 to PP-003, PP-101 to PP-103, and *start* PP-501 (account setup takes time to settle)
- **Sprint 2:** PP-201 to PP-205
- **Sprint 3:** PP-301 to PP-306
- **Sprint 4:** PP-401 to PP-404
- **Sprint 5–6:** PP-502 to PP-507
- **Sprint 7:** PP-601 to PP-603
- **Sprint 8:** PP-701 to PP-705

---

## E0 · Setup & hygiene

### PP-001 · Push the starter to GitHub
**Type:** Task · **Points:** 1 · **Depends on:** none

Get the skeleton under version control and onto GitHub.

**Acceptance criteria**
- [ ] `dotnet build`, `dotnet test` and `dotnet run --project src/PhotoPoster` all work on your machine
- [ ] Initial commit on `main`, pushed to a new GitHub repo (private is fine to start)
- [ ] Branch protection on `main` requires a PR (Settings → Branches)
- [ ] `RootFolder` in `appsettings.Development.json` points at a real test folder containing 5–10 photos

**Notes:** Make the test folder a *copy* of some photos, not your real library. You'll be pointing buggy code at it.

### PP-002 · Configure user-secrets
**Type:** Task · **Points:** 1 · **Depends on:** PP-001

**Acceptance criteria**
- [ ] Set a dummy `Caption:ApiKey` with `dotnet user-secrets set`
- [ ] Temporarily log `CaptionOptions.ApiKey.Length` at startup to prove it's read, then remove the log line
- [ ] Confirm `git status` shows nothing secret

**Learn:** the configuration provider order (appsettings → appsettings.{Env} → user-secrets → environment variables → command line). Later sources override earlier ones.

### PP-003 · CI pipeline: build and test on every PR
**Type:** Task · **Points:** 2 · **Depends on:** PP-001

**Acceptance criteria**
- [ ] `.github/workflows/ci.yml` runs `dotnet restore`, `build`, `test` on PRs and on pushes to `main`
- [ ] Uses `actions/setup-dotnet` with `global-json-file: global.json`
- [ ] Branch protection requires the CI check to pass
- [ ] CI status badge in README

**Notes:** This should feel familiar from Azure Pipelines. GitHub Actions YAML is very similar. If you'd rather use Azure Pipelines for nostalgia or résumé reasons, that's equally valid.

---

## E1 · Photo discovery

### PP-101 · Implement the folder scanner
**Type:** Story · **Points:** 3 · **Depends on:** PP-001

As the app, I can list every photo file in the configured folder.

**Acceptance criteria**
- [ ] New class `Services/FileSystemPhotoSource.cs` implements `IPhotoSource`
- [ ] Reads `PhotoSourceOptions` (via `IOptions<PhotoSourceOptions>`)
- [ ] Respects `IncludeSubfolders` and `Extensions` (case-insensitive: `.JPG` matches `.jpg`)
- [ ] Skips hidden and system files
- [ ] Missing folder: logs a clear error and returns an empty list rather than crashing
- [ ] Registered in `Program.cs` (uncomment the TODO line)
- [ ] Temporarily call it from `Worker` and log the count and file names to prove it works

**Hints:** `Directory.EnumerateFiles(path, "*", new EnumerationOptions { RecurseSubdirectories = ..., AttributesToSkip = ... })`. `FileInfo` gives you size and last-write time for `PhotoFile`.

### PP-102 · Unit tests for the scanner
**Type:** Task · **Points:** 2 · **Depends on:** PP-101

**Acceptance criteria**
- [ ] Tests create a temp directory (`Directory.CreateTempSubdirectory()`) with a mix of files: photos, non-photos, a subfolder, mixed-case extensions
- [ ] Covers: extension filter, subfolder on and off, missing folder
- [ ] Temp directory is cleaned up (`IDisposable` test class)

**Learn:** `Options.Create(new PhotoSourceOptions { ... })` builds an `IOptions<T>` for tests without a host.

### PP-103 · Content hash for each photo
**Type:** Story · **Points:** 2 · **Depends on:** PP-101

As the app, I recognise a photo even if it was renamed or moved, so I never post it twice.

**Acceptance criteria**
- [ ] A small `IFileHasher` / `Sha256FileHasher` service returns a hex SHA-256 of a file's contents
- [ ] Streams the file (`SHA256.HashDataAsync(stream)`) and doesn't load it all into memory
- [ ] Test: two files with identical bytes but different names give the same hash

**Notes:** Hashing on every scan is fine for hundreds of photos. If it gets slow, PP-203 can skip files whose path, size and last-modified time haven't changed.

---

## E2 · Persistence (EF Core + SQLite)

### PP-201 · Add EF Core, DbContext and entity mapping
**Type:** Task · **Points:** 3 · **Depends on:** PP-001

**Acceptance criteria**
- [ ] Packages added: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`
- [ ] `Data/PhotoPosterDbContext.cs` with `DbSet<Photo>` and `DbSet<PostRecord>`
- [ ] Configuration (in `OnModelCreating` or `IEntityTypeConfiguration<T>` classes):
  - unique index on `Photo.ContentHash`
  - `PostRecord.Status` stored as a string
  - `PostRecord.Hashtags` stored somehow. Decide: a JSON column via `.PrimitiveCollection()` (EF 8+), or a comma-separated value converter. Write down why.
- [ ] Connection string `ConnectionStrings:PhotoPoster` in appsettings (e.g. `Data Source=photoposter.db`), registered with `AddDbContext`
- [ ] `*.db` added to `.gitignore`

**Watch out:** SQLite has limited support for ordering and comparing `DateTimeOffset`. Either store UTC `DateTime` or add a value converter. Look this up before writing the queries in PP-204.

### PP-202 · First migration, applied on startup
**Type:** Task · **Points:** 2 · **Depends on:** PP-201

**Acceptance criteria**
- [ ] `dotnet tool install --global dotnet-ef` (or a local tool manifest)
- [ ] `dotnet ef migrations add InitialCreate --project src/PhotoPoster` creates `Data/Migrations/`
- [ ] App applies pending migrations at startup (`db.Database.MigrateAsync()` in a scope before `host.Run()`)
- [ ] Deleting the .db file and running the app recreates it

### PP-203 · Repository: sync scanned files into the DB
**Type:** Story · **Points:** 3 · **Depends on:** PP-103, PP-202

**Acceptance criteria**
- [ ] `Data/EfPhotoRepository.cs` implements `IPhotoRepository.SyncAsync`
- [ ] New hash: insert a `Photo`
- [ ] Known hash at a new path: update `FilePath` (the file was moved or renamed)
- [ ] Photo in DB but not found in the scan: `IsAvailable = false` (never delete rows, because history matters)
- [ ] Previously unavailable photo found again: `IsAvailable = true`
- [ ] Registered as **scoped** (DbContext is scoped)

### PP-204 · "Next photo to post" selection
**Type:** Story · **Points:** 2 · **Depends on:** PP-203

**Acceptance criteria**
- [ ] `GetNextUnpostedAsync(platform)` returns an available photo with no `Published` post for that platform
- [ ] Strategy decided and documented: oldest discovered first, random, or oldest file date first (random is the most fun to watch)
- [ ] Photos with a `Draft` or `Approved` post are excluded, so they aren't picked twice
- [ ] Returns null when everything has been posted, and the pipeline logs a friendly "out of photos" warning
- [ ] `AddPostAsync` / `UpdatePostAsync` implemented

### PP-205 · Repository tests
**Type:** Task · **Points:** 3 · **Depends on:** PP-204

**Acceptance criteria**
- [ ] Tests use SQLite **in-memory** (`DataSource=:memory:` with the connection kept open), not the EF InMemory provider, so behaviour matches production
- [ ] Covers: insert new, move or rename, mark unavailable, next-photo excludes posted and drafted photos, null when exhausted

---

## E3 · Caption generation

### PP-301 · Spike: choose the LLM provider and model
**Type:** Spike · **Points:** 2 · **Time box:** 1 evening

**Questions to answer**
- [ ] Which provider and model has good vision quality at low cost? Estimate the cost per caption at your posting rate.
- [ ] Official .NET SDK, or plain `HttpClient` against the REST API? (Raw HTTP teaches more. An SDK is less code.)
- [ ] Max image size and accepted formats. Does HEIC work, or do you need to convert?
- [ ] Does it support structured or JSON output so you can parse reliably?

**Output:** a short note in `docs/decisions/001-llm-provider.md` and `Caption:Model` filled in.

### PP-302 · Design the prompt and output contract
**Type:** Task · **Points:** 2 · **Depends on:** PP-301

**Acceptance criteria**
- [ ] Prompt lives in its own file (e.g. `Prompts/caption.txt`, copied to output) or a constant, not inline in the HTTP code
- [ ] Asks for JSON matching `GeneratedCaption`: `{ "caption": "...", "hashtags": ["..."] }`
- [ ] Includes `CaptionOptions.StyleGuide` and `MaxHashtags`
- [ ] Tested by hand in the provider's web console with 5 of your photos, with results noted

**Notes:** "Trending" hashtags: don't scrape anything. Let the model suggest relevant, commonly used tags. Scraping is a ToS risk and isn't needed.

### PP-303 · Implement `ICaptionGenerator`
**Type:** Story · **Points:** 5 · **Depends on:** PP-302

**Acceptance criteria**
- [ ] `Services/LlmCaptionGenerator.cs` registered as a typed `HttpClient` (`AddHttpClient<ICaptionGenerator, LlmCaptionGenerator>`) or wraps the SDK client
- [ ] Reads the image, base64-encodes it, sends it with the prompt, and parses the JSON response into `GeneratedCaption`
- [ ] API key comes from `CaptionOptions` (user-secrets), and `.ValidateOnStart()` is turned on for `CaptionOptions` in `Program.cs`
- [ ] Logs token usage if the API returns it
- [ ] Test with a fake `HttpMessageHandler` that returns canned JSON. No real API calls in unit tests.

### PP-304 · Validate and clean up the model output
**Type:** Story · **Points:** 2 · **Depends on:** PP-303

**Acceptance criteria**
- [ ] Strips a leading `#` from hashtags, removes duplicates (case-insensitive), drops tags with spaces or punctuation
- [ ] Caps at `MaxHashtags` (Instagram's hard limit is 30)
- [ ] Caption plus hashtags fits within 2,200 characters (Instagram's limit). Truncate the caption sensibly if not.
- [ ] Malformed JSON gets one retry, then fails with a clear exception
- [ ] Pure function, fully unit tested (this is the most testable code in the app)

### PP-305 · Downscale images before sending
**Type:** Task · **Points:** 2 · **Depends on:** PP-303

**Acceptance criteria**
- [ ] Use `SixLabors.ImageSharp` (check its licence terms for your use) or another library to resize to the provider's recommended max (e.g. long edge ≤ 1568 px) and re-encode as JPEG
- [ ] Respects EXIF orientation (`image.Mutate(x => x.AutoOrient())`)
- [ ] Log the before and after size to see the savings

**Notes:** HEIC support is limited in most .NET imaging libraries. Either drop `.heic` from `Extensions` for now or note it as a known gap.

### PP-306 · `--dry-run` command
**Type:** Story · **Points:** 2 · **Depends on:** PP-304, PP-204

As the developer, I can run `dotnet run -- --dry-run` to caption the next photo and print it, without the scheduler.

**Acceptance criteria**
- [ ] With `--dry-run`, the app scans, syncs, picks a photo, generates a caption, prints the path, caption and hashtags, then exits
- [ ] Doesn't write a `PostRecord`
- [ ] Without the flag, the normal worker runs

**Hints:** Check `args` in `Program.cs` and either skip `AddHostedService<Worker>()` or run the pipeline directly after `Build()`. A one-flag check is fine. `System.CommandLine` is overkill for now.

**🎉 Checkpoint:** this is the first "wow, it works" moment. Run it on 20 photos and tune the prompt.

---

## E4 · Scheduling

### PP-401 · Finalise schedule configuration
**Type:** Task · **Points:** 1 · **Depends on:** none

**Acceptance criteria**
- [ ] `Schedule:TimeZoneId` set to your zone (`"Eastern Standard Time"`, `"Pacific Standard Time"`, etc. On .NET 6+ on Windows, IANA ids like `"America/New_York"` work too.)
- [ ] Add a custom validation that `TimeZoneId` actually resolves (`TimeZoneInfo.TryFindSystemTimeZoneById`). Look at `IValidateOptions<ScheduleOptions>`.
- [ ] Test for the custom validator

### PP-402 · Schedule calculator
**Type:** Story · **Points:** 5 · **Depends on:** PP-401

**Acceptance criteria**
- [ ] `Scheduling/ScheduleCalculator.cs` implements `GetNextRun(now)`: the next configured time today (if still ahead) or the first one tomorrow, in the configured zone, returned as a `DateTimeOffset`
- [ ] Applies random jitter of ±`MaxJitterMinutes`. Inject `Random` or a jitter function so tests are deterministic.
- [ ] Unit tests: before the first slot, between slots, after the last slot (rolls to tomorrow), unsorted config times, **DST transitions** (spring forward and fall back)
- [ ] `Worker` uses it: `await Task.Delay(next - now, time, stoppingToken)`, replacing `PlaceholderInterval`

**Learn:** `Microsoft.Extensions.TimeProvider.Testing` gives you `FakeTimeProvider`. You can advance time in tests and watch the Worker fire without waiting.

### PP-403 · Post pipeline (logging only)
**Type:** Story · **Points:** 3 · **Depends on:** PP-402, PP-306

**Acceptance criteria**
- [ ] `Pipeline/PostPipeline.cs` implements `IPostPipeline.RunOnceAsync`: scan, sync, pick, caption, then save a `PostRecord` with `Status = Draft`
- [ ] `Worker` creates a DI scope per run (`IServiceScopeFactory`), because the pipeline uses scoped DbContext
- [ ] Exceptions in one run are caught and logged in `Worker`, and the loop continues
- [ ] Refactor `--dry-run` to reuse the pipeline (add a "don't save" flag or split the steps)
- [ ] Unit test the pipeline with fakes for every interface

### PP-404 · Missed-run and overlap behaviour
**Type:** Story · **Points:** 2 · **Depends on:** PP-403

**Acceptance criteria**
- [ ] Decide and document: if the PC was off at 9:00 and boots at 9:40, do you post (catch up) or skip to the next slot? (Suggestion: catch up if within N minutes, otherwise skip.)
- [ ] No double posting if the app restarts right after a run. Check "already posted in this slot?" against the DB.
- [ ] Tests for both

---

## E5 · Instagram

### PP-501 · Spike: Meta developer setup and current auth flow
**Type:** Spike · **Points:** 3 · **No code** · Start early, in Sprint 1

**Acceptance criteria**
- [ ] A **throwaway test Instagram account** created and switched to a Creator or Business professional account
- [ ] Meta developer app created, with the Instagram product added
- [ ] Test account added under App Roles (Development Mode is enough for your own accounts, so App Review isn't needed)
- [ ] **Read the current Meta docs** and record which flow you're using ("Instagram API with Instagram Login" vs the Facebook Page–linked Graph API), the base URL and version, and the required permissions or scopes. This has changed several times, so trust today's docs over any guide, this one included.
- [ ] Long-lived token generated, and the account id recorded
- [ ] Both stored in user-secrets (`Instagram:AccessToken`, `Instagram:AccountId`)
- [ ] Notes in `docs/decisions/002-instagram-auth.md`

### PP-502 · Prepare images to Instagram's spec
**Type:** Story · **Points:** 3 · **Depends on:** PP-305

**Acceptance criteria**
- [ ] JPEG output
- [ ] Aspect ratio clamped to between 4:5 (portrait) and 1.91:1 (landscape), centre-cropped if outside that range
- [ ] Width within Instagram's limits (check current docs, historically 320–1440 px)
- [ ] File size under the documented max
- [ ] Writes to a temp file and returns the path. Doesn't modify the original.
- [ ] Unit tests using small generated images of different shapes

### PP-503 · Temporary public image hosting (Azure Blob)
**Type:** Story · **Points:** 3 · **Depends on:** PP-001

**Acceptance criteria**
- [ ] Storage account and private container created (free tier or pennies)
- [ ] `Services/AzureBlobImageHost.cs` implements `IImageHost` using `Azure.Storage.Blobs`
- [ ] `UploadAsync` uploads with a unique name and returns a **read-only SAS URL** that expires within an hour or so
- [ ] `DeleteAsync` removes the blob
- [ ] Connection string in user-secrets
- [ ] Manual check: the SAS URL opens in a browser, and after expiry it returns an error
- [ ] Bonus: develop locally against **Azurite** (the storage emulator). Note that Instagram can't reach Azurite, so the end-to-end test still needs real Azure.

### PP-504 · InstagramPublisher
**Type:** Story · **Points:** 5 · **Depends on:** PP-501, PP-502, PP-503

**Acceptance criteria**
- [ ] `Publishers/InstagramPublisher.cs` implements `ISocialPublisher` with `Platform => "Instagram"`, as a typed `HttpClient`
- [ ] Flow: prepare image (PP-502), upload (PP-503), **create media container** (`POST /{account-id}/media` with `image_url` and `caption`), **poll the container status** until `FINISHED`, **publish** (`POST /{account-id}/media_publish`), and always delete the hosted image in a `finally`
- [ ] Returns `PublishResult` with the media id, or the API error message
- [ ] `.ValidateOnStart()` turned on for `InstagramOptions`
- [ ] Unit tests with a fake `HttpMessageHandler` for the happy path, container error, and publish error
- [ ] The access token never appears in logs (check your HTTP logging)

### PP-505 · Resilience: retries and timeouts
**Type:** Task · **Points:** 2 · **Depends on:** PP-504

**Acceptance criteria**
- [ ] `Microsoft.Extensions.Http.Resilience` with `.AddStandardResilienceHandler()` on the Instagram and LLM HTTP clients
- [ ] **Not** blindly retrying the publish call, which could double post. Retry the safe calls only, or check whether it already published before retrying.
- [ ] Pipeline marks the `PostRecord` as `Failed` with the error, and the photo becomes eligible again next slot (or after N failures it's skipped, your choice)

### PP-506 · Access-token refresh
**Type:** Story · **Points:** 3 · **Depends on:** PP-504

**Acceptance criteria**
- [ ] Look up the current refresh endpoint and rules (long-lived tokens are ~60 days and refreshable before expiry)
- [ ] Token is stored somewhere the app can **update** (user-secrets is read-only at runtime). Options: a DB table, or a protected local file via the `Microsoft.AspNetCore.DataProtection` package.
- [ ] Refreshes when the token is within ~10 days of expiry, checked once per day
- [ ] Logs a loud warning if refresh fails

### PP-507 · End-to-end test post
**Type:** Story · **Points:** 2 · **Depends on:** PP-504, PP-505

**Acceptance criteria**
- [ ] Pipeline runs with publishing on, against the **test account only**
- [ ] One photo posted with the generated caption, `PostRecord` is `Published` with `ExternalPostId`, and the blob is deleted
- [ ] Run 3–5 posts over a day on a real schedule
- [ ] Note any surprises in the ticket

**🎉 Checkpoint:** it posts by itself.

---

## E6 · Approval gate

### PP-601 · Spike: choose the approval UX
**Type:** Spike · **Points:** 1

Options, in rough order of effort:
1. **Minimal API + one HTML page** on the same host (`WebApplication` instead of `Host`). Probably the best fit, and good résumé value.
2. A console prompt (doesn't work once it runs as a service).
3. Blazor Server (more to learn, nicer UI).
4. Email or Telegram "reply YES" (fun but fiddly).

**Output:** decision note. The tickets below assume option 1.

### PP-602 · Drafts review page
**Type:** Story · **Points:** 5 · **Depends on:** PP-601, PP-403

**Acceptance criteria**
- [ ] Switch `Program.cs` from `Host.CreateApplicationBuilder` to `WebApplication.CreateBuilder` (the Worker still registers as a hosted service, and the SDK changes to `Microsoft.NET.Sdk.Web`)
- [ ] `GET /drafts` lists Draft posts with a thumbnail, caption and hashtags
- [ ] `GET /photos/{id}` serves the image (only photos that are in the DB, to block path traversal)
- [ ] Approve, Reject, and Edit caption actions (`POST /drafts/{id}/approve` etc.)
- [ ] **Bound to localhost only** (`http://localhost:5080`). There's no auth, so it must never be exposed.

### PP-603 · Pipeline respects approval
**Type:** Story · **Points:** 3 · **Depends on:** PP-602

**Acceptance criteria**
- [ ] New option `Pipeline:RequireApproval` (default `true`)
- [ ] When true, each slot publishes the oldest `Approved` post. If there's none, it creates a new Draft and logs "waiting for approval".
- [ ] When false, it creates and publishes immediately (the old behaviour)
- [ ] Keep a small buffer: generate drafts ahead (e.g. keep 3 pending) so there's always something to approve
- [ ] Tests for both modes

---

## E7 · Go live & operate

### PP-701 · Logging you can read later
**Type:** Task · **Points:** 2

**Acceptance criteria**
- [ ] Logs written to a rolling file (Serilog with `Serilog.Extensions.Hosting` and `Serilog.Sinks.File`, or similar)
- [ ] Each pipeline run has a correlation id or log scope so one run's lines can be grouped
- [ ] No secrets or full tokens in logs

### PP-702 · Failure alerting
**Type:** Story · **Points:** 2 · **Depends on:** PP-505

**Acceptance criteria**
- [ ] On a failed post, a failed token refresh, or running out of photos, send yourself a notification (email via SMTP, a Discord webhook, or ntfy.sh are all simple)
- [ ] An `INotifier` interface, so the channel can be swapped

### PP-703 · Run as a Windows Service
**Type:** Task · **Points:** 2 · **Depends on:** PP-603

**Acceptance criteria**
- [ ] `Microsoft.Extensions.Hosting.WindowsServices` added, with `builder.Services.AddWindowsService()`
- [ ] `dotnet publish -c Release -o <folder>` and install with `sc.exe create PhotoPoster binPath=...`
- [ ] Starts automatically on boot. Check it survives a reboot.
- [ ] Secrets provided via environment variables or the protected store from PP-506, because user-secrets only load in Development
- [ ] SQLite DB and log paths are absolute, under e.g. `%ProgramData%\PhotoPoster\`, not the working directory (services start in `System32`)

### PP-704 · Switch to the real account
**Type:** Task · **Points:** 1 · **Depends on:** PP-507, PP-703

**Acceptance criteria**
- [ ] Real account converted to Creator, added to the Meta app, token generated
- [ ] `RequireApproval = true` for at least the first 2 weeks
- [ ] Short runbook in `docs/RUNBOOK.md`: how to stop it, rotate the token, add photos, and read the logs

### PP-705 · Portfolio polish
**Type:** Task · **Points:** 2

**Acceptance criteria**
- [ ] README rewritten for a stranger: what it does, a screenshot of the approval page, an architecture diagram, how to run, and the design decisions (link `docs/decisions/`)
- [ ] Test coverage report in CI (coverlet is already referenced)
- [ ] Repo made public (after a final secrets scan of the **full git history**, e.g. with `gitleaks`)

---

## E8 · Stretch goals (pick any, in any order)

| ID | Ticket | Pts | Notes |
|---|---|---|---|
| PP-801 | Bluesky publisher | 3 | AT Protocol, app password, direct blob upload. The easiest one, and the best test of `ISocialPublisher`. |
| PP-802 | Multi-platform pipeline | 5 | Pipeline loops over all registered `ISocialPublisher`s. Per-platform caption tailoring and per-platform schedules. |
| PP-803 | Tumblr publisher | 3 | REST API v2, OAuth. Tags are a native field. |
| PP-804 | Reddit publisher | 5 | OAuth. Subreddit and flair instead of hashtags. Respect each subreddit's rules. |
| PP-805 | Analytics pull-back | 3 | Store likes and comments per post, and show them on the drafts page. |
| PP-806 | Avoid similar photos back to back | 5 | Perceptual hash (pHash) and distance threshold. |
| PP-807 | Seasonal weighting | 2 | Use EXIF date-taken to favour photos from this time of year. |
| PP-808 | Continuous deployment | 3 | Publish an artifact from CI, deploy to the PC or a small VM. |

---

## Decisions log

Keep short notes in `docs/decisions/NNN-title.md` whenever a spike or a "decide and document" criterion asks for one: context, options, decision, why. Future you (and interviewers) will thank you.
