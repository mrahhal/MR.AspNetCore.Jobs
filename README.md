# MR.AspNetCore.Jobs

A lightweight inprocess background processing library for Asp.Net Core.

## Overview

Although a lot of the code was inspired from Hangfire this is a complete rewrite. I'll refer to this library as "Jobs".

Jobs integrates well with the Asp.Net Core and its dependency injection system with the following features:

- Provides a way to schedule 3 types of background jobs:
    - Fire and forget: These are jobs that need to be executed in the background some time later.
    - Delayed: These are jobs that need to be executed after a certain delay.
    - Cron: These are cron jobs that execute regularly at certain points in time (for example daily or monthly).
- Jobs are persisted so that whenever you schedule a job it's guaranteed to be executed at some point in the future.
