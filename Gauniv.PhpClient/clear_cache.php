<?php

if (function_exists('opcache_reset')) {
    opcache_reset();
    echo "✅ OpCache 已清理<br>";
} else {
    echo "ℹ️ OpCache 未启用<br>";
}

if (function_exists('apcu_clear_cache')) {
    apcu_clear_cache();
    echo "✅ APCu 缓存已清理<br>";
} else {
    echo "ℹ️ APCu 未启用<br>";
}

// 清理会话
session_start();
session_destroy();
echo "✅ 会话已清理<br>";

echo "<br><strong>缓存清理完成！</strong><br>";
echo '<a href="index.php">返回首页</a>';
?>
