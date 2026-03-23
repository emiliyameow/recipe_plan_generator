

// ингредиенты и единицы измерения
const ingredientsData = [
    { id: 1, name: 'Flour' },
    { id: 2, name: 'Sugar' },
    { id: 3, name: 'Egg' },
    { id: 4, name: 'Butter' }
];

const unitsData = [
    { id: 1, name: 'g' },
    { id: 2, name: 'ml' },
    { id: 3, name: 'pcs' }
];

/* проверка вынесена на сервер
const currentUserId = localStorage.getItem("userId");
const currentUsername = localStorage.getItem("username");

if (!currentUserId || !currentUsername) {
    alert("❌ Please log in first!");
    window.location.href = "/login";
}
*/


const logoutBtn = document.getElementById("logoutBtn");
logoutBtn.addEventListener('click', async () => {
    try {
        const response = await fetch('/user/quit', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        /*
        if (response.ok) {
            alert("✅ All private recipes are now public!");
            document.querySelectorAll('#myRecipes .recipe-card .visibility').forEach(el => {
                el.textContent = 'Public recipe';
            });
        } else {
            const err = await response.text();
            alert("❌ Error: " + err);
        }*/
        window.location.href = "/meal_plan";
    } catch (err) {
        console.error(err);
        alert("⚠️ Failed to connect to the server");
    }
});


// базовые элементы страницы
const ingredientsListDiv = document.getElementById('ingredientsList');
const addIngredientBtn = document.getElementById('addIngredientBtn');
const myRecipesDiv = document.getElementById('myRecipes');
const otherRecipesDiv = document.getElementById('otherRecipes');

// отрисовка строки ингредиента
function renderIngredientRow(ingredientName = '', quantity = '', unitId = '') {
    const row = document.createElement('div');
    row.className = 'ingredient-row';

    // поле названия
    const input = document.createElement('input');
    input.type = 'text';
    input.className = 'ingredient-input';
    input.placeholder = 'Ingredient';
    input.value = ingredientName;

    // поле количества
    const qtyInput = document.createElement('input');
    qtyInput.type = 'number';
    qtyInput.className = 'ingredient-quantity';
    qtyInput.placeholder = 'Quantity';
    qtyInput.value = quantity;

    // выпадающий список единиц
    const unitSelect = document.createElement('select');
    unitSelect.className = 'ingredient-unit';
    unitsData.forEach(u => {
        const option = document.createElement('option');
        option.value = u.id;
        option.textContent = u.name;
        if (u.id == unitId) option.selected = true;
        unitSelect.appendChild(option);
    });

    // кнопка удалить строку
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.textContent = '×';
    btn.addEventListener('click', () => row.remove());

    row.append(input, qtyInput, unitSelect, btn);
    ingredientsListDiv.appendChild(row);
}

// кнопка добавить ингредиент
addIngredientBtn.addEventListener('click', () => renderIngredientRow());

// сбор ингредиентов из формы
function getRecipeIngredients() {
    const rows = document.querySelectorAll('.ingredient-row');
    const arr = [];

    rows.forEach(r => {
        const name = r.querySelector('.ingredient-input').value.trim();
        const quantity = parseFloat(r.querySelector('.ingredient-quantity').value);
        const unitId = parseInt(r.querySelector('.ingredient-unit').value);
        const ing = ingredientsData.find(i => i.name.toLowerCase() === name.toLowerCase());
        const ingredientId = ing ? ing.id : 0;

        if (name && !isNaN(quantity)) {
            arr.push({ ingredientId, name, quantity, unitId });
        }
    });

    return arr;
}

// обработчик добавления рецепта
document.getElementById('recipeForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const title = document.getElementById('recipeTitle').value.trim();
    const description = document.getElementById('recipeDescription').value.trim();
    const isPublic = document.getElementById('isPublic').checked;
    const ingredients = getRecipeIngredients();

    if (!title || ingredients.length === 0) {
        alert('⚠️ Please enter a title and at least one ingredient!');
        return;
    }

    // собираем данные рецепта
    const recipeData = {
        title,
        description,
        isPublic,
        //userId: parseInt(currentUserId),
        ingredients
    };

    try {
        // отправляем рецепт на сервер
        const response = await fetch('/save-recipe', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(recipeData)
        });

        if (response.ok) {
            const msg = await response.text();
            alert('✅ ' + msg);

            // создаём карточку рецепта на странице
            const card = document.createElement('div');
            card.className = 'recipe-card';
            card.innerHTML = `
                <h3>${title}</h3>
                ${!isPublic ? '<p class="visibility">Private recipe</p>' : '<p class="visibility">Public recipe</p>'}
                <ul>
                    ${ingredients.map(i => `<li>${i.name} — ${i.quantity} ${unitsData.find(u => u.id === i.unitId)?.name || ''}</li>`).join('')}
                </ul>
                <p>${description || 'Without description'}</p>
            `;
            myRecipesDiv.appendChild(card);

            // показываем секцию, если это первый рецепт
            const recipesSection = document.getElementById('recipesSection');
            const myRecipesHeader = recipesSection.querySelector('h2');

            recipesSection.style.display = 'block';

            // возвращаем сетку (если была скрыта)
            if (getComputedStyle(myRecipesDiv).display === 'none') {
                myRecipesDiv.style.display = '';
            }

            // обновляем счётчик рецептов
            const currentCount = parseInt(myRecipesHeader.textContent.match(/\d+/)?.[0] || 0);
            myRecipesHeader.textContent = `My recipes (${currentCount + 1})`;

            // очищаем форму
            e.target.reset();
            ingredientsListDiv.innerHTML = '';
            await fetchMyRecipes();
        } else {
            const err = await response.text();
            alert('❌ Error: ' + err);
        }
    } catch (err) {
        console.error('Error saving recipe:', err);
        alert('⚠️ Failed to connect to the server');
    }
})

// загрузка рецептов пользователя
async function fetchMyRecipes() {
    try {
        const response = await fetch('/get-my-recipes', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
            //body: JSON.stringify({ userId: parseInt(currentUserId)}) //убираем
        });
        if (!response.ok) throw new Error('Failed to fetch recipes');
        const recipes = await response.json();

        const myRecipesDiv = document.getElementById('myRecipes');
        const recipesSection = document.getElementById('recipesSection');
        const myRecipesHeader = recipesSection.querySelector('h2');
        const makeAllPublicBtn = document.getElementById('makeAllPublicBtn');

        myRecipesDiv.innerHTML = '';
        let hasPrivate = false;

        recipes.forEach(r => {
            if (!r.isPublic) hasPrivate = true;

            const card = document.createElement('div');
            card.className = 'recipe-card';
            card.innerHTML = `
                <h3>${r.title}</h3>
                ${!r.isPublic ? '<p class="visibility">Private recipe</p>' : '<p class="visibility">Public recipe</p>'}
                <ul>
                    ${(r.ingredients || []).map(i =>
                `<li>${i.name} — ${i.quantity} ${unitsData.find(u => u.id === i.unitId)?.name || ''}</li>`
            ).join('')}
                </ul>
                <p>${r.description || 'Without description'}</p>
            `;
            myRecipesDiv.appendChild(card);
        });

        // показываем кнопку "сделать все публичными", если есть приватные
        if (makeAllPublicBtn) {
            makeAllPublicBtn.style.display = hasPrivate ? 'block' : 'none';
        }

        // если рецептов нет — скрываем секцию
        if (recipes.length === 0) {
            myRecipesHeader.style.display = 'none';
            myRecipesDiv.style.display = 'none';
            makeAllPublicBtn.style.display = 'none';
        } else {
            myRecipesHeader.style.display = 'block';
            myRecipesDiv.style.display = 'flex';
            myRecipesHeader.textContent = `My recipes (${recipes.length})`;
        }

    } catch (err) {
        console.error(err);
        alert('⚠️ Failed to load recipes');
    }
}

// загрузка публичных рецептов
async function fetchPublicRecipes() {
    try {
        //const currentUserId = parseInt(localStorage.getItem("userId") || 0);
        const response = await fetch('/get-public-recipes', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
            //body: JSON.stringify({ userId: parseInt(currentUserId)}) //убираем
        });
        
        if (!response.ok) throw new Error('Failed to fetch public recipes');

        const recipes = await response.json();
        const otherRecipesDiv = document.getElementById('otherRecipes');
        const otherTitle = document.getElementById('otherTitle');

        otherRecipesDiv.innerHTML = '';

        if (!recipes || recipes.length === 0) {
            otherTitle.style.display = 'none';
            return;
        }

        otherTitle.style.display = 'block';
        recipes.forEach(r => {
            const card = document.createElement('div');
            card.className = 'recipe-card';
            card.innerHTML = `
                <h3>${r.Title}</h3>
                <p class="author">Author: ${r.Username || 'Unknown'}</p>
                <ul>
                    ${(r.Ingredients || []).map(i =>
                `<li>${i.Name} — ${i.Quantity} ${unitsData.find(u => u.id === i.UnitId)?.name || ''}</li>`
            ).join('')}
                </ul>
                <p>${r.Description || 'Without description'}</p>
            `;
            otherRecipesDiv.appendChild(card);
        });
        otherRecipesDiv.style.display = 'flex';
        
    } catch (err) {
        console.error(err);
        alert('⚠️ Failed to load public recipes');
    }
}

// кнопка сделать все публичным
const makeAllPublicBtn = document.getElementById('makeAllPublicBtn');
makeAllPublicBtn.addEventListener('click', async () => {
    if (!confirm("Are you sure? All private recipes will become public.")) return;

    try {
        const response = await fetch('/make-all-public', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
            //body: JSON.stringify({ userId: parseInt(currentUserId)}) //убираем
        });

        if (response.ok) {
            alert("✅ All private recipes are now public!");
            document.querySelectorAll('#myRecipes .recipe-card .visibility').forEach(el => {
                el.textContent = 'Public recipe';
            });
        } else {
            const err = await response.text();
            alert("❌ Error: " + err);
        }
    } catch (err) {
        console.error(err);
        alert("⚠️ Failed to connect to the server");
    }
});

// запуск при загрузке страницы
window.addEventListener("DOMContentLoaded", async () => {
    await loadUser();
    await fetchMyRecipes();
    await fetchPublicRecipes();
});

async function loadUser() {
    try {
        const response = await fetch('/user/me', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
            //body: JSON.stringify({ userId: parseInt(currentUserId)}) //убираем
        });
        if (!response.ok) {
            alert("❌ Please log in first!");
            window.location.href = "/login";
            return;
        }

        const username = await response.text();
        document.getElementById("userInfo").textContent = username;

    } catch (err) {
        console.error("Ошибка при загрузке пользователя:", err);
        //alert("⚠️ Ошибка подключения к серверу");
    }
}
