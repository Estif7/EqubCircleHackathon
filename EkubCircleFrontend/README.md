# EkubCircle Angular Frontend

Angular frontend for `EkubCircleBackend-Clean-Fixed-v4`.

## Requirements
- Node.js 20+ (Node 22 recommended)
- npm 10+
- Backend running on `http://localhost:5000`

## Run

```bash
npm install
npm start
```

Open `http://localhost:4200`.

The Angular dev proxy forwards `/api/*` to `http://localhost:5000`, so no CORS change is required for local development.

## Included flows
- Register
- Simulated Fayda OTP verification
- Login + JWT session
- Dashboard / My Circles
- Available circles search/filter
- Create circle
- Join circle
- Organizer member management
- Start circle / fixed payout order
- Current round and pot
- Record payments
- Payout current round
- Round history
- Profile/session view

## Backend assumptions
This UI intentionally targets the exact DTO names and routes in `EkubCircleBackend-Clean-Fixed-v4`.

The current backend does not expose a dedicated profile endpoint, so Profile uses the authenticated JWT/session information already returned by login/OTP verification.
