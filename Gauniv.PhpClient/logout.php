<?php
require_once 'config.php';

// 调用API登出
api_request('/auth/logout', 'POST');

// 清除session
session_destroy();

// 清除cookies
setcookie(SESSION_NAME, '', time() - 3600, '/');

// 重定向到首页
header('Location: index.php');
exit;
?>
