<?php
$page_title = 'Gauniv 游戏平台 - 游戏商店';
require_once 'includes/header.php';

// 获取分页参数
$page = isset($_GET['page']) ? max(1, (int)$_GET['page']) : 1;
$limit = 12;
$offset = ($page - 1) * $limit;

// 获取游戏列表
$games_response = api_request("/games?offset={$offset}&limit={$limit}");
$games = $games_response['games'] ?? [];
$total_count = $games_response['total'] ?? 0;
$total_pages = ceil($total_count / $limit);

// 获取分类用于筛选
$categories = api_request('/categories') ?? [];
?>

<div class="row mb-4">
    <div class="col-12">
        <h1 class="text-white">
            <i class="bi bi-collection"></i> 游戏商店
            <span class="badge bg-light text-dark"><?php echo $total_count; ?> 款游戏</span>
        </h1>
    </div>
</div>

<!-- 筛选区域 -->
<div class="row mb-4">
    <div class="col-12">
        <div class="card">
            <div class="card-body">
                <h5><i class="bi bi-funnel"></i> 筛选选项</h5>
                <div class="row mt-3">
                    <div class="col-md-6">
                        <label class="form-label">按分类筛选</label>
                        <select class="form-select" id="categoryFilter" onchange="filterByCategory()">
                            <option value="">全部分类</option>
                            <?php foreach ($categories as $category): ?>
                                <option value="<?php echo $category['id']; ?>">
                                    <?php echo htmlspecialchars($category['name']); ?> 
                                    (<?php echo $category['gameCount']; ?>)
                                </option>
                            <?php endforeach; ?>
                        </select>
                    </div>
                    <?php if (is_logged_in()): ?>
                    <div class="col-md-6">
                        <label class="form-label">显示</label>
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" id="showOwnedOnly">
                            <label class="form-check-label" for="showOwnedOnly">
                                仅显示已拥有的游戏
                            </label>
                        </div>
                    </div>
                    <?php endif; ?>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- 游戏列表 -->
<div class="row" id="gamesList">
    <?php if (empty($games)): ?>
        <div class="col-12">
            <div class="alert alert-info">
                <i class="bi bi-info-circle"></i> 暂无游戏
            </div>
        </div>
    <?php else: ?>
        <?php foreach ($games as $game): ?>
        <div class="col-md-4 col-lg-3 mb-4 game-item" 
             data-owned="<?php echo $game['isOwned'] ? '1' : '0'; ?>">
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

<!-- 分页 -->
<?php if ($total_pages > 1): ?>
<div class="row mt-4">
    <div class="col-12">
        <nav>
            <ul class="pagination justify-content-center">
                <?php if ($page > 1): ?>
                    <li class="page-item">
                        <a class="page-link" href="?page=<?php echo $page - 1; ?>">
                            <i class="bi bi-chevron-left"></i> 上一页
                        </a>
                    </li>
                <?php endif; ?>
                
                <?php for ($i = max(1, $page - 2); $i <= min($total_pages, $page + 2); $i++): ?>
                    <li class="page-item <?php echo $i === $page ? 'active' : ''; ?>">
                        <a class="page-link" href="?page=<?php echo $i; ?>"><?php echo $i; ?></a>
                    </li>
                <?php endfor; ?>
                
                <?php if ($page < $total_pages): ?>
                    <li class="page-item">
                        <a class="page-link" href="?page=<?php echo $page + 1; ?>">
                            下一页 <i class="bi bi-chevron-right"></i>
                        </a>
                    </li>
                <?php endif; ?>
            </ul>
        </nav>
    </div>
</div>
<?php endif; ?>

<script>
function filterByCategory() {
    const categoryId = document.getElementById('categoryFilter').value;
    if (categoryId) {
        window.location.href = 'category.php?id=' + categoryId;
    }
}

// 仅显示已拥有的游戏
document.getElementById('showOwnedOnly')?.addEventListener('change', function() {
    const gameItems = document.querySelectorAll('.game-item');
    gameItems.forEach(item => {
        if (this.checked) {
            if (item.dataset.owned === '0') {
                item.style.display = 'none';
            }
        } else {
            item.style.display = '';
        }
    });
});
</script>

