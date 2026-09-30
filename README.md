# IoT Sensor Monitoring & Stateful Rule Engine

A simple and fast backend service to process sensor data, run rules, generate alerts, and calculate time-based stats.

## How to Run and Test
**Requirements:** .NET 9 SDK

To run the application:
```bash
dotnet run --project src/IoTMonitoring.Api
```

Once the app is running, you can see the API in Swagger:
* HTTPS: `https://localhost:7165/swagger`
* HTTP: `http://localhost:5186/swagger`

To run the tests:
```bash
dotnet test
```

## Architecture & Design Decisions
This project uses **Clean Architecture**. The main logic (rules, validation, alerts) is inside the Domain and Application layers. This means our core code does not depend on the database or the web API.

**Database Choice (Trade-off):** 
For real-world IoT systems, a time-series database like InfluxDB or TimescaleDB is the standard choice. However, I used **SQLite** here because it is portable and zero-setup. Reviewers don't need to install docker or external databases to run this task.

## Handling Messy Data & Safe Re-runs

**Handling Duplicates (First-wins):** 
If two records have the exact same `(deviceId, metric, ts, seq)`, we keep the first one and ignore the others. I used a `HashSet` in memory because it provides blazing-fast O(1) lookups.

**Safe Re-runs (Idempotency):** 
Processing the same `readings.jsonl` file twice will not create duplicate readings, alerts, or rule results. 
* **Keying Strategy:** Before saving to the database, the system uses the composite key `(deviceId, metric, ts, seq)` to query existing records within a specific time boundary (min/max timestamp). It filters out the existing keys in memory and only inserts the new, completely unique records. The same idempotent logic is applied when saving Alerts.

## Rule Engine & Classification Policy
Rules are loaded from the `rules.json` file when the app starts. I used the **Strategy Pattern**. 
* **Extensibility:** If you want to add a new rule operator, you do not need to modify existing code. You just create a new class implementing the `IRuleOperatorStrategy` interface and register it in the DI container.

**Classification Policy:**
Every valid reading is checked against the rules. If it passes, it is "Acceptable". If it breaks a rule, it is marked as "Unacceptable" and the exact rule and reason are recorded. Invalid or duplicate records are rejected early and never reach the rule engine.
* **No Rule Policy:** If a reading has no applicable enabled rules, it defaults to being "Acceptable".

**Out-of-Order Data for SustainedAbove (Trade-off):**
For the time-based rules, I chose the **"Batch (Sort then Scan)"** method. 
First, the app groups the data by device and metric, and explicitly sorts them by time. This uses a bit more memory, but it makes the code much simpler and handles out-of-order data perfectly. 
* **Duration Window:** The duration is measured strictly in **Event Time** (using the reading's `Ts` field), not the system processing time. The engine remembers when the violation starts (`episodeStartTs`) and stops the alert when the value goes back to normal.

## Alerts and Cooldown Policy
When a rule like `SustainedAbove` fails, it creates only one Domain Alert with a `StartTs` and `EndTs`. It does not mark every reading as unacceptable.

**5-Minute Cooldown:** 
If a new alert happens for the same device and rule within 5 minutes of the last alert's end time, the system ignores it. This domain-level deduplication stops the system from making too many spam alerts.

## Aggregation API
This API only uses "Acceptable" (good) readings to calculate stats. Bad data is ignored.

**Time Buckets (Trade-off):** 
SQLite is not very good at grouping time (like 5-minute buckets). Instead of hard SQL queries, I did the math in C# using integer division on `Ticks`. It groups the data very fast in memory and keeps the code cross-database compatible.

**Empty Buckets:** 
Since I use the `GroupBy` method in C#, if a time bucket has no data, it naturally doesn't show up in the API result.

## AI Usage
I did not use coding agents like Claude, Cursor, or Copilot Workspace to write the code for me. I only used Google Gemini as a chat assistant to discuss Clean Architecture ideas, find the C# math trick for time buckets, and review unit test edge cases.