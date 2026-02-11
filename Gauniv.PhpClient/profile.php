<?php
require_once 'config.php';

// 需要登录
if (!is_logged_in()) {
    $_SESSION['message'] = '请先登录';
    $_SESSION['message_type'] = 'warning';
    header('Location: login.php');
    exit;
}

// 获取用户详细信息
$user_info = api_request('/auth/me');

if (isset($user_info['error'])) {
    $_SESSION['message'] = '无法获取用户信息';
    $_SESSION['message_type'] = 'danger';
    header('Location: index.php');
    exit;
}

$page_title = '个人信息';
require_once 'includes/header.php';
?>

<div class="row justify-content-center">
    <div class="col-md-8">
        <div class="card shadow-lg">
            <div class="card-header text-white text-center py-4" 
                 style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);">
                <i class="bi bi-person-circle" style="font-size: 4rem;"></i>
                <h2 class="mt-3">个人信息</h2>
            </div>
            
            <div class="card-body p-5">
                <div class="row mb-4">
                    <div class="col-md-6">
                        <div class="card bg-light">
                            <div class="card-body">
                                <h6 class="text-muted mb-2">
                                    <i class="bi bi-person"></i> 名字
                                </h6>
                                <h4><?php echo htmlspecialchars($user_info['firstName']); ?></h4>
                            </div>
                        </div>
                    </div>
                    
                    <div class="col-md-6">
                        <div class="card bg-light">
                            <div class="card-body">
                                <h6 class="text-muted mb-2">
                                    <i class="bi bi-person"></i> 姓氏
                                </h6>
                                <h4><?php echo htmlspecialchars($user_info['lastName']); ?></h4>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="card bg-light mb-4">
                    <div class="card-body">
                        <h6 class="text-muted mb-2">
                            <i class="bi bi-envelope"></i> 邮箱地址
                        </h6>
                        <h4><?php echo htmlspecialchars($user_info['email']); ?></h4>
                    </div>
                </div>
                
                <div class="card bg-light mb-4">
                    <div class="card-body">
                        <h6 class="text-muted mb-2">
                            <i class="bi bi-calendar"></i> 注册时间
                        </h6>
                        <h4><?php echo format_date($user_info['registeredAt']); ?></h4>
                    </div>
                </div>
                
                <hr class="my-4">
                
                <div class="d-grid gap-2">
                    <a href="library.php" class="btn btn-primary btn-lg">
                        <i class="bi bi-bookmark-star"></i> 查看游戏库
                    </a>
                    
                    <a href="logout.php" class="btn btn-outline-danger">
                        <i class="bi bi-box-arrow-right"></i> 退出登录
                    </a>
                </div>
            </div>
        </div>
    </div>
</div>

