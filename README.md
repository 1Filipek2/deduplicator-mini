# Deduplicator

This is my **first C# mini project**, built as a CLI tool for finding and removing duplicate files.  
The project is still small, but I plan to **gradually extend and improve it** as I learn more about C# and .NET.

Built on **.NET 10** and tested on **Fedora Linux**.

## What does it do?
Scans a directory, detects duplicate files, and lets you safely remove them after confirmation.

## How it works
Even though this is a beginner project, it already uses some solid concepts:

- Funnel-style algorithm: files are grouped by size first, then compared using SHA-256 hashes
- Asynchronous file processing to avoid blocking and reduce memory usage
- Console UI powered by Spectre.Console (tables, progress bars)
- Immutable data models using C# `record` types

## Installation & Run
Requires .NET 10 SDK.

```bash
git clone https://github.com/1Filipek2/deduplicator-mini
cd Deduplicator.Cli
dotnet run
