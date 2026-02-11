<?php
require_once 'config.php';

// 需要登录
if (!is_logged_in()) {
    $_SESSION['message'] = '请先登录以查看您的游戏库';
    $_SESSION['message_type'] = 'warning';
    header('Location: login.php');
    exit;
}

// 获取用户拥有的游戏
$response = api_request('/games?owned=true');
$owned_games = $response['games'] ?? [];

$page_title = '我的游戏库';
require_once 'includes/header.php';

$user = get_logged_in_user();
?>

<div class="row mb-4">
    <div class="col-12">
        <div class="card" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border: none;">
            <div class="card-body text-white">
                <h1>
                    <i class="bi bi-bookmark-star"></i> 
                    <?php echo htmlspecialchars($user['firstName'] ?? $user['email']); ?> 的游戏库
                </h1>
                <p class="lead mb-0">
                    您拥有 <strong><?php echo count($owned_games); ?></strong> 款游戏
                </p>
            </div>
        </div>
    </div>
</div>

<?php if (empty($owned_games)): ?>
    <div class="row">
        <div class="col-12">
            <div class="card text-center py-5">
                <div class="card-body">
                    <i class="bi bi-inbox" style="font-size: 4rem; color: #ccc;"></i>
                    <h3 class="mt-3">您的游戏库是空的</h3>
                    <p class="text-muted">浏览商店，发现您喜欢的游戏！</p>
                    <a href="games.php" class="btn btn-primary btn-lg mt-3">
                        <i class="bi bi-shop"></i> 浏览游戏商店
                    </a>
                </div>
            </div>
        </div>
    </div>
<?php else: ?>
    <div class="row">
        <?php foreach ($owned_games as $game): ?>
        <div class="col-md-4 col-lg-3 mb-4">
            <div class="card game-card h-100">
                <div class="card-body d-flex flex-column">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <h5 class="card-title mb-0">
                            <?php echo htmlspecialchars($game['name']); ?>
                        </h5>
                        <span class="badge bg-success">
                            <i class="bi bi-check-circle"></i>
                        </span>
                    </div>
                    
                    <div class="mb-2">
                        <?php foreach ($game['categoryNames'] ?? [] as $cat_name): ?>
                            <span class="badge bg-secondary category-badge">
                                <?php echo htmlspecialchars($cat_name); ?>
                            </span>
                        <?php endforeach; ?>
                    </div>
                    
                    <div class="mb-2">
                        <small class="text-muted">
                            <i class="bi bi-hdd"></i> <?php echo format_size($game['size']); ?>
                        </small>
                    </div>
                    
                    <div class="mb-3">
                        <small class="text-muted">
                            购买价格: <strong><?php echo format_price($game['price']); ?></strong>
                        </small>
                    </div>
                    
                    <div class="mt-auto d-grid gap-2">
                        <a href="download.php?id=<?php echo $game['id']; ?>" 
                           class="btn btn-success">
                            <i class="bi bi-download"></i> 下载
                        </a>
                        <a href="game.php?id=<?php echo $game['id']; ?>" 
                           class="btn btn-outline-primary btn-sm">
                            <i class="bi bi-info-circle"></i> 详情
                        </a>
                    </div>
                </div>
            </div>
        </div>
        <?php endforeach; ?>
    </div>
    
    <div class="row mt-4">
        <div class="col-12">
            <div class="card">
                <div class="card-body text-center">
                    <h5>
                        <i class="bi bi-graph-up"></i> 游戏库统计
                    </h5>
                    <div class="row mt-3">
                        <div class="col-md-4">
                            <div class="p-3">
                                <h2 class="text-primary"><?php echo count($owned_games); ?></h2>
                                <p class="text-muted mb-0">拥有游戏</p>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="p-3">
                                <h2 class="text-success">
                                    <?php 
                                    $total_size = array_sum(array_column($owned_games, 'size'));
                                    echo format_size($total_size);
                                    ?>
                                </h2>
                                <p class="text-muted mb-0">总大小</p>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="p-3">
                                <h2 class="text-info">
                                    <?php 
                                    $total_value = array_sum(array_column($owned_games, 'price'));
                                    echo format_price($total_value);
                                    ?>
                                </h2>
                                <p class="text-muted mb-0">总价值</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
<?php endif; ?>

