# DropLink Android — Test Build

This is a testable Android Studio project for the DropLink delivery platform.

Branding
- App name: DropLink
- Uses the Deutronoma Group symbol only (no Deutronoma wording inside the app logo)
- Main colors: navy blue, gold, white

Included test features
- Driver and Controller roles
- Demo login
- Driver dashboard and assigned deliveries
- Delivery details and customer calling
- Open pickup/drop-off in installed map app
- Simulated navigation/live tracking screen
- Proof of delivery photo picker, recipient name/signature note, and delivery note
- Complete delivery flow
- Controller dashboard
- Create and assign a delivery
- Search/manage all deliveries
- Local data persistence on the device

Important
This is a TEST BUILD. Delivery data is stored locally in WebView localStorage.
Real multi-device synchronization, secure accounts, server database, background GPS,
push notifications, controller-to-driver live assignment, and real-time tracking still
need the production backend.

How to test
1. Extract the ZIP.
2. Open Android Studio.
3. File > Open > select the DropLinkAndroid folder.
4. Allow Gradle sync to finish.
5. Connect an Android phone with USB debugging enabled, or start an emulator.
6. Press Run.
7. On the DropLink welcome screen choose Driver or Controller.
8. Demo login accepts any values.

Recommended production next step:
- Firebase Authentication + Firestore
- Background driver GPS service
- Google Maps SDK
- Firebase Cloud Messaging notifications
- Controller/driver account permissions
