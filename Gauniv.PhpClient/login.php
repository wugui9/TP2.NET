<?php
require_once 'config.php';

// 如果已登录，重定向到首页
if (is_logged_in()) {
    header('Location: index.php');
    exit;
}

$error = '';
$success = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $email = $_POST['email'] ?? '';
    $password = $_POST['password'] ?? '';
    $remember = isset($_POST['remember']);
    
    if (empty($email) || empty($password)) {
        $error = '请填写所有字段';
    } else {
        $response = api_request('/auth/login', 'POST', [
            'email' => $email,
            'password' => $password,
            'rememberMe' => $remember
        ]);
        
        if (isset($response['error']) && $response['error']) {
            $error = $response['message'] ?? '登录失败';
        } elseif (isset($response['success']) && $response['success']) {
            // 保存用户信息到session
            $_SESSION['user'] = [
                'email' => $response['email'],
                'firstName' => $response['firstName'] ?? '',
                'lastName' => $response['lastName'] ?? ''
            ];
            
            $_SESSION['message'] = '登录成功！欢迎回来 ' . ($response['firstName'] ?? $response['email']);
            $_SESSION['message_type'] = 'success';
            
            header('Location: index.php');
            exit;
        } else {
            $error = '登录失败，请检查邮箱和密码';
        }
    }
}

$page_title = '用户登录';
require_once 'includes/header.php';
?>

<div class="row justify-content-center">
    <div class="col-md-6 col-lg-5">
        <div class="card shadow-lg">
            <div class="card-body p-5">
                <div class="text-center mb-4">
                    <i class="bi bi-controller" style="font-size: 3rem; color: #5865F2;"></i>
                    <h2 class="mt-3">登录 Gauniv</h2>
                    <p class="text-muted">访问您的游戏库</p>
                </div>
                
                <?php if ($error): ?>
                    <div class="alert alert-danger">
                        <i class="bi bi-exclamation-triangle"></i> <?php echo htmlspecialchars($error); ?>
                    </div>
                <?php endif; ?>
                
                <?php if ($success): ?>
                    <div class="alert alert-success">
                        <i class="bi bi-check-circle"></i> <?php echo htmlspecialchars($success); ?>
                    </div>
                <?php endif; ?>
                
                <form method="POST" action="">
                    <div class="mb-3">
                        <label for="email" class="form-label">
                            <i class="bi bi-envelope"></i> 邮箱地址
                        </label>
                        <input type="email" class="form-control form-control-lg" id="email" 
                               name="email" required 
                               value="<?php echo htmlspecialchars($_POST['email'] ?? ''); ?>"
                               placeholder="your@email.com">
                    </div>
                    
                    <div class="mb-3">
                        <label for="password" class="form-label">
                            <i class="bi bi-lock"></i> 密码
                        </label>
                        <input type="password" class="form-control form-control-lg" 
                               id="password" name="password" required
                               placeholder="••••••••">
                    </div>
                    
                    <div class="mb-3 form-check">
                        <input type="checkbox" class="form-check-input" id="remember" name="remember">
                        <label class="form-check-label" for="remember">
                            记住我
                        </label>
                    </div>
                    
                    <div class="d-grid gap-2">
                        <button type="submit" class="btn btn-primary btn-lg">
                            <i class="bi bi-box-arrow-in-right"></i> 登录
                        </button>
                    </div>
                </form>
                
                <hr class="my-4">
                
                <div class="text-center">
                    <p class="mb-0">还没有账号？ 
                        <a href="register.php" class="text-decoration-none">
                            <i class="bi bi-person-plus"></i> 立即注册
                        </a>
                    </p>
                </div>
            </div>
        </div>
    </div>
</div>

