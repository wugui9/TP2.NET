<?php
$page_title = 'Gauniv 游戏平台 - 游戏分类';
require_once 'includes/header.php';

// 获取所有分类
$categories_response = api_request('/categories');
// API 返回格式: {"value": [...], "Count": n}
$categories = [];
if (is_array($categories_response) && !isset($categories_response['error'])) {
    $categories = $categories_response['value'] ?? $categories_response;
}
?>

<div class="row mb-4">
    <div class="col-12">
        <h1 class="text-white">
            <i class="bi bi-tags"></i> 游戏分类
            <span class="badge bg-light text-dark"><?php echo count($categories); ?> 个分类</span>
        </h1>
    </div>
</div>

<div class="row">
    <?php if (empty($categories)): ?>
        <div class="col-12">
            <div class="alert alert-info">
                <i class="bi bi-info-circle"></i> 暂无分类
            </div>
        </div>
    <?php else: ?>
        <?php foreach ($categories as $category): ?>
            <?php if (is_array($category)): ?>
        <div class="col-md-6 col-lg-4 mb-4">
            <a href="category.php?id=<?php echo $category['id'] ?? 0; ?>" class="text-decoration-none" style="display: block; height: 100%;">
                <div class="card h-100" style="cursor: pointer;">
                    <div class="card-body">
                        <h4 class="card-title text-primary">
                            <i class="bi bi-folder"></i> 
                            <?php echo htmlspecialchars($category['name'] ?? '未知分类'); ?>
                        </h4>
                        
                        <p class="card-text text-muted">
                            <?php echo htmlspecialchars($category['description'] ?? '暂无描述'); ?>
                        </p>
                        
                        <div class="d-flex justify-content-between align-items-center mt-3">
                            <span class="badge bg-primary" style="font-size: 1rem;">
                                <i class="bi bi-collection"></i> 
                            </span>
                            
                            <span class="text-primary">
                                <i class="bi bi-arrow-right"></i> 查看游戏
                            </span>
                        </div>
                    </div>
                </div>
            </a>
        </div>
            <?php endif; ?>
        <?php endforeach; ?>
    <?php endif; ?>
</div>

