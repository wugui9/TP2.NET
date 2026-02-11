<?php
// API 调试脚本
require_once 'config.php';

echo "<h2>API 调试信息</h2>";
echo "<pre>";

echo "API Base URL: " . API_BASE_URL . "\n\n";

// 测试分类接口
echo "=== 测试 /categories 接口 ===\n";
$categories_response = api_request('/categories');

echo "响应类型: " . gettype($categories_response) . "\n";
echo "是否为数组: " . (is_array($categories_response) ? '是' : '否') . "\n";

if (is_array($categories_response)) {
    echo "是否有 error 键: " . (isset($categories_response['error']) ? '是' : '否') . "\n";
    echo "数组元素数量: " . count($categories_response) . "\n";
    echo "\n完整响应内容:\n";
    print_r($categories_response);
} else {
    echo "响应内容:\n";
    var_dump($categories_response);
}

echo "\n\n=== 测试 /games 接口 ===\n";
$games_response = api_request('/games?page=1&pageSize=3');
echo "响应类型: " . gettype($games_response) . "\n";
if (is_array($games_response)) {
    echo "游戏数据:\n";
    print_r($games_response);
}

echo "</pre>";
?>
