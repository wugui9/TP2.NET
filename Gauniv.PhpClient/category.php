<?php
require_once 'config.php';

$category_id = isset($_GET['id']) ? (int)$_GET['id'] : 0;

if ($category_id <= 0) {
    header('Location: categories.php');
    exit;
}

// 获取分类及其游戏
$response = api_request("/categories/{$category_id}/games");

if (isset($response['error'])) {
    $_SESSION['message'] = '分类不存在';
    $_SESSION['message_type'] = 'danger';
    header('Location: categories.php');
    exit;
}

$category = $response['category'] ?? [];
$games = $response['games'] ?? [];

$page_title = htmlspecialchars($category['name']) . ' - 分类';
require_once 'includes/header.php';
?>

<div class="row mb-4">
    <div class="col-12">
        <div class="card" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border: none;">
            <div class="card-body text-white">
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb mb-2" style="background: transparent;">
                        <li class="breadcrumb-item">
                            <a href="categories.php" class="text-white">
                                <i class="bi bi-tags"></i> 分类
                            </a>
                        </li>
                        <li class="breadcrumb-item active text-white" aria-current="page">
                            <?php echo htmlspecialchars($category['name']); ?>
                        </li>
                    </ol>
                </nav>
                
                <h1 class="mb-2">
                    <i class="bi bi-folder"></i> 
                    <?php echo htmlspecialchars($category['name']); ?>
                </h1>
                
                <p class="lead mb-0">
                    <?php echo htmlspecialchars($category['description'] ?? ''); ?>
                </p>
                
                <div class="mt-3">
                    <span class="badge bg-light text-dark" style="font-size: 1rem;">
                        <?php echo $category['gameCount']; ?> 款游戏
                    </span>
                </div>
            </div>
        </div>
    </div>
</div>

<div class="row">
    <?php if (empty($games)): ?>
        <div class="col-12">
            <div class="alert alert-info">
                <i class="bi bi-info-circle"></i> 此分类下暂无游戏
            </div>
            <a href="categories.php" class="btn btn-outline-primary">
                <i class="bi bi-arrow-left"></i> 返回分类列表
            </a>
        </div>
    <?php else: ?>
        <?php foreach ($games as $game): ?>
        <div class="col-md-4 col-lg-3 mb-4">
            <div class="card game-card h-100">
                <div class="card-body d-flex flex-column">
                    <h5 class="card-title">
                        <?php echo htmlspecialchars($game['name']); ?>
                    </h5>
                    
                    <?php if ($game['isOwned']): ?>
                        <span class="badge bg-success mb-2">
                            <i class="bi bi-check-circle"></i> 已拥有
                        </span>
                    <?php endif; ?>
                    
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
                            <i class="bi bi-clock"></i> <?php echo format_date($game['createdAt']); ?>
                        </small>
                    </div>
                    
                    <div class="mt-auto">
                        <div class="d-flex justify-content-between align-items-center">
                            <span class="price-tag"><?php echo format_price($game['price']); ?></span>
                            <a href="game.php?id=<?php echo $game['id']; ?>" class="btn btn-primary btn-sm">
                                查看详情
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <?php endforeach; ?>
    <?php endif; ?>
</div>

<div class="row mt-4">
    <div class="col-12">
        <a href="categories.php" class="btn btn-outline-primary">
            <i class="bi bi-arrow-left"></i> 返回分类列表
        </a>
    </div>
</div>

