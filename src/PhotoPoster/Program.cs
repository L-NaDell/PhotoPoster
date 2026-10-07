using PhotoPoster;
using PhotoPoster.Configuration;

var builder = Host.CreateApplicationBuilder(args);

// ---- Configuration ---------------------------------------------------------
// Each options class binds to a section of appsettings.json. Secrets (API keys,
// access tokens) go in user-secrets locally. See README "Secrets".
builder.Services.AddOptions<PhotoSourceOptions>()
    .Bind(builder.Configuration.GetSection(PhotoSourceOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<ScheduleOptions>()
    .Bind(builder.Configuration.GetSection(ScheduleOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// TODO(PP-303): add .ValidateOnStart() once captioning is implemented.
builder.Services.AddOptions<CaptionOptions>()
    .Bind(builder.Configuration.GetSection(CaptionOptions.SectionName))
    .ValidateDataAnnotations();

// TODO(PP-504): add .ValidateOnStart() once Instagram publishing is implemented.
builder.Services.AddOptions<InstagramOptions>()
    .Bind(builder.Configuration.GetSection(InstagramOptions.SectionName))
    .ValidateDataAnnotations();

// ---- Services --------------------------------------------------------------
// TimeProvider is injected instead of calling DateTime.Now, so scheduling
// logic can be unit tested with a fake clock (PP-402).
builder.Services.AddSingleton(TimeProvider.System);

// Register implementations here as you build them. Interfaces live next to
// where they're used: Services/, Publishers/, Scheduling/, Pipeline/.
//
// builder.Services.AddSingleton<IPhotoSource, FileSystemPhotoSource>();      // PP-101
// builder.Services.AddDbContext<PhotoPosterDbContext>(...);                    // PP-201
// builder.Services.AddScoped<IPhotoRepository, EfPhotoRepository>();           // PP-203
// builder.Services.AddHttpClient<ICaptionGenerator, LlmCaptionGenerator>();    // PP-303
// builder.Services.AddSingleton<IScheduleCalculator, ScheduleCalculator>();    // PP-402
// builder.Services.AddScoped<IPostPipeline, PostPipeline>();                   // PP-403
// builder.Services.AddSingleton<IImageHost, AzureBlobImageHost>();             // PP-503
// builder.Services.AddHttpClient<ISocialPublisher, InstagramPublisher>();      // PP-504

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
