document.addEventListener("DOMContentLoaded", async () => {
    const usernameEl = document.getElementById("username");
    const emailEl = document.getElementById("email");
    const saveBtn = document.querySelector(".body-info-block button"); // кнопка "Save changes"
    const message = document.getElementById("message");

    // === Load user data ===
    try {
        const res = await fetch("/user/profile-body-info");
        if (res.ok) {
            const data = await res.json();
            usernameEl.textContent = data.username || "—";
            emailEl.textContent = data.email || "—";

            if (data.bodyInfo) {
                document.getElementById("height").value = data.bodyInfo.Height || "";
                document.getElementById("weight").value = data.bodyInfo.Weight || "";
                document.getElementById("age").value = data.bodyInfo.Age || "";
                document.getElementById("gender").value = data.bodyInfo.Gender || "";
                document.getElementById("activityLevel").value = data.bodyInfo.ActivityLevel || "";
                document.getElementById("goal").value = data.bodyInfo.Goal || "";
            }
        } else {
            usernameEl.textContent = "Unknown user";
            emailEl.textContent = "Unknown";
        }
    } catch (err) {
        console.error("Failed to load profile:", err);
        usernameEl.textContent = "Error";
        emailEl.textContent = "Error";
    }

    // === Validation & Save ===
    saveBtn.addEventListener("click", async () => {
        const fields = {
            height: parseFloat(document.getElementById("height").value),
            weight: parseFloat(document.getElementById("weight").value),
            age: parseInt(document.getElementById("age").value),
            gender: document.getElementById("gender").value,
            activityLevel: document.getElementById("activityLevel").value,
            goal: document.getElementById("goal").value
        };

        const errors = [];

        // Reset styles
        document.querySelectorAll(".body-info-block input, .body-info-block select")
            .forEach(el => el.classList.remove("invalid", "valid"));

        // Validate all at once
        if (!fields.height || fields.height < 50 || fields.height > 250) {
            errors.push("Height must be between 50 and 250 cm.");
            document.getElementById("height").classList.add("invalid");
        } else document.getElementById("height").classList.add("valid");

        if (!fields.weight || fields.weight < 20 || fields.weight > 300) {
            errors.push("Weight must be between 20 and 300 kg.");
            document.getElementById("weight").classList.add("invalid");
        } else document.getElementById("weight").classList.add("valid");

        if (!fields.age || fields.age < 10 || fields.age > 120) {
            errors.push("Age must be between 10 and 120 years.");
            document.getElementById("age").classList.add("invalid");
        } else document.getElementById("age").classList.add("valid");

        if (!fields.gender) {
            errors.push("Please select your gender.");
            document.getElementById("gender").classList.add("invalid");
        } else document.getElementById("gender").classList.add("valid");

        if (!fields.activityLevel) {
            errors.push("Please select your activity level.");
            document.getElementById("activityLevel").classList.add("invalid");
        } else document.getElementById("activityLevel").classList.add("valid");

        if (!fields.goal) {
            errors.push("Please select your goal.");
            document.getElementById("goal").classList.add("invalid");
        } else document.getElementById("goal").classList.add("valid");

        if (errors.length > 0) {
            message.style.display = "block";
            message.innerHTML = `
        <div style="color:#ea9a34; margin-top:8px;">
            Please fix the following errors:
            <ul style="margin-top:4px; padding-left:20px;">
                ${errors.map(e => `${e}`).join("")}
            </ul>
        </div>
    `;
            return;
        }
        message.style.display = "none";
        // --- Save to backend ---
        try {
            const res = await fetch("/user/add-body-info", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(fields)
            });
            if (res.ok) {
                message.style.display = "block";
                message.textContent = "Changes saved successfully!";
                message.style.color = "#2e7d32";
            } else {
                message.style.display = "block";
                const err = await res.text();
                message.textContent = "Error: " + err;
                message.style.color = "#d32f2f";
            }
        } catch (err) {
            console.error(err);
            message.style.display = "block";
            message.textContent = "Failed to connect to the server ";
            message.style.color = "#d32f2f";
        }
    });
});

// === Quit button ===
const logoutBtn = document.getElementById("logoutBtn");
logoutBtn.addEventListener('click', async () => {
    try {
        const response = await fetch('/user/quit', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        window.location.href = "/meal_plan";
    } catch (err) {
        console.error(err);
        alert("Failed to connect to the server");
    }
});

