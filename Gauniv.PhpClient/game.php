<?php
require_once 'config.php';

$game_id = isset($_GET['id']) ? (int)$_GET['id'] : 0;

if ($game_id <= 0) {
    header('Location: games.php');
    exit;
}

// 处理购买请求
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action'])) {
    if (!is_logged_in()) {
        $_SESSION['message'] = '请先登录';
        $_SESSION['message_type'] = 'warning';
        header('Location: login.php');
        exit;
    }
    
    if ($_POST['action'] === 'purchase') {
        $response = api_request("/games/{$game_id}/purchase", 'POST');
        
        if (isset($response['success']) && $response['success']) {
            $_SESSION['message'] = $response['message'] ?? '购买成功！';
            $_SESSION['message_type'] = 'success';
        } else {
            $_SESSION['message'] = $response['message'] ?? '购买失败';
            $_SESSION['message_type'] = 'danger';
        }
        
        header("Location: game.php?id={$game_id}");
        exit;
    }
}

// 获取游戏详情
$game = api_request("/games/{$game_id}");

if (isset($game['error'])) {
    $_SESSION['message'] = '游戏不存在';
    $_SESSION['message_type'] = 'danger';
    header('Location: games.php');
    exit;
}

$page_title = htmlspecialchars($game['name']) . ' - 游戏详情';
require_once 'includes/header.php';
?>

<div class="row">
    <div class="col-md-8">
        <div class="card mb-4">
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-start mb-3">
                    <h1 class="card-title mb-0">
                        <?php echo htmlspecialchars($game['name']); ?>
                    </h1>
                    <?php if ($game['isOwned']): ?>
                        <span class="badge bg-success" style="font-size: 1rem;">
                            <i class="bi bi-check-circle"></i> 已拥有
                        </span>
                    <?php endif; ?>
                </div>
                
                <div class="mb-3">
                    <?php foreach ($game['categories'] ?? [] as $category): ?>
                        <span class="badge bg-primary category-badge">
                            <?php echo htmlspecialchars($category['name']); ?>
                        </span>
                    <?php endforeach; ?>
                </div>
                
                <hr>
                
                <h4><i class="bi bi-info-circle"></i> 游戏描述</h4>
                <p class="lead">
                    <?php echo nl2br(htmlspecialchars($game['description'] ?? '暂无描述')); ?>
                </p>
            </div>
        </div>
        
        <!-- 游戏信息 -->
        <div class="card">
            <div class="card-body">
                <h4><i class="bi bi-list-ul"></i> 游戏信息</h4>
                <table class="table table-borderless">
                    <tr>
                        <td><strong><i class="bi bi-hdd"></i> 文件大小:</strong></td>
                        <td><?php echo format_size($game['size']); ?></td>
                    </tr>
                    <tr>
                        <td><strong><i class="bi bi-file-earmark"></i> 文件名:</strong></td>
                        <td><?php echo htmlspecialchars($game['fileName'] ?? 'N/A'); ?></td>
                    </tr>
                    <tr>
                        <td><strong><i class="bi bi-calendar"></i> 上架时间:</strong></td>
                        <td><?php echo format_date($game['createdAt']); ?></td>
                    </tr>
                    <tr>
                        <td><strong><i class="bi bi-tag"></i> 分类:</strong></td>
                        <td>
                            <?php 
                            $cat_names = array_map(function($c) { 
                                return htmlspecialchars($c['name']); 
                            }, $game['categories'] ?? []);
                            echo implode(', ', $cat_names);
                            ?>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
    
    <!-- 购买/下载区域 -->
    <div class="col-md-4">
        <div class="card sticky-top" style="top: 20px;">
            <div class="card-body">
                <div class="text-center mb-4">
                    <div class="display-4 fw-bold" style="color: #27ae60;">
                        <?php echo format_price($game['price']); ?>
                    </div>
                </div>
                
                <?php if (!is_logged_in()): ?>
                    <div class="alert alert-info">
                        <i class="bi bi-info-circle"></i> 请先登录以购买游戏
                    </div> 
                <?php elseif ($game['isOwned']): ?>
                    <div class="alert alert-success">
                        <i class="bi bi-check-circle"></i> 您已拥有此游戏
                    </div>
                    <div class="d-grid gap-2">
                        <a href="download.php?id=<?php echo $game_id; ?>" class="btn btn-success btn-lg">
                            <i class="bi bi-download"></i> 下载游戏
                        </a>
                        <a href="library.php" class="btn btn-outline-primary">
                            <i class="bi bi-bookmark-star"></i> 查看游戏库
                        </a>
                    </div>
                <?php else: ?>
                    <form method="POST" action="">
                        <input type="hidden" name="action" value="purchase">
                        <div class="d-grid gap-2">
                            <button type="submit" class="btn btn-primary btn-lg">
                                <i class="bi bi-cart-plus"></i> 立即购买
                            </button>
                        </div>
                    </form>
                <?php endif; ?>
                
                <hr class="my-4">
                
                <div class="text-center">
                    <a href="games.php" class="btn btn-outline-secondary">
                        <i class="bi bi-arrow-left"></i> 返回商店
                    </a>
                </div>
            </div>
        </div>
    </div>
</div>

