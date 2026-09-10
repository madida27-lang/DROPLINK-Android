# DROPLINK — linked production preparation

This repository now contains the production-linked DROPLINK delivery system.

- `customer/` — customer Android app (`za.co.deutronomagroup.droplink`)
- `app/` — driver Android app (`za.co.deutronomagroup.droplink.driver`)
- `backend/` — PHP + MySQL shared API for customers, drivers and management
- `management-web/` — web controller/dispatch dashboard
- `store/` — Play Store privacy, listing and data-safety preparation

All three live components use the shared API endpoint:
`https://deutronomagroup.co.za/droplink-api/index.php`

The backend must be deployed and configured on Afrihost before production accounts and cross-device orders work.
