# EkubCircle E2E Circle Fix v3

## Changes

1. **Circle start lock**
   - A circle can start only when the number of active members equals the configured member limit.
   - Backend enforces this rule even if the UI is bypassed.
   - Frontend disables **Start circle & lock order** until the circle is full and shows the remaining member count.

2. **Member dashboard next-action cards**
   - Added **Next Payment Circle** card.
   - Added **Next Payout Circle** card.
   - The dashboard reads these from `GET /api/dashboard/summary`.

3. **SignalR real-time updates**
   - Added `/hubs/notifications`.
   - Events: new circle, member joined, circle started, payment recorded, payout completed.
   - Dashboard refreshes automatically for real-time circle/payment changes.
   - Circle details refresh automatically when members/payments/payouts change.
   - Toast notifications are shown in the app.
   - Angular proxy supports WebSockets for `/hubs`.

4. **Simulated member payment**
   - Members can click **Pay my contribution** for their own membership.
   - Organizers can still record payment for any member.
   - Backend prevents a member from paying on behalf of another member.
   - Payment amount must equal the circle contribution.

5. **My Circles member counts**
   - Fixed the backend query so `MemberCount` includes all active memberships.
   - Dashboard cards now show the actual `members / member limit` value.

## Setup

Backend:
```bat
dotnet build
 dotnet run --project API
```

Frontend:
```bat
cd EkubCircleFrontend
npm install
npm start
```

Open `http://localhost:4200`.

No database reset is required for these changes.
