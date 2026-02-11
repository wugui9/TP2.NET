<?php
// API配置
define('API_BASE_URL', 'http://localhost:5231/api');
define('SESSION_NAME', 'GAUNIV_SESSION');

// 启动session
if (session_status() === PHP_SESSION_NONE) {
    session_name(SESSION_NAME);
    session_start();
}

// 辅助函数 - API请求
function api_request($endpoint, $method = 'GET', $data = null) {
    $url = API_BASE_URL . $endpoint;
    
    $options = [
        'http' => [
            'method' => $method,
            'header' => [
                'Content-Type: application/json',
                'Accept: application/json'
            ],
            'ignore_errors' => true
        ]
    ];
    
    // 添加cookie以维持session
    if (isset($_COOKIE[SESSION_NAME])) {
        $options['http']['header'][] = 'Cookie: ' . SESSION_NAME . '=' . $_COOKIE[SESSION_NAME];
    }
    
    if ($data !== null && in_array($method, ['POST', 'PUT', 'PATCH'])) {
        $options['http']['content'] = json_encode($data);
    }
    
    $context = stream_context_create($options);
    $response = @file_get_contents($url, false, $context);
    
    if ($response === false) {
        return ['error' => true, 'message' => 'API请求失败'];
    }
    
    // 获取响应头
    $response_headers = http_get_last_response_headers();
    
    // 保存响应中的cookie
    if ($response_headers) {
        foreach ($response_headers as $header) {
            if (stripos($header, 'Set-Cookie:') === 0) {
                $cookie_parts = explode(';', substr($header, 12));
                $cookie = explode('=', $cookie_parts[0], 2);
                if (count($cookie) === 2) {
                    setcookie($cookie[0], $cookie[1], time() + 3600 * 24, '/');
                }
            }
        }
    }
    
    $decoded = json_decode($response, true);
    
    // 如果JSON解码失败，返回错误
    if (!is_array($decoded)) {
        return ['error' => true, 'message' => 'API响应格式错误', 'raw_response' => substr($response, 0, 200)];
    }
    
    // 获取HTTP状态码
    if ($response_headers && isset($response_headers[0])) {
        preg_match('/\d{3}/', $response_headers[0], $matches);
        $status_code = isset($matches[0]) ? (int)$matches[0] : 200;
        
        if ($status_code >= 400) {
            return ['error' => true, 'message' => $decoded['message'] ?? 'API错误', 'status' => $status_code];
        }
    }
    
    return $decoded;
}

// 检查用户是否已登录
function is_logged_in() {
    return isset($_SESSION['user']) && !empty($_SESSION['user']);
}

// 获取当前登录用户信息
function get_logged_in_user() {
    return $_SESSION['user'] ?? null;
}

// 格式化价格
function format_price($price) {
    return number_format($price, 2) . ' €';
}

// 格式化文件大小
function format_size($bytes) {
    if ($bytes >= 1073741824) {
        return number_format($bytes / 1073741824, 2) . ' GB';
    } elseif ($bytes >= 1048576) {
        return number_format($bytes / 1048576, 2) . ' MB';
    } elseif ($bytes >= 1024) {
        return number_format($bytes / 1024, 2) . ' KB';
    }
    return $bytes . ' B';
}

// 格式化日期
function format_date($date_string) {
    $date = new DateTime($date_string);
    return $date->format('Y-m-d H:i');
}
?>
