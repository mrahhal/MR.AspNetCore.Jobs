# MR.AspNetCore.Jobs

A lightweight inprocess background processing library for Asp.Net Core.

## Overview

A lot of the code was inspired from [Hangfire](https://github.com/HangfireIO/Hangfire) but this is a complete rewrite. I'll refer to this library as "Jobs".

Jobs integrates well with Asp.Net Core and its dependency injection system with the following features:

- Provides a way to schedule 3 types of background jobs:
    - Fire and forget: These are jobs that need to be executed in the background some time later (preferably very soon).
    - Delayed: These are jobs that need to be executed after a certain delay (minimally).
    - Cron: These are cron jobs that execute regularly at certain points in time (for example daily or monthly).
- Jobs are persisted so that whenever you schedule a job it's guaranteed to be executed at some point in the future even if the application restarts and stays offline for days.

## Getting started

### Configuration
```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    // Registers Jobs with an sql server adapter
    services.AddJobs(options => options.UseSqlServer("[my connection string]"));
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    ...
    // Starts the processing server
    app.UseJobs();
}
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

    public IActionResult Home()
    {
        _jobsManager.Enqueue(...);
        return View();
    }
}
```

### Fire and forget jobs:
```cs
// Execute a static method.
_jobsManager.Enqueue(() => SomeStaticClass.SomeStaticMethod("foo"));

// Execute an instance method. FooService will be created using DI so it is injectable.
_jobsManager.Enqueue<FooService>(service => service.SomeMethod("foo"));
```

### Delayed jobs:
```cs
// Execute after 1 minute.
_jobsManager.Enqueue(() => ..., TimeSpan.FromMinutes(1));
```

### Cron jobs:
First, we'll have to create a registry that describes all the cron jobs we want to run:

```cs
public class SomeCronJobRegistry : CronJobRegistry
{
    public SomeCronJobRegistry()
    {
        // Use RegisterJob to register cron jobs:
        // - FooJob should be added to DI because it will be injected when executing the job.
        // - Give the job a name.
        // - Use the Cron class to create various kinds of cron expressions.
        RegisterJob<FooJob>(nameof(FooJob), Cron.Minutely());
    }
}
```

Then, we tell Jobs to use this registry:
```cs
services.AddJobs(options =>
{
    options.UseSqlServer("[my connection string]");

    // Use the SomeCronJobRegistry.
    options.UseCronJobRegistry(new SomeCronJobRegistry());
});
```

After the processing server starts it will know when to execute cron jobs whenever needed without your intervention.
