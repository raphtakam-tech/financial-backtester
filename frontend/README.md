# Trading Strategy Backtester (Frontend)

Minimalist React (JavaScript) frontend for running financial strategy backtests against the provided .NET backend.

## Tech Stack

- React 18
- Vite (dev server on port 3000)
- Tailwind CSS
- Axios
- Recharts
- React Router v6
- Lucide React (optional icons)

## Getting Started

1. Ensure the backend is running and accessible (default expected at `http://localhost:5000`).
2. Configure the backend URL (if different) in a `.env` file:

```
VITE_API_BASE_URL=http://localhost:5000
```

3. Install dependencies and start the dev server:

```
npm install
npm run dev
```

The app will open at http://localhost:3000

## Project Structure

```
frontend/
├── public/
├── src/
│   ├── components/
│   │   ├── BacktestForm.jsx
│   │   ├── ResultsDisplay.jsx
│   │   ├── MetricsCard.jsx
│   │   ├── PortfolioChart.jsx
│   │   ├── TradesTable.jsx
│   │   └── LoadingSpinner.jsx
│   ├── services/
│   │   └── api.js
│   ├── pages/
│   │   ├── Home.jsx
│   │   └── Results.jsx
│   ├── utils/
│   │   └── format.js
│   ├── App.jsx
│   ├── index.css
│   └── index.js
├── index.html
├── package.json
├── postcss.config.js
├── tailwind.config.js
├── vite.config.js
└── .env
```

## Notes

- Backend endpoints used:
  - GET `/api/strategy/list`
  - GET `/api/strategy/{strategyType}/parameters`
  - POST `/api/backtest/run`
  - GET `/api/stockdata/cache-status/{symbol}`
- Error handling returns user-friendly messages for 400/404/429/5xx and network failures.
- Tailwind classes are used for a clean, professional UI.
- Charts are rendered with Recharts and optimized for readability.

## Troubleshooting

- CORS: Ensure the backend has CORS enabled for http://localhost:3000 during development.
- Ports: This app runs on port 3000; change `server.port` in `vite.config.js` if needed.
- Env vars: Vite only exposes vars prefixed with `VITE_`.
