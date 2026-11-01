OO Project – Financial Backtester (Backend + Frontend)

## Team

Group project (EFREI, I2-ITF 2025): Matteo Liagre, Lilou Constantin, Jonathan Ren, David Khersis, Tristan Brahy, Myriam Fossouo, Raph Takam Tene.

- Raph Takam Tene: C# / .NET 8 backend developer (backtesting engine).


This repository contains a minimalist financial backtesting backend (ASP.NET Core 8) and a React frontend (Vite + Tailwind) to run simple strategies (SMA, Buy & Hold), visualize performance, and inspect trades.

## Repo layout

```
Project.sln
backend/
  BacktestingAPI/
    Controllers/
    Services/
    Models/
    Data/
    Strategies/
    Program.cs
    appsettings.json
frontend/
  index.html
  src/
  package.json
  vite.config.js
  tailwind.config.js
  .env
```

## Prerequisites

- Windows, macOS, or Linux
- .NET 8 SDK
- Node.js 18+ and npm
- Alpha Vantage API key (optional for first run — minimal AAPL data is seeded)

---

## Backend (ASP.NET Core)

The backend provides:

- Historical data fetching (Alpha Vantage with caching in SQLite)
- Strategy execution (SMA crossover, Buy & Hold)
- Backtest metrics (return, volatility, Sharpe, drawdown, etc.)

### Configure

Edit `backend/BacktestingAPI/appsettings.json`:

- Set `AlphaVantage.ApiKey` to your key (get one free at https://www.alphavantage.co/support/#api-key)
- `ConnectionStrings.DefaultConnection` defaults to a local SQLite file `backtesting.db`

### Run

```powershell
cd backend/BacktestingAPI
dotnet build
dotnet run
```

Notes:
- The API typically listens on both HTTP and HTTPS (ports are shown in the console on start, e.g., https://localhost:7000 and http://localhost:5000).
- Swagger UI is available in Development at the HTTPS URL: `https://localhost:<https-port>/swagger`.
- EF Core migrations are applied automatically on first run; a minimal AAPL sample (~30 days) is seeded.
- CORS is enabled for `http://localhost:3000` (the React dev server).

### API endpoints (quick reference)

- GET `/api/stockdata/symbols` — cached/supported symbols
- GET `/api/stockdata/{symbol}?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd` — OHLCV data (cache-first)
- GET `/api/stockdata/cache-status/{symbol}` — whether cached and last update time
- GET `/api/strategy/list` — strategy names and descriptions
- GET `/api/strategy/{strategyType}/parameters` — required parameters and defaults
- POST `/api/backtest/run` — run a backtest

Example body:

```json
{
  "symbol": "AAPL",
  "startDate": "2023-01-01",
  "endDate": "2024-01-01",
  "initialCapital": 10000,
  "strategyType": "SMA",
  "parameters": {
    "shortPeriod": 20,
    "longPeriod": 50
  }
}
```

Metric units: unless otherwise noted in code comments, percentage-like values may be provided in percent units (25 = 25%). Some implementations may return annualized return as a fraction (0.12 = 12%). When in doubt, check a sample payload from Swagger.

---

## Frontend (React + Vite + Tailwind)

The frontend provides a clean, data-focused UI to configure and run backtests, then visualize results.

### Configure

Set the backend URL for development in `frontend/.env`:

```
VITE_API_BASE_URL=http://localhost:5000
```

If your backend prints different ports, update the value (HTTPS also works, e.g., `https://localhost:7000`).

### Install and run

```powershell
cd frontend
npm install
npm run dev
```

Open the URL printed in the terminal (usually `http://localhost:3000`). If port 3000 is in use, Vite will select the next free port and display it.

---

## End-to-end tutorial (local test)

Follow these steps to run a complete backtest locally.

1) Start the backend

```powershell
cd backend/BacktestingAPI
dotnet run
```

Take note of the base URL(s) printed (e.g., `https://localhost:7000`, `http://localhost:5000`). Leave this running.

2) Configure the frontend to point at your backend

Edit `frontend/.env` and set:

```
VITE_API_BASE_URL=<your backend base URL>
```

Examples:
- `VITE_API_BASE_URL=http://localhost:5000`
- `VITE_API_BASE_URL=https://localhost:7000`

3) Start the frontend

```powershell
cd frontend
npm install
npm run dev
```

Open the printed URL (e.g., `http://localhost:3000`).

4) Run your first backtest in the UI

- Stock Symbol: `AAPL`
- Initial Capital: `10000`
- Date Range: choose a recent 1-year span or the dates you know are cached/seeded
- Strategy: `SMA` (defaults short=20, long=50) or `Buy & Hold`
- Click “Run Backtest”

You should see:
- Metric cards (Total Return, Annualized Return, Volatility, Sharpe, Max Drawdown, Win Rate)
- A portfolio value line chart (blue) with an initial capital reference line
- A trades table with BUY/SELL rows

5) Try another strategy or date range

- Use the “Run New Backtest” button in the results header
- Switch to `Buy & Hold` to compare against SMA

---

## Troubleshooting

- CORS/mixed content: ensure the backend allows origin `http://localhost:3000`. HTTPS backend + HTTP frontend is fine if CORS is configured.
- Wrong backend URL: update `frontend/.env` to the exact base URL printed by the backend.
- Port 3000 already in use: Vite will pick the next free port automatically. Or close other dev servers.
- Alpha Vantage rate limit: the backend uses a cache; if limits are hit, try a cached range or wait and retry.
- No data available: pick a date range that includes cached/seeded AAPL data or provide an Alpha Vantage API key and allow the backend to fetch.

---

## Notes for developers

- Backend models and controllers are designed with clear contracts; errors return helpful messages and proper HTTP status codes.
- Frontend uses Axios with unified error handling and a minimalist Tailwind UI. Charts are built with Recharts for clarity and performance.
- If metric units appear inconsistent (e.g., annualized return), check the backend payload in Swagger to confirm unit conventions.
