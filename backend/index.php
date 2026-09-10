<?php
header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('X-Content-Type-Options: nosniff');
header('Cache-Control: no-store');
header('Access-Control-Allow-Headers: Authorization, Content-Type');
header('Access-Control-Allow-Methods: GET, POST, PATCH, OPTIONS');
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') { http_response_code(204); exit; }

$cfg = require __DIR__ . '/config.php';
try {
  $pdo = new PDO('mysql:host='.$cfg['db_host'].';dbname='.$cfg['db_name'].';charset=utf8mb4', $cfg['db_user'], $cfg['db_pass'], [
    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
    PDO::ATTR_EMULATE_PREPARES => false,
  ]);
} catch (Throwable $e) { fail(503, 'Database unavailable'); }

function out($data, $code=200){ http_response_code($code); echo json_encode($data, JSON_UNESCAPED_SLASHES); exit; }
function fail($code,$msg){ out(['ok'=>false,'error'=>$msg],$code); }
function body(){ $raw=file_get_contents('php://input'); if(!$raw)return []; $j=json_decode($raw,true); return is_array($j)?$j:[]; }
function bearer(){ $h=$_SERVER['HTTP_AUTHORIZATION']??''; return preg_match('/Bearer\s+(.+)/i',$h,$m)?trim($m[1]):null; }
function auth($pdo,$roles=[]){
  $t=bearer(); if(!$t) fail(401,'Login required'); $h=hash('sha256',$t);
  $st=$pdo->prepare('SELECT u.* FROM api_tokens t JOIN users u ON u.id=t.user_id WHERE t.token_hash=? AND t.expires_at>NOW() AND u.active=1 LIMIT 1');
  $st->execute([$h]); $u=$st->fetch(); if(!$u) fail(401,'Session expired');
  if($roles && !in_array($u['role'],$roles,true)) fail(403,'Not allowed'); return $u;
}
function issueToken($pdo,$uid){ $raw=bin2hex(random_bytes(32)); $h=hash('sha256',$raw); $st=$pdo->prepare('INSERT INTO api_tokens(user_id,token_hash,expires_at) VALUES(?,?,DATE_ADD(NOW(), INTERVAL 30 DAY))'); $st->execute([$uid,$h]); return $raw; }
function cleanPhone($p){ return preg_replace('/[^0-9+]/','',(string)$p); }
function orderByPublic($pdo,$id){ $st=$pdo->prepare('SELECT o.*,c.name customer_name,c.phone customer_phone,d.name driver_name,d.phone driver_phone FROM orders o JOIN users c ON c.id=o.customer_id LEFT JOIN users d ON d.id=o.driver_id WHERE o.public_id=? LIMIT 1'); $st->execute([$id]); $r=$st->fetch(); if(!$r) fail(404,'Order not found'); return $r; }
function canSee($u,$o){ return $u['role']==='admin' || ($u['role']==='customer' && (int)$o['customer_id']===(int)$u['id']) || ($u['role']==='driver' && (int)$o['driver_id']===(int)$u['id']); }

$route=trim($_GET['route']??'health','/'); $method=$_SERVER['REQUEST_METHOD'];

if($route==='health') out(['ok'=>true,'service'=>'DROPLINK API','time'=>gmdate('c')]);

if($route==='auth/register-customer' && $method==='POST'){
  $b=body(); $name=trim($b['name']??''); $phone=cleanPhone($b['phone']??''); $pass=(string)($b['password']??'');
  if(strlen($name)<2||strlen($phone)<8||strlen($pass)<8) fail(422,'Name, phone and an 8+ character password are required');
  $st=$pdo->prepare('SELECT id FROM users WHERE phone=?'); $st->execute([$phone]); if($st->fetch()) fail(409,'Phone already registered');
  $st=$pdo->prepare("INSERT INTO users(role,name,phone,password_hash) VALUES('customer',?,?,?)"); $st->execute([$name,$phone,password_hash($pass,PASSWORD_DEFAULT)]); $uid=(int)$pdo->lastInsertId();
  out(['ok'=>true,'token'=>issueToken($pdo,$uid),'user'=>['id'=>$uid,'role'=>'customer','name'=>$name,'phone'=>$phone]],201);
}
if($route==='auth/login' && $method==='POST'){
  $b=body(); $login=trim($b['login']??''); $pass=(string)($b['password']??'');
  $st=$pdo->prepare('SELECT * FROM users WHERE (phone=? OR email=?) AND active=1 LIMIT 1'); $st->execute([cleanPhone($login),$login]); $u=$st->fetch();
  if(!$u||!password_verify($pass,$u['password_hash'])) fail(401,'Incorrect login');
  out(['ok'=>true,'token'=>issueToken($pdo,(int)$u['id']),'user'=>['id'=>(int)$u['id'],'role'=>$u['role'],'name'=>$u['name'],'phone'=>$u['phone'],'email'=>$u['email']]]);
}
if($route==='me' && $method==='GET'){ $u=auth($pdo); out(['ok'=>true,'user'=>['id'=>(int)$u['id'],'role'=>$u['role'],'name'=>$u['name'],'phone'=>$u['phone'],'email'=>$u['email']]]); }

if($route==='orders' && $method==='GET'){
  $u=auth($pdo); $sql='SELECT o.*,c.name customer_name,c.phone customer_phone,d.name driver_name,d.phone driver_phone FROM orders o JOIN users c ON c.id=o.customer_id LEFT JOIN users d ON d.id=o.driver_id'; $args=[];
  if($u['role']==='customer'){ $sql.=' WHERE o.customer_id=?'; $args[]=$u['id']; }
  elseif($u['role']==='driver'){ $sql.=' WHERE o.driver_id=?'; $args[]=$u['id']; }
  $sql.=' ORDER BY o.created_at DESC LIMIT 200'; $st=$pdo->prepare($sql); $st->execute($args); out(['ok'=>true,'orders'=>$st->fetchAll()]);
}
if($route==='orders' && $method==='POST'){
  $u=auth($pdo,['customer','admin']); $b=body();
  $customerId=$u['role']==='customer'?(int)$u['id']:(int)($b['customer_id']??0); if(!$customerId) fail(422,'Customer required');
  foreach(['pickup','dropoff','package_size'] as $f) if(!trim((string)($b[$f]??''))) fail(422,$f.' is required');
  $public='DLK-'.strtoupper(substr(bin2hex(random_bytes(5)),0,10));
  $st=$pdo->prepare('INSERT INTO orders(public_id,customer_id,pickup,dropoff,sender_name,sender_phone,receiver_name,receiver_phone,package_size,notes,payment_method,delivery_fee,driver_tip,status) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?)');
  $st->execute([$public,$customerId,$b['pickup'],$b['dropoff'],$b['sender_name']??null,$b['sender_phone']??null,$b['receiver_name']??null,$b['receiver_phone']??null,$b['package_size'],$b['notes']??null,$b['payment_method']??'Cash',(float)($b['delivery_fee']??0),(float)($b['driver_tip']??0),'Searching for driver']);
  out(['ok'=>true,'order'=>orderByPublic($pdo,$public)],201);
}
if(preg_match('#^orders/([^/]+)$#',$route,$m) && $method==='GET'){
  $u=auth($pdo); $o=orderByPublic($pdo,$m[1]); if(!canSee($u,$o)) fail(403,'Not allowed'); out(['ok'=>true,'order'=>$o]);
}
if(preg_match('#^orders/([^/]+)$#',$route,$m) && $method==='PATCH'){
  $u=auth($pdo,['driver','admin']); $o=orderByPublic($pdo,$m[1]); if($u['role']==='driver'&&(int)$o['driver_id']!==(int)$u['id']) fail(403,'Not your delivery'); $b=body();
  $allowed=['In Progress','Driver going to pickup','Parcel collected','On the way','Driver arrived','Delivered','Issue'];
  $sets=[];$args=[];
  if(isset($b['status'])){ if(!in_array($b['status'],$allowed,true)) fail(422,'Invalid status'); $sets[]='status=?';$args[]=$b['status']; }
  if($u['role']==='admin' && array_key_exists('driver_id',$b)){ $sets[]='driver_id=?';$args[]=$b['driver_id']?:null; if($b['driver_id'] && !isset($b['status'])){$sets[]='status=?';$args[]='Driver assigned';} }
  if(!$sets) fail(422,'Nothing to update'); $args[]=$o['id']; $st=$pdo->prepare('UPDATE orders SET '.implode(',',$sets).' WHERE id=?');$st->execute($args); out(['ok'=>true,'order'=>orderByPublic($pdo,$m[1])]);
}

if(preg_match('#^orders/([^/]+)/messages$#',$route,$m)){
  $u=auth($pdo); $o=orderByPublic($pdo,$m[1]); if(!canSee($u,$o)) fail(403,'Not allowed');
  if($method==='GET'){ $st=$pdo->prepare('SELECT m.*,u.name sender_name,u.role sender_role FROM messages m JOIN users u ON u.id=m.sender_id WHERE m.order_id=? ORDER BY m.created_at ASC LIMIT 300');$st->execute([$o['id']]);out(['ok'=>true,'messages'=>$st->fetchAll()]); }
  if($method==='POST'){ $b=body();$msg=trim((string)($b['message']??'')); if($msg==='')fail(422,'Message required');$scope=$b['recipient_scope']??($u['role']==='customer'?'driver':'customer'); if(!in_array($scope,['customer','driver','control'],true))fail(422,'Invalid recipient');$st=$pdo->prepare('INSERT INTO messages(order_id,sender_id,recipient_scope,message) VALUES(?,?,?,?)');$st->execute([$o['id'],$u['id'],$scope,$msg]);out(['ok'=>true],201); }
}

if($route==='driver/location' && $method==='POST'){
  $u=auth($pdo,['driver']); $b=body(); if(!isset($b['latitude'],$b['longitude']))fail(422,'Location required');
  $st=$pdo->prepare('INSERT INTO driver_locations(driver_id,latitude,longitude,accuracy_m) VALUES(?,?,?,?) ON DUPLICATE KEY UPDATE latitude=VALUES(latitude),longitude=VALUES(longitude),accuracy_m=VALUES(accuracy_m),updated_at=CURRENT_TIMESTAMP');
  $st->execute([$u['id'],(float)$b['latitude'],(float)$b['longitude'],isset($b['accuracy_m'])?(float)$b['accuracy_m']:null]); out(['ok'=>true]);
}
if($route==='drivers' && $method==='GET'){ auth($pdo,['admin']); $st=$pdo->query("SELECT id,name,phone,email,active FROM users WHERE role='driver' ORDER BY name"); out(['ok'=>true,'drivers'=>$st->fetchAll()]); }
if($route==='drivers' && $method==='POST'){
  auth($pdo,['admin']); $b=body();$name=trim($b['name']??'');$phone=cleanPhone($b['phone']??'');$pass=(string)($b['password']??''); if(strlen($name)<2||strlen($phone)<8||strlen($pass)<8)fail(422,'Name, phone and an 8+ character password are required');
  $st=$pdo->prepare("INSERT INTO users(role,name,phone,email,password_hash) VALUES('driver',?,?,?,?)");$st->execute([$name,$phone,$b['email']??null,password_hash($pass,PASSWORD_DEFAULT)]);out(['ok'=>true,'id'=>(int)$pdo->lastInsertId()],201);
}
if($route==='driver-locations' && $method==='GET'){ auth($pdo,['admin']); $st=$pdo->query("SELECT l.*,u.name driver_name,u.phone driver_phone FROM driver_locations l JOIN users u ON u.id=l.driver_id ORDER BY l.updated_at DESC");out(['ok'=>true,'locations'=>$st->fetchAll()]); }

if(preg_match('#^orders/([^/]+)/proof$#',$route,$m) && $method==='POST'){
  $u=auth($pdo,['driver']); $o=orderByPublic($pdo,$m[1]); if((int)$o['driver_id']!==(int)$u['id'])fail(403,'Not your delivery');
  $recipient=trim($_POST['recipient_name']??'');$note=trim($_POST['note']??'');$photoUrl=null;
  if(isset($_FILES['photo'])&&$_FILES['photo']['error']===UPLOAD_ERR_OK){ if($_FILES['photo']['size']>$cfg['max_upload_bytes'])fail(413,'Photo too large'); $mime=(new finfo(FILEINFO_MIME_TYPE))->file($_FILES['photo']['tmp_name']);$ext=['image/jpeg'=>'jpg','image/png'=>'png','image/webp'=>'webp'][$mime]??null;if(!$ext)fail(415,'Unsupported photo'); if(!is_dir($cfg['upload_dir']))mkdir($cfg['upload_dir'],0755,true);$fn=$o['public_id'].'-'.time().'.'.$ext;$dst=$cfg['upload_dir'].'/'.$fn;if(!move_uploaded_file($_FILES['photo']['tmp_name'],$dst))fail(500,'Upload failed');$photoUrl=rtrim($cfg['base_url'],'/').'/uploads/'.$fn; }
  $st=$pdo->prepare('INSERT INTO delivery_proofs(order_id,recipient_name,note,photo_url) VALUES(?,?,?,?) ON DUPLICATE KEY UPDATE recipient_name=VALUES(recipient_name),note=VALUES(note),photo_url=COALESCE(VALUES(photo_url),photo_url)');$st->execute([$o['id'],$recipient,$note,$photoUrl]);$pdo->prepare("UPDATE orders SET status='Delivered' WHERE id=?")->execute([$o['id']]);out(['ok'=>true,'order'=>orderByPublic($pdo,$m[1]),'photo_url'=>$photoUrl]);
}

if($route==='admin/create' && $method==='POST'){
  if(empty($cfg['setup_enabled'])) fail(403,'Admin setup is disabled');
  $expected=(string)($cfg['setup_key']??'');
  if(strlen($expected)<24 || str_starts_with($expected,'CHANGE_ME')) fail(503,'Admin setup key is not configured');
  $b=body(); $setup=(string)($b['setup_key']??''); if(!hash_equals($expected,$setup))fail(403,'Invalid setup key');
  $count=(int)$pdo->query("SELECT COUNT(*) FROM users WHERE role='admin'")->fetchColumn(); if($count>0) fail(409,'Admin account already exists');
  $name=trim($b['name']??'DROPLINK Admin');$phone=cleanPhone($b['phone']??'');$pass=(string)($b['password']??'');if(strlen($phone)<8||strlen($pass)<10)fail(422,'Phone and a 10+ character password are required');
  $st=$pdo->prepare("INSERT INTO users(role,name,phone,email,password_hash) VALUES('admin',?,?,?,?)");$st->execute([$name,$phone,$b['email']??null,password_hash($pass,PASSWORD_DEFAULT)]);out(['ok'=>true,'id'=>(int)$pdo->lastInsertId()],201);
}

fail(404,'Route not found');
