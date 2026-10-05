# EkubCircle Mobile & Web App (Flutter)

A cross-platform Flutter application for **EkubCircle** — a transparent, verifiable Rotating Savings and Credit Association (ROSCA / Ekub) built on top of the EkubCircle ASP.NET Core REST API.

---

## 📱 Features

- **Authentication & Simulated Fayda Verification**:
  - Sign in with email or phone + password.
  - Registration with Ethiopian phone number, email, and Fayda FAN number.
  - 6-digit OTP verification flow with quick paste support.
  - Quick-fill test buttons for demo accounts (Tester, Organizer, Member).
  - Secure JWT bearer token session persistence with `shared_preferences`.

- **Interactive Dashboard**:
  - Personalized greeting with real-time stats (My circles, Open circles, Active circles).
  - **Next Payment Card**: Displays upcoming contribution dues, round number, and status.
  - **Next Payout Card**: Real-time status on whether the current round is ready for payout or awaiting member payments.
  - **My Circles**: Interactive cards detailing frequency, organizer, and member progress.
  - **Available Circles**: Real-time listing with instant search and frequency filtering (`Weekly` / `Monthly`).

- **Circle Management & Lifecycle**:
  - **Create Circle**: Configure name, contribution amount (ETB), frequency, member limit, and optional start date.
  - **Preparation Phase (Open)**:
    - Non-members can join open seats with one tap.
    - Organizers can invite members by phone or email.
    - Organizers can start the circle once the member limit is reached, locking the fixed payout sequence and generating rounds.
  - **Active Ekub Rounds**:
    - Real-time round tracking (Current Pot in ETB, Receiver Name, Contribution Amount).
    - Payment progress bar (`X of Y members paid`).
    - Members can pay their contribution with simulated instant verification.
    - Organizers can record payments for unpaid members.
    - Organizers can trigger payout once all member contributions are recorded.
  - **Completion Phase**:
    - Celebration banner once every member has received their payout.
  - **Audit History**:
    - Full round-by-round ledger with payout amounts and recipient names.

- **Dynamic API Server Switcher**:
  - Easily toggle or customize the API endpoint (`http://localhost:5000`, `http://10.0.2.2:5000` for Android emulator, or your PC LAN IP for physical mobile devices) directly from the login screen or Profile settings.

---

## 🚀 Getting Started

### Prerequisites

- Flutter SDK (v3.13+ / Dart 3.1+)
- ASP.NET Core Backend API running on `http://localhost:5000` (or `http://0.0.0.0:5000`)
- PostgreSQL database running (`EkubCircleDb`)

### Running the Flutter App

Navigate to the Flutter directory:

```bash
cd ekub_circle_mobile
```

Get packages:

```bash
flutter pub get
```

#### Run in Chrome (Web):

```bash
flutter run -d chrome
```

#### Run on Windows Desktop:

```bash
flutter run -d windows
```

#### Run on Android Emulator:

```bash
flutter run -d android
```
*(When running on Android Emulator, the app automatically defaults to `http://10.0.2.2:5000` to connect to your host machine's backend).*

#### Run Tests:

```bash
flutter test
```

#### Analyze Code:

```bash
flutter analyze
```

---

## 🛠️ Project Structure

```text
ekub_circle_mobile/
├── lib/
│   ├── models/
│   │   └── models.dart               # DTOs & JSON serialization
│   ├── services/
│   │   ├── api_service.dart          # HTTP client for all backend endpoints
│   │   └── auth_service.dart         # Authentication state & persistent session
│   ├── theme/
│   │   └── app_theme.dart            # Material 3 colors, buttons & typography
│   ├── widgets/
│   │   └── common_widgets.dart       # StatusBadge, UserAvatar, ServerSettingsDialog
│   ├── screens/
│   │   ├── login_screen.dart         # Sign-in & demo account quick fill
│   │   ├── register_screen.dart      # Registration form
│   │   ├── verify_otp_screen.dart    # 6-digit OTP verification screen
│   │   ├── dashboard_screen.dart     # Main overview & quick actions
│   │   ├── browse_circles_screen.dart# Search & filter available/my circles
│   │   ├── circle_details_screen.dart# Round tracking, payments & payouts
│   │   ├── create_circle_screen.dart # Form to create a new circle
│   │   ├── profile_screen.dart       # User details & API endpoint config
│   │   └── main_navigation_screen.dart # Bottom navigation bar
│   └── main.dart                     # App entry point & provider injection
├── test/
│   └── widget_test.dart              # App smoke tests
└── pubspec.yaml                      # Dependencies & assets
```

