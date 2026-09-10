# DROPLINK Google Play Data Safety preparation

These notes must be checked against the final production build before submitting Play Console forms.

## Customer app
Potentially collected/processed:
- Name and phone number — account and delivery coordination
- Pickup/drop-off addresses — core delivery functionality
- Sender/receiver names and phone numbers — delivery coordination
- Delivery notes/package details — core functionality
- Trip chat messages — customer/driver communication

## Driver app
Potentially collected/processed:
- Name, phone and optional email — account management
- Precise/approximate location while app is in use — dispatch/tracking
- Trip chat messages — customer/driver communication
- Proof-of-delivery photo, recipient name and delivery note — delivery verification

## Security/behavior
- HTTPS API only
- Passwords stored as hashes on the server
- API session tokens stored on device local storage
- No advertising SDK is included in this source package
- No background-location permission is declared

Do not claim data is not collected if the deployed backend stores it. Complete Play Console answers from the actual production configuration.
