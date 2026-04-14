/**
 * Generates a strong, random password.
 * Includes uppercase, lowercase, numbers, and special characters.
 */
function generateStrongPassword() {
    const chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+~";
    let password = "";
    for (let i = 0; i < 12; i++) {
        password += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    // Ensure at least one number and one special char is present (basic enforcement)
    password += Math.floor(Math.random() * 10);
    password += "!@#$"[Math.floor(Math.random() * 4)];
    return password;
}

/**
 * Evaluates password strength and updates the UI meter.
 */
function checkPasswordStrength(password) {
    let strength = 0;
    if (password.length >= 8) strength += 1;
    if (password.match(/[a-z]+/)) strength += 1;
    if (password.match(/[A-Z]+/)) strength += 1;
    if (password.match(/[0-9]+/)) strength += 1;
    if (password.match(/[$@#&!]+/)) strength += 1;

    const meter = document.getElementById('password-strength-meter');
    const text = document.getElementById('password-strength-text');

    if (!meter || !text) return;

    meter.className = 'progress-bar'; // reset classes

    switch (strength) {
        case 0: case 1: case 2:
            meter.classList.add('bg-danger');
            meter.style.width = '25%';
            text.innerText = "Weak";
            break;
        case 3:
            meter.classList.add('bg-warning');
            meter.style.width = '50%';
            text.innerText = "Fair";
            break;
        case 4:
            meter.classList.add('bg-info');
            meter.style.width = '75%';
            text.innerText = "Good";
            break;
        case 5:
            meter.classList.add('bg-success');
            meter.style.width = '100%';
            text.innerText = "Strong";
            break;
    }
}

// Bind events when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    const pwdInput = document.getElementById('InputPassword');
    const genBtn = document.getElementById('btn-generate-pwd');

    if (pwdInput) {
        pwdInput.addEventListener('input', function () {
            checkPasswordStrength(this.value);
        });
    }

    if (genBtn && pwdInput) {
        genBtn.addEventListener('click', function () {
            const newPwd = generateStrongPassword();
            pwdInput.value = newPwd;
            pwdInput.type = "text"; // Show password temporarily
            checkPasswordStrength(newPwd);

            // Revert to password type after 3 seconds
            setTimeout(() => { pwdInput.type = "password"; }, 3000);
        });
    }
});