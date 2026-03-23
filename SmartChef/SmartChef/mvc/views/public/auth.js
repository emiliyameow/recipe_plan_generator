const registerForm = document.getElementById('registerForm');
const password = document.getElementById('password');
const passwordConfirm = document.getElementById('password_confirmation');
const msg = document.getElementById('registerMessage');

const username = document.getElementById('username');
const email = document.getElementById('email');
const login = document.getElementById('login');

const conditions = {
    length: document.getElementById('length'),
    lowercase: document.getElementById('lowercase'),
    uppercase: document.getElementById('uppercase'),
    number: document.getElementById('number')
};

// 🔹 Регулярные выражения для валидации
const regex = {
    username: /^[A-Za-z0-9_]{5,20}$/,
    login: /^[A-Za-z0-9_]{5,20}$/,
    email: /^[\w.-]+@[\w.-]+\.\w{2,}$/,
    name: /^[A-Za-z]+$/
};

// Проверка пароля
function validatePassword() {
    const val = password.value;
    let valid = true;

    if (val.length >= 8) conditions.length.className = 'valid';
    else { conditions.length.className = 'invalid'; valid = false; }

    if (/[a-z]/.test(val)) conditions.lowercase.className = 'valid';
    else { conditions.lowercase.className = 'invalid'; valid = false; }

    if (/[A-Z]/.test(val)) conditions.uppercase.className = 'valid';
    else { conditions.uppercase.className = 'invalid'; valid = false; }

    if (/\d/.test(val)) conditions.number.className = 'valid';
    else { conditions.number.className = 'invalid'; valid = false; }

    return valid;
}

// Универсальная проверка полей
function validateField(field, pattern) {
    if (!field.value.trim()) {
        field.classList.add('invalid');
        field.classList.remove('valid');
        return false;
    }
    if (!pattern.test(field.value.trim())) {
        field.classList.add('invalid');
        field.classList.remove('valid');
        return false;
    }
    field.classList.remove('invalid');
    field.classList.add('valid');
    return true;
}

function validateForm() {
    const usernameValid = validateField(username, regex.username);
    const loginValid = validateField(login, regex.login);
    const emailValid = validateField(email, regex.email);
    const passwordValid = validatePassword();
    const passwordsMatch = password.value && password.value === passwordConfirm.value;

    if (!passwordsMatch) {
        passwordConfirm.classList.add('invalid');
    } else {
        passwordConfirm.classList.remove('invalid');
        passwordConfirm.classList.add('valid');
    }

    const allValid = usernameValid && loginValid && emailValid && passwordValid && passwordsMatch;
    // submitBtn.disabled = !allValid;

    return allValid;
}

// Обновляем валидацию при вводе
[username, email, login, password, passwordConfirm].forEach(f => {
    f.addEventListener('input', validateForm);
});

// Отправка формы
if (registerForm) {
    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        msg.textContent = '';
        
        if (!validateForm()) {
            msg.style.color = '#d32f2f';
            msg.textContent = 'Please correct the highlighted fields.';
            return;
        }
        const afterLogin = document.getElementById('afterLogin');
        const loginTransfer = document.getElementById('loginTransfer');
        
        const body = {
            username: username.value.trim(),
            email: email.value.trim(),
            login: login.value.trim(),
            password: password.value.trim()
        };

        try {
            const response = await fetch("/signup", {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if (response.ok) {
                msg.style.color = '#2e7d32';
                msg.textContent = 'You were successfully signed up!';
                registerForm.style.display = 'none';
                
                afterLogin.style.display = 'block';
                loginTransfer.style.display = 'none';
            } else {
                const err = await response.json();
                msg.style.color = '#d32f2f';
                msg.textContent = `Error: ${err.error}`;
            }
        } catch (err) {
            msg.style.color = '#d32f2f';
            msg.textContent = 'Could not connect to the server';
            console.error(err);
        }
    });
}
