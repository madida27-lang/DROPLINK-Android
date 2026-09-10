# DROPLINK — Play Store launch checklist

## Linked production architecture
- Customer: `za.co.deutronomagroup.droplink`
- Driver: `za.co.deutronomagroup.droplink.driver`
- Management: web/desktop controller, not submitted to Play Store
- Shared API: `https://deutronomagroup.co.za/droplink-api/index.php`
- Shared MySQL database: configured on Afrihost

Customer orders, driver assignments, status changes, trip chat, driver location and proof of delivery all use the same API/database.

## 1. Deploy Afrihost backend
1. Create MySQL database + user in cPanel.
2. Import `backend/schema.sql` with phpMyAdmin.
3. Enter the DB values in `backend/config.php`.
4. Temporarily enable first-admin setup with a long random setup key.
5. Upload `afrihost-deploy/droplink-api/` to `public_html/droplink-api/`.
6. Upload `afrihost-deploy/droplink-control/` to `public_html/droplink-control/`.
7. Upload `afrihost-deploy/droplink/` to `public_html/droplink/`.
8. Confirm API health.
9. Create the first admin, then immediately disable setup.

## 2. Android build preparation
- compileSdk 36
- targetSdk 36
- minSdk 24
- versionCode 1
- versionName 1.0.0
- HTTPS-only traffic
- Customer app requests Internet only
- Driver app requests Internet + foreground fine/coarse location
- Driver build does not declare Android background location

## 3. Build validation
GitHub Actions `Validate DROPLINK Android Bundles` builds both release AABs without a signing key to verify the projects compile.

## 4. Release signing
Create and safely store a Google Play upload keystore. Configure GitHub repository secrets:
- `KEYSTORE_BASE64`
- `KEYSTORE_PASSWORD`
- `KEY_ALIAS`
- `KEY_PASSWORD`

Then manually run `Build signed DROPLINK Play Store AABs`.

Never commit a real keystore or passwords into the public repository.

## 5. Play Console
Create two applications:
- DROPLINK
- DROPLINK Driver

Complete: App access, Ads, Content rating, Target audience, Data safety, Privacy policy, Store listing, Countries/regions and the required testing track.

Privacy policy URL prepared for deployment:
`https://deutronomagroup.co.za/droplink/privacy.html`

Review `store/DATA-SAFETY-NOTES.md` against the final live behavior before submission.
