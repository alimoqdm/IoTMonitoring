# IoT Sensor Ingestion & Stateful Rule Engine

## 📌 Executive Summary
This project implements a backend service for ingesting, cleaning, and evaluating IoT sensor data. It features a custom Rule Engine capable of handling stateful evaluations (`SustainedAbove`) and includes an aggregation API. The system is built using **.NET 8** and **SQLite**, strictly following **Clean Architecture** principles.

## 🏗️ Architecture & Design Decisions
- **Clean Architecture:** The solution is divided into `Domain`, `Application`, `Infrastructure`, and `Api` layers. The core business logic (Validation, Rule Applicability, Rule Evaluation, Alerting) resides purely in the `Domain` and `Application` layers, completely independent of the database or web frameworks.
- **Data Ingestion:** Implemented via a streaming read (`StreamReader`) to handle large files efficiently without memory limits. Messy data (malformed JSON, invalid types like "NaN", missing required fields) are caught gracefully using `try-catch` blocks and validation checks, counting them as `InvalidRecords` without crashing the app.
- **Rule Engine (Strategy Pattern):** Extensibility is achieved using the **Strategy Pattern**. New operators can be added by simply creating a new class implementing `IRuleOperatorStrategy` without modifying the core engine logic (Open/Closed Principle).

## ⚙️ Messy Data, Out-of-Order & Deduplication
- **Deduplication:** A **"First-wins"** policy is enforced in memory using a `HashSet` based on the composite key `(deviceId, metric, ts, seq)`.
- **Out-of-Order Handling (Sort then Scan):** To correctly evaluate stateful rules on out-of-order data, the engine groups readings by `(DeviceId, Metric)` and explicitly sorts them by timestamp (`Ts`) before passing them to the Rule Operators. This guarantees predictable event-time windowing.

## 🚨 Stateful Operator (`SustainedAbove`) & Alerting
- **State Management:** The `SustainedAboveStrategy` scans the chronologically sorted readings, explicitly tracking `episodeStartTs`. The window begins when the threshold is crossed and ends when the value drops below it (or the stream ends).
- **Cooldown Policy:** Alerts are deduplicated at the domain level. If an alert is generated for a `(Rule, Device, Metric)`, subsequent violations within a **5-minute window** from the `EndTs` of the previous alert are ignored.

## 🛡️ Idempotent Processing
Processing the same `readings.jsonl` multiple times will not duplicate readings, violations, or alerts.
- **Database Level:** Composite Unique Indexes are configured in EF Core (`IX_Unique_SensorReading` and `IX_Unique_Alert`).
- **Application Level (Delta Check):** During batch inserts, the repository fetches existing keys within the timestamp range and filters out duplicates before calling `AddRangeAsync`.

## 📊 Aggregation API
- **Endpoint:** `GET /api/aggregation`
- Fetches strictly `Acceptable` readings using `.AsNoTracking()` for high performance.
- Aggregates metrics into requested time buckets (`bucketSizeSeconds`) in-memory. Empty buckets are intentionally omitted to keep the payload clean and meaningful.

## 🤖 AI Usage Disclosure
* AI Assistance: Google Gemini was used as a pair-programming thought partner to brainstorm architectural structure (Clean Architecture layers), refine the Strategy Pattern for the Rule Engine, and optimize EF Core's idempotent insert logic.