# DROPLINK backend deployment

This is the shared online layer that links Customer, Driver and Management. It is designed for PHP + MySQL on standard Linux hosting.

1. Create MySQL DB/user in cPanel.
2. Import schema.sql.
3. Edit config.php.
4. Upload to public_html/droplink-api/.
5. Test index.php?route=health.
6. Create first admin via POST route admin/create, then disable that route.

Important: keep config.php private and use HTTPS only.
