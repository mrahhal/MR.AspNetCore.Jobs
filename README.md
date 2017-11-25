# MR.AspNetCore.Jobs

AppVeyor | Travis
---------|-------
[![Build status](https://img.shields.io/appveyor/ci/mrahhal/mr-aspnetcore-jobs/master.svg)](https://ci.appveyor.com/project/mrahhal/mr-aspnetcore-jobs) | [![Travis](https://img.shields.io/travis/mrahhal/MR.AspNetCore.Jobs/master.svg)](https://travis-ci.org/mrahhal/MR.AspNetCore.Jobs)

[![NuGet version](https://img.shields.io/nuget/v/MR.AspNetCore.Jobs.svg)](https://www.nuget.org/packages/MR.AspNetCore.Jobs)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

A background processing library for Asp.Net Core.

[CHANGELOG](CHANGELOG.md)

**Note that this is in development so incompatible changes can happen between minor versions. (Starting from `0.7.0`, the api is stabalizing and there'll probably be no breaking changes in the database schema)**

## Overview

A lot of the code was inspired from [Hangfire](https://github.com/HangfireIO/Hangfire) but this is a complete (and a more lightweight) rewrite. I'll refer to this library as "Jobs".

Jobs integrates well with Asp.Net Core and its dependency injection system with the following features:

- Provides a way to schedule 3 types of background jobs:
    - Fire and forget: These are jobs that need to be executed in the background some time later (preferably very soon).
    - Delayed: These are jobs that need to be executed after a certain delay (minimally).
    - Cron: These are cron jobs that execute regularly at certain points in time (for example daily or monthly).
- Jobs are persisted so that whenever you schedule a job it's guaranteed to be executed at some point in the future even if the application restarts and stays offline for days.
- Asynchronous processing pipeline: all jobs can be asynchronous.

## Adapters

- `MR.AspNetCore.Jobs.SqlServer`: Microsoft's Sql Server
- *`MR.AspNetCore.Jobs.PostgreSQL`: PostgreSQL [Coming Soon]*
- *`MR.AspNetCore.Jobs.MySql`: MySql [Coming Soon]*
- *`MR.AspNetCore.Jobs.Sqlite`: Sqlite [Coming Soon]*
- *`MR.AspNetCore.Jobs.Redis`: Redis [Coming Soon]*

## Getting started

### Configuration
```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    // Registers Jobs with an sql server adapter
    services.AddJobs(options => options.UseSqlServer("[my connection string]"));
}
```

```cs
public static async Task Main(string[] args)
{
    var host = BuildWebHost(args);

    await host.StartJobsAsync();

    host.Run();
}

public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>()
        .Build();
```

If you're not using latest C# version and therefore can't use `async Main`, you can simply do:

```cs
host.StartJobsAsync().GetAwaiter().GetResult();
```

Anywhere you want to enqueue a background job you use `IJobsManager`, use DI to get it injected:
```cs
public class HomeController : Controller
{
    private IJobsManager _jobsManager;

    public HomeController(IJobsManager jobsManager)
    {
        _jobsManager = jobsManager;
    }

    public async Task<IActionResult> Home()
    {
        await _jobsManager.EnqueueAsync(...);
        return View();
    }
}
```

### Fire and forget jobs
```cs
// Execute a static method.
await _jobsManager.EnqueueAsync(() => SomeStaticClass.SomeStaticMethod("foo"));

// Execute an instance method. FooService will be created using DI so it is injectable.
await _jobsManager.EnqueueAsync<FooService>(service => service.SomeMethod("foo"));
```

### Delayed jobs
```cs
// Execute after 1 minute.
await _jobsManager.EnqueueAsync(() => ..., TimeSpan.FromMinutes(1));
```

All methods (fire and forget + delayed) can be async (return Task) and they'll be correctly awaited.

### Cron jobs
First, we'll have to create a registry that describes all the cron jobs we want to run:

```cs
public class FooJob : IJob
{
    ILogger<FooJob> _logger;

    // This is injectable so make sure you add FooJob to DI.
    public FooJob(ILogger<FooJob> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync()
    {
        // Do stuff
        _logger.LogInformation("FooJob is executing!");
        return Task.FromResult(0);
    }
}

public class SomeCronJobRegistry : CronJobRegistry
{
    public SomeCronJobRegistry()
    {
        // Use RegisterJob to register cron jobs:
        // - FooJob should be added to DI because it will be injected when executing the job.
        // - Give the job a unique name.
        // - Use the Cron class to create various kinds of cron expressions.
        RegisterJob<FooJob>(nameof(FooJob), Cron.Minutely());
    }
}
```

> You can extend `JobSync` if your job is inherently synchronous.

Then, we tell Jobs to use this registry:
```cs
services.AddJobs(options =>
{
    options.UseSqlServer("[my connection string]");

    // Use the SomeCronJobRegistry.
    options.UseCronJobRegistry<SomeCronJobRegistry>();
});
```

After the processing server starts it will know when to execute cron jobs whenever needed without your intervention.

## Retrying behavior
All kinds of jobs can be given different retrying behaviors. For delayed jobs, you are required to implement your job inside a class that implements `IRetryable`, else the default behavior will be used. `IRetryable` has one property called `RetryBehavior` that returns an instance of `RetryBehavior` which will be used to determine whether and when to retry a failed job before marking it as failed. You can use `RetryBehavior.DefaultRetry`, `RetryBehavior.NoRetry`, or your own subclass that overrides this behavior.

## Samples

- [`Basic`](/samples/Basic): implements Jobs in an Asp.Net Core app.
