<?php
// DROPLINK production API configuration.
// Fill these values after creating the MySQL database/user in Afrihost cPanel.
// Never commit real passwords to a public repository.
return [
  'db_host' => 'localhost',
  'db_name' => 'CHANGE_ME_DATABASE',
  'db_user' => 'CHANGE_ME_USER',
  'db_pass' => 'CHANGE_ME_PASSWORD',
  'base_url' => 'https://deutronomagroup.co.za/droplink-api',
  'upload_dir' => __DIR__ . '/uploads',
  'max_upload_bytes' => 8 * 1024 * 1024,
  // Set a long random value temporarily, create the first admin, then set setup_enabled=false.
  'setup_enabled' => false,
  'setup_key' => 'CHANGE_ME_LONG_RANDOM_SETUP_KEY',
];
