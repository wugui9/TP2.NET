<?php
require_once 'config.php';

// 需要登录
if (!is_logged_in()) {
    $_SESSION['message'] = '请先登录';
    $_SESSION['message_type'] = 'warning';
    header('Location: login.php');
    exit;
}

$game_id = isset($_GET['id']) ? (int)$_GET['id'] : 0;

if ($game_id <= 0) {
    header('Location: library.php');
    exit;
}

// 获取游戏信息以显示下载页面
$game = api_request("/games/{$game_id}");

if (isset($game['error']) || !$game['isOwned']) {
    $_SESSION['message'] = '您没有此游戏或游戏不存在';
    $_SESSION['message_type'] = 'danger';
    header('Location: library.php');
    exit;
}

// 实际下载文件
if (isset($_GET['download']) && $_GET['download'] === 'start') {
    $download_url = API_BASE_URL . "/games/{$game_id}/download";
    
    $options = [
        'http' => [
            'method' => 'GET',
            'header' => [
                'Accept: application/octet-stream'
            ]
        ]
    ];
    
    // 添加cookie
    if (isset($_COOKIE[SESSION_NAME])) {
        $options['http']['header'][] = 'Cookie: ' . SESSION_NAME . '=' . $_COOKIE[SESSION_NAME];
    }
    
    $context = stream_context_create($options);
    $file_content = @file_get_contents($download_url, false, $context);
    
    if ($file_content !== false) {
        $filename = $game['fileName'] ?? ($game['name'] . '.exe');
        
        header('Content-Type: application/octet-stream');
        header('Content-Disposition: attachment; filename="' . $filename . '"');
        header('Content-Length: ' . strlen($file_content));
        
        echo $file_content;
        exit;
    } else {
        $_SESSION['message'] = '下载失败，请重试';
        $_SESSION['message_type'] = 'danger';
        header("Location: download.php?id={$game_id}");
        exit;
    }
}

$page_title = '下载 ' . htmlspecialchars($game['name']);
require_once 'includes/header.php';
?>

<div class="row justify-content-center">
    <div class="col-md-8">
        <div class="card shadow-lg">
            <div class="card-body text-center p-5">
                <i class="bi bi-download" style="font-size: 5rem; color: #27ae60;"></i>
                
                <h1 class="mt-4 mb-3">下载游戏</h1>
                <h3 class="text-primary mb-4"><?php echo htmlspecialchars($game['name']); ?></h3>
                
                <div class="alert alert-info">
                    <h5><i class="bi bi-info-circle"></i> 下载信息</h5>
                    <hr>
                    <div class="text-start">
                        <p><strong>文件名:</strong> <?php echo htmlspecialchars($game['fileName'] ?? 'N/A'); ?></p>
                        <p><strong>文件大小:</strong> <?php echo format_size($game['size']); ?></p>
                        <p class="mb-0"><strong>类别:</strong> 
                            <?php 
                            $cat_names = array_map(function($c) { 
                                return htmlspecialchars($c['name']); 
                            }, $game['categories'] ?? []);
                            echo implode(', ', $cat_names);
                            ?>
                        </p>
                    </div>
                </div>
                
                <div class="d-grid gap-3 mt-4">
                    <a href="download.php?id=<?php echo $game_id; ?>&download=start" 
                       class="btn btn-success btn-lg">
                        <i class="bi bi-download"></i> 开始下载
                    </a>
                    
                    <div class="row">
                        <div class="col-md-6">
                            <a href="game.php?id=<?php echo $game_id; ?>" 
                               class="btn btn-outline-primary w-100">
                                <i class="bi bi-info-circle"></i> 查看详情
                            </a>
                        </div>
                        <div class="col-md-6">
                            <a href="library.php" class="btn btn-outline-secondary w-100">
                                <i class="bi bi-arrow-left"></i> 返回游戏库
                            </a>
                        </div>
                    </div>
                </div>
                
                <div class="mt-4">
                    <small class="text-muted">
                        <i class="bi bi-shield-check"></i> 下载的文件是安全的 | 
                        <i class="bi bi-arrow-clockwise"></i> 您可以随时重新下载
                    </small>
                </div>
            </div>
        </div>
    </div>
</div>

