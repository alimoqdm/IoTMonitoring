# IoT Sensor Monitoring & Stateful Rule Engine

A lightweight, robust backend service designed to ingest messy sensor data, evaluate stateful rules, generate alerts, and provide time-based aggregations. 

## How to Run and Test

**Prerequisites:** .NET 9 SDK

To run the application and start the ingestion process:

    dotnet run --project src/IoTMonitoring.Api

Once the application is running, the Aggregation API is accessible via Swagger at:
- HTTPS: https://localhost:7165/swagger
- HTTP: http://localhost:5186/swagger

To run the test suite:

    dotnet test

## Architecture & Design Decisions
The project is built using **Clean Architecture**. The core business logic (validation, deduplication, stateful rules, classification) lives entirely in the `Domain` and `Application` layers. This ensures the engine remains completely agnostic of the database or HTTP framework. 

**Storage Trade-off:** I chose **SQLite** because it is perfect for a self-contained, portable service. While it sacrifices the high concurrency of dedicated databases like PostgreSQL, it provides a zero-setup experience for reviewers and fits the bounded scope of this task perfectly.

## Messy Data & Idempotency
* **Duplicate Policy (First-wins):** When a reading has the exact same (deviceId, metric, ts, seq) as another, the first one encountered is kept, and subsequent ones are rejected as duplicates. I used an in-memory `HashSet` for O(1) lookup speed.
* **Idempotent Re-runs:** Processing the same `readings.jsonl` file multiple times is perfectly safe. Before persisting, the system performs a "delta check" by fetching existing keys within a bounding-box time range, filtering out what has already been stored.

## Rule Engine & SustainedAbove Strategy
Rules are loaded as seed data from `rules.json` at startup. The engine utilizes the **Strategy Pattern**, making it highly extensible. Adding a new operator strictly adheres to the Open/Closed Principle: simply create a new class implementing `IRuleOperatorStrategy`.

**Handling Out-of-Order Data (Trade-off):**
For the stateful `SustainedAbove` rule, I chose the **"Batch (Sort then Scan)"** approach over a streaming state machine.
* *Why?* While it consumes slightly more memory to group and sort readings chronologically per device/metric, it drastically reduces code complexity and guarantees 100% predictable event-time evaluation for late or out-of-order arrivals.
* The engine tracks `episodeStartTs`. The violation window starts when the metric crosses the threshold and ends as soon as it drops back below it.

## Alerting & Cooldown Policy
* A sustained rule violation produces a single Domain Alert with exact `StartTs` and `EndTs` boundaries, mapping one real-world episode to one alert.
* **Cooldown Deduplication:** I enforced a 5-minute cooldown policy at the domain level. If a new alert for the same (Rule, Device, Metric) occurs within 5 minutes of the previous alert's end time, it is intentionally ignored to prevent alert fatigue.

## Aggregation API
* The `/api/aggregation` endpoint strictly aggregates **Acceptable** readings. Invalid or rule-violating readings are excluded from the statistics.
* **Bucketing Trade-off:** Instead of writing complex, database-specific SQL queries (which SQLite struggles with for time-bucketing), I used an integer division trick on C# `Ticks` to snap timestamps into exact buckets in-memory. This offloads work to the application layer but ensures blazing-fast, cross-database compatible bucketing.
* **Empty Buckets:** By using `GroupBy` on the existing data stream, time windows with no data are naturally omitted from the response.

## AI Usage Disclosure
I did not use any autonomous coding agents (such as Claude, Cursor, or Copilot Workspace) to write the logic for me. I solely used Google Gemini as an interactive pair-programming thought partner to discuss Clean Architecture boundaries, brainstorm the C# Ticks integer division trick for time-bucketing, and review edge cases for my unit tests.