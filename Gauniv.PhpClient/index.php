<?php
$page_title = 'Gauniv 游戏平台 - 首页';
require_once 'includes/header.php';

// 获取最新游戏
$games_response = api_request('/games?offset=0&limit=6');
$latest_games = $games_response['games'] ?? [];

// 获取分类
$categories_response = api_request('/categories');
// API 直接返回数组
$categories = [];
if (is_array($categories_response) && !isset($categories_response['error'])) {
    $categories = $categories_response;
}
?>

<div class="row mb-5">
    <div class="col-12">
        <div class="card text-white" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border: none;">
            <div class="card-body text-center py-5">
                <h1 class="display-4 fw-bold mb-3">
                    <i class="bi bi-controller"></i> 欢迎来到 Gauniv 游戏平台
                </h1>
                <?php if (!is_logged_in()): ?>
                <?php endif; ?>
            </div>
        </div>
    </div>
</div>

<!-- 分类快速导航 -->
<div class="row mb-5">
    <div class="col-12">
        <h2 class="text-white mb-4">
            <i class="bi bi-tags"></i> 游戏分类
        </h2>
    </div>
    <?php if (is_array($categories) && !empty($categories)): ?>
        <?php foreach (array_slice($categories, 0, 6) as $category): ?>
            <?php if (is_array($category)): ?>
    <div class="col-md-4 mb-3">
        <a href="category.php?id=<?php echo $category['id'] ?? 0; ?>" class="text-decoration-none" style="display: block;">
            <div class="card" style="cursor: pointer; transition: transform 0.2s;">
                <div class="card-body">
                    <h5 class="card-title">
                        <i class="bi bi-folder"></i> <?php echo htmlspecialchars($category['name'] ?? '未知分类'); ?>
                    </h5>
                    <p class="text-muted mb-2">
                        <?php echo htmlspecialchars($category['description']); ?>
                    </p>
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="text-primary">
                        </span>
                    </div>
                </div>
            </div>
        </a>
    </div>
            <?php endif; ?>
        <?php endforeach; ?>
    <?php else: ?>
    <div class="col-12">
        <div class="alert alert-info">
            <i class="bi bi-info-circle"></i> 暂无分类数据
        </div>
    </div>
    <?php endif; ?>
</div>

<div class="row">
    <div class="col-12">
        <h2 class="text-white mb-4">
            <i class="bi bi-stars"></i> 最新上架
        </h2>
    </div>
    <?php if (empty($latest_games)): ?>
        <div class="col-12">
            <div class="alert alert-info">
                <i class="bi bi-info-circle"></i> 暂无游戏
            </div>
        </div>
    <?php else: ?>
        <?php foreach ($latest_games as $game): ?>
        <div class="col-md-4 mb-4">
                <div class="card game-card">
                <div class="card-body">
                    <h5 class="card-title">
                        <?php echo htmlspecialchars($game['name'] ?? '未知游戏'); ?>
                        <?php if (isset($game['isOwned']) && $game['isOwned']): ?>
                            <span class="badge bg-success float-end">已拥有</span>
                        <?php endif; ?>
                    </h5>
                    <div class="mb-3">
                        <?php foreach ($game['categoryNames'] ?? [] as $cat_name): ?>
                            <span class="badge bg-secondary category-badge">
                                <?php echo htmlspecialchars($cat_name ?? ''); ?>
                            </span>
                        <?php endforeach; ?>
                    </div>
                    <div class="mb-2">
                        <small class="text-muted">
                            <i class="bi bi-hdd"></i> <?php echo format_size($game['size'] ?? 0); ?>
                        </small>
                    </div>
                    <div class="mb-3">
                        <small class="text-muted">
                            <i class="bi bi-clock"></i> <?php echo format_date($game['createdAt'] ?? date('Y-m-d H:i:s')); ?>
                        </small>
                    </div>
                    <div class="d-flex justify-content-between align-items-center mt-auto">
                        <span class="price-tag"><?php echo format_price($game['price'] ?? 0); ?></span>
                        <a href="game.php?id=<?php echo $game['id'] ?? 0; ?>" class="btn btn-primary">
                            查看详情
                        </a>
                    </div>
                </div>
            </div>
        </div>
        <?php endforeach; ?>
    <?php endif; ?>
</div>

<div class="row mt-4">
    <div class="col-12 text-center">
        <a href="games.php" class="btn btn-lg btn-light">
            <i class="bi bi-arrow-right"></i> 查看更多游戏
        </a>
    </div>
</div>

