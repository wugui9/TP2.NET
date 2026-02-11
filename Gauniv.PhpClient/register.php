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
    $confirm_password = $_POST['confirm_password'] ?? '';
    $first_name = $_POST['first_name'] ?? '';
    $last_name = $_POST['last_name'] ?? '';
    
    if (empty($email) || empty($password) || empty($first_name) || empty($last_name)) {
        $error = '请填写所有字段';
    } elseif ($password !== $confirm_password) {
        $error = '两次输入的密码不一致';
    } elseif (strlen($password) < 6) {
        $error = '密码长度至少为6个字符';
    } else {
        $response = api_request('/auth/register', 'POST', [
            'email' => $email,
            'password' => $password,
            'firstName' => $first_name,
            'lastName' => $last_name
        ]);
        
        if (isset($response['error']) && $response['error']) {
            $error = $response['message'] ?? '注册失败';
            if (isset($response['errors'])) {
                $error .= ': ' . implode(', ', $response['errors']);
            }
        } elseif (isset($response['message'])) {
            $success = $response['message'];
            $_SESSION['message'] = '注册成功！请登录';
            $_SESSION['message_type'] = 'success';
            header('Location: login.php');
            exit;
        }
    }
}

$page_title = '用户注册';
require_once 'includes/header.php';
?>

<div class="row justify-content-center">
    <div class="col-md-6 col-lg-5">
        <div class="card shadow-lg">
            <div class="card-body p-5">
                <div class="text-center mb-4">
                    <i class="bi bi-person-plus-fill" style="font-size: 3rem; color: #5865F2;"></i>
                    <h2 class="mt-3">注册 Gauniv</h2>
                    <p class="text-muted">创建您的游戏账户</p>
                </div>
                
                <?php if ($error): ?>
                    <div class="alert alert-danger">
                        <i class="bi bi-exclamation-triangle"></i> <?php echo htmlspecialchars($error); ?>
                    </div>
                <?php endif; ?>
                
                <form method="POST" action="">
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label for="first_name" class="form-label">
                                <i class="bi bi-person"></i> 名字
                            </label>
                            <input type="text" class="form-control" id="first_name" 
                                   name="first_name" required
                                   value="<?php echo htmlspecialchars($_POST['first_name'] ?? ''); ?>"
                                   placeholder="张">
                        </div>
                        <div class="col-md-6">
                            <label for="last_name" class="form-label">
                                <i class="bi bi-person"></i> 姓氏
                            </label>
                            <input type="text" class="form-control" id="last_name" 
                                   name="last_name" required
                                   value="<?php echo htmlspecialchars($_POST['last_name'] ?? ''); ?>"
                                   placeholder="三">
                        </div>
                    </div>
                    
                    <div class="mb-3">
                        <label for="email" class="form-label">
                            <i class="bi bi-envelope"></i> 邮箱地址
                        </label>
                        <input type="email" class="form-control" id="email" 
                               name="email" required
                               value="<?php echo htmlspecialchars($_POST['email'] ?? ''); ?>"
                               placeholder="your@email.com">
                    </div>
                    
                    <div class="mb-3">
                        <label for="password" class="form-label">
                            <i class="bi bi-lock"></i> 密码
                        </label>
                        <input type="password" class="form-control" 
                               id="password" name="password" required
                               placeholder="至少6个字符">
                        <small class="form-text text-muted">密码长度至少为6个字符</small>
                    </div>
                    
                    <div class="mb-3">
                        <label for="confirm_password" class="form-label">
                            <i class="bi bi-lock-fill"></i> 确认密码
                        </label>
                        <input type="password" class="form-control" 
                               id="confirm_password" name="confirm_password" required
                               placeholder="再次输入密码">
                    </div>
                    
                    <div class="d-grid gap-2">
                        <button type="submit" class="btn btn-primary btn-lg">
                            <i class="bi bi-person-plus"></i> 注册
                        </button>
                    </div>
                </form>
                
                <hr class="my-4">
                
                <div class="text-center">
                    <p class="mb-0">已有账号？ 
                        <a href="login.php" class="text-decoration-none">
                            <i class="bi bi-box-arrow-in-right"></i> 立即登录
                        </a>
                    </p>
                </div>
            </div>
        </div>
    </div>
</div>

