// Вход
const loginForm = document.getElementById('loginForm');

if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const login = document.getElementById('login').value.trim();
        const password = document.getElementById('password').value.trim();

        const msg = document.getElementById('loginMessage');
        const afterLogin = document.getElementById('afterLogin');
        const signupTransfer = document.getElementById('signupTransfer');
        try {
            const response = await fetch("/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ login, password })
            }); // для безопасности, изменяем данные 

            if (response.ok) {
                const data = await response.json();
                msg.style.color = '#2e7d32';
                msg.textContent = `Welcome, ${data.username}!`;
                afterLogin.style.display = 'block';
                signupTransfer.style.display = 'none';
                
                // сохраняем userId в localStorage
                //localStorage.setItem("userId", data.userId);
                //localStorage.setItem("username", data.username);
                //localStorage.setItem("userRole", data.userRole);

            } else {
                const err = await response.json();
                msg.textContent = `Error: ${err.error}`;
                msg.style.color = '#d32f2f';
                afterLogin.style.display = 'none';
            }
        } catch (err) {
            msg.textContent = "Could not connect to the server";
            msg.style.color = '#d32f2f';
            console.error(err);
        }
    });
}

