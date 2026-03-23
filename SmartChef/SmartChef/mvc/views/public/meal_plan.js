const logoutBtn = document.getElementById("logoutBtn");
const generateArea = document.getElementById("generate-area");
const generatePlanBtn = document.getElementById("generatePlanBtn");
const message = document.getElementById('formMessage');

let currentProfileData = null;
let errors = []
let requestForPlan =
    {
        Calories: 0,
        Proteins: 0,
        Fats: 0,
        Carbs: 0,

        BreakfastTime: 0,
        LunchDinnerTime: 0,
        Cuisine: [],
        Intolerances: [],
        Diet: "",

        RecipePlanExist: false,
        BreakfastId: 0,
        LunchId: 0,
        DinnerId: 0
    };

class Recipe {
    constructor({
        Id,
        Image,
        ImageType,
        Title,
        ReadyInMinutes,
        Servings,
        SourceUrl,
        Calories,
        Proteins,
        Carbs,
        Fats,
        PercentProtein,
        PercentFat,
        PercentCarbs,
        DishTypes,
        Summary
    }) {
        this.Id = Id;
        this.Image = Image;
        this.ImageType = ImageType;
        this.Title = Title;
        this.ReadyInMinutes = ReadyInMinutes;
        this.Servings = Servings;
        this.SourceUrl = SourceUrl;
        this.Calories = Calories;
        this.Proteins = Proteins;
        this.Carbs = Carbs;
        this.Fats = Fats;
        this.PercentProtein = PercentProtein;
        this.PercentFat = PercentFat;
        this.PercentCarbs = PercentCarbs;
        this.DishTypes = DishTypes;
        this.Summary = Summary;
    }
}

function clearCaloriesInfo() {
    const caloriesMessage = document.getElementById("caloriesMessage");
    const generateArea = document.getElementById("generate-area");
    const planMessage = document.getElementById("planMessage");

    if (caloriesMessage) {
        caloriesMessage.style.display = "none";
        caloriesMessage.textContent = "";
    }
    if (generateArea) {
        generateArea.style.display = "none";
    }
    if (planMessage) {
        planMessage.textContent = "";
    }
    if (message) {
        message.textContent = "";
    }
}


async function loadUser() {
    try {
        const response = await fetch('/user/me', {
            method: 'GET',
            headers: {'Content-Type': 'application/json'}
        });
        if (response.ok) {
            const data = await response.json();
            const userInfo = document.getElementById("userInfo");

            userInfo.textContent = "\uD83D\uDC64 - " + data;
            userInfo.addEventListener('click', () => {
                window.location.href = `/user_profile`;
            });

            document.getElementById("un-authentificated-user-panel").style.display = 'none';
            document.getElementById("authentificated-user-panel").style.display = "block";

            return data;
        }

        return null;
    } catch (err) {
        console.error("Ошибка при загрузке пользователя:", err);

        return null;
    }
}

async function calculateCalories({request}) {
    try {
        message.textContent = 'Loading daily plan...';
        message.style.color = '#555';

        console.log(JSON.stringify(request));
        const response = await fetch('/api/calculate-calories', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(request)
        });

        if (response.ok) {
            const result = await response.json();
            message.style.color = '#2e7d32';

            const infoString = "Calories were calculated successfully! Approximate macronutrient intake for you:";
            const caloriesString =
                `Calories: ${Math.round(result.Calories)} | ` +
                `Proteins: ${Math.round(result.Proteins)} g | ` +
                `Fats: ${Math.round(result.Fats)} g | ` +
                `Carbohydrates: ${Math.round(result.Carbs)} g `;

            requestForPlan.Calories = result.Calories;
            requestForPlan.Proteins = result.Proteins;
            requestForPlan.Fats = result.Fats;
            requestForPlan.Carbs = result.Carbs;

            message.textContent = infoString;

            const caloriesMessage = document.getElementById("caloriesMessage");
            caloriesMessage.textContent = caloriesString;
            caloriesMessage.style.display = "block";
            generateArea.style.display = "block";

            console.log('Server response:', result);

            return result;
        }

        throw new Error(`Server error ${response.status}`);
    } catch (err) {
        console.error('Error sending data:', err);
        message.textContent = 'Error calculating calories. Try again later';
        message.style.color = '#d32f2f';
        
        return null;
    }
}

async function generatePlan()  {
    const messageAfterGeneration = document.getElementById("messageAfterGeneration");

    try {
        const response = await fetch('/user/generate-plan', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify(requestForPlan)
        });

        if (response.ok) {
            return await response.json();
        }

        const err = await response.json();

        messageAfterGeneration.style.color = '#d32f2f';
        messageAfterGeneration.style.display = 'block';
        messageAfterGeneration.textContent = `Error: ${err.error}`;
        
        generatePlanBtn.disabled = false;

        return null;
    } catch (err) {
        messageAfterGeneration.style.color = '#d32f2f';
        messageAfterGeneration.style.display = 'block';
        messageAfterGeneration.textContent = `Failed to generate plan: ${err.error}`;
        console.error(err);

        return null;
    }
}

document.addEventListener('DOMContentLoaded', async ()=> {
    const userData = await loadUser();
    const form = document.getElementById('nutritionForm');

    if (userData == null) {
        form.style.display = 'none';
        document.getElementById('header1').style.display = 'none';
        document.getElementById('infoForUnathorised').style.display = 'block';
        document.getElementById('generationRules').style.display = 'none';
    } else {
        document.getElementById('infoForUnathorised').style.display = 'none';
        document.getElementById('generationRules').style.display = 'block';
    }

    const radios = document.querySelectorAll('input[name="source"]');
    const manualTypeRadios = document.querySelectorAll('input[name="manualType"]');
    const profile = document.getElementById('profileInfo');

    const manual = document.getElementById('manualInput');
    const caloriesGoal = document.getElementById('manualCaloriesGoal');
    const macros = document.getElementById('manualBju');

    try {
        const res = await fetch("/user/profile-body-info");
        if (res.ok) {
            const data = await res.json();

            currentProfileData = data;

            const loginInfo = document.getElementById('loginInfo');
            const loginButton = document.getElementById("loginBtnInForm");
            loginInfo.style.display = "none";
            loginButton.style.display = "none";

            const profileInfoBody = document.getElementById("profileInfoBody");

            if (data.bodyInfo) {
                // создаём строку с полной информацией о пользователе
                const infoString =
                    `Weight: ${data.bodyInfo.Weight} kg | ` +
                    `Height: ${data.bodyInfo.Height} cm | ` +
                    `Age: ${data.bodyInfo.Age} | ` +
                    `Gender: ${data.bodyInfo.Gender} | ` +
                    `Activity Level: ${data.bodyInfo.ActivityLevel} | ` +
                    `Goal: ${data.bodyInfo.Goal}`;


                // выводим в div

                profileInfoBody.textContent = infoString;
                document.getElementById("profileInfoBodyPanel").style.display = "block";
            } else {

                // выводим в div

                profileInfoBody.textContent = "No data about your body";
                document.getElementById("profileButton").textContent = "Enter in profile";
                document.getElementById("profileInfoBodyPanel").style.display = "block";
            }
        } else {

            document.getElementById("profileInfoBody").style.display = "none";
        }
    } catch (err) {

        document.getElementById("profileInfoBody").textContent = err.message;
    }

    // toggle source
    radios.forEach(r => {
        r.addEventListener('change', () => {
            if (r.value === 'profile' && r.checked) {
                profile.classList.remove('hidden');
                manual.classList.add('hidden');
            } else if (r.value === 'manual' && r.checked) {
                manual.classList.remove('hidden');
                profile.classList.add('hidden');
            } else {
                profile.classList.add('hidden');
                manual.classList.add('hidden');
            }
        });
    });

    // toggle manual type
    manualTypeRadios.forEach(r => {
        r.addEventListener('change', () => {
            if (r.value === 'calories') {
                caloriesGoal.classList.remove('hidden');
                macros.classList.add('hidden');
            } else {
                caloriesGoal.classList.add('hidden');
                macros.classList.remove('hidden');
            }
        });
    });

    // toggle meal time boxes
    // переключение видимости полей пищи

    const breakfastCheckbox = document.getElementById('useBreakfastTime');
    const breakfastBox = document.getElementById('breakfastTimeBox');

    const lunchDinnerCheckbox = document.getElementById('useLunchDinnerTime');
    const lunchDinnerBox = document.getElementById('lunchDinnerTimeBox');

    // --- Завтрак ---
    if (breakfastCheckbox && breakfastBox) {
        // Устанавливаем начальное состояние
        breakfastBox.classList.toggle('hidden', !breakfastCheckbox.checked);

        // При изменении состояния чекбокса
        breakfastCheckbox.addEventListener('change', () => {
            breakfastBox.classList.toggle('hidden', !breakfastCheckbox.checked);
        });
    }

    // --- Обед + Ужин (одно время) ---
    if (lunchDinnerCheckbox && lunchDinnerBox) {
        lunchDinnerBox.classList.toggle('hidden', !lunchDinnerCheckbox.checked);

        lunchDinnerCheckbox.addEventListener('change', () => {
            lunchDinnerBox.classList.toggle('hidden', !lunchDinnerCheckbox.checked);
        });
    }

    // --- Обработка формы --- 
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        message.textContent = '';
        message.style.color = '#ea9a34';

        errors = [];

        const source = document.querySelector('input[name="source"]:checked')?.value;
        const cuisine = Array.from(document.getElementById('cuisine').selectedOptions).map(o => o.value);

        if (cuisine.length < 4 && cuisine.length > 0) errors.push("You should select at least 4 cuisines to get varied meal plan");

        const intolerances = Array.from(document.getElementById('intolerances').selectedOptions).map(o => o.value);
        if (intolerances.length >= 5) errors.push("It is better to lower count of your intolerances for normal plan generation (4)");

        const diet = document.getElementById('diet').value;

        if (document.getElementById('useBreakfastTime').checked &&
            +document.getElementById('breakfastTime').value <= 0) errors.push("Breakfast time should be more than 0");
        
        if (document.getElementById('useLunchDinnerTime').checked &&
            +document.getElementById('lunchDinnerTime').value<= 0) errors.push("Lunch/dinner time should be more than 0");

        let request = {
            TypeFlag: null,
            Calories: null,
            Proteins: null,
            Fats: null,
            Carbs: null,
            Goal: null,
            UserBodyInformation: null,
        };


        if (source === 'manual') {
            const manualType = document.querySelector('input[name="manualType"]:checked')?.value;

            if (manualType === 'calories') {
                const calories = Number(document.getElementById('manualCalories').value);
                const goal = document.getElementById('manualGoal').value;

                if (!calories || calories < 1200 || calories > 4000) {
                    errors.push("Calories must be between 1200 and 4000.");
                }
                if (!goal) {
                    errors.push("Please select a goal.");
                }

                request.TypeFlag = 'calories';
                request.Calories = calories;
                request.Goal = goal;
            } else if (manualType === 'macronutrients') {
                const protein = Number(document.getElementById('manualProtein').value);
                const fats = Number(document.getElementById('manualFats').value);
                const carbs = Number(document.getElementById('manualCarbs').value);

                // Проверка положительных чисел
                if (protein <= 0) errors.push("Protein must be greater than 0.");
                if (fats <= 0) errors.push("Fats must be greater than 0.");
                if (carbs <= 0) errors.push("Carbs must be greater than 0.");

                const totalCalories = protein * 4 + fats * 9 + carbs * 4;
                if (totalCalories < 1200 || totalCalories > 4000) errors.push("Total calories from macros should be between 1200 and 4000.");

                const proteinPct = (protein * 4 / totalCalories) * 100;
                const fatsPct = (fats * 9 / totalCalories) * 100;
                const carbsPct = (carbs * 4 / totalCalories) * 100;

                if (proteinPct < 10 || proteinPct > 35) errors.push("Protein percentage should be 10–35% of total calories.");
                if (fatsPct < 20 || fatsPct > 35) errors.push("Fats percentage should be 20–35% of total calories.");
                if (carbsPct < 45 || carbsPct > 65) errors.push("Carbs percentage should be 45–65% of total calories.");

                request.TypeFlag = 'macronutrients';
                request.Proteins = protein;
                request.Fats = fats;
                request.Carbs = carbs;
            }
        } else if (source === 'profile') {
            if (!currentProfileData) {
                errors.push("Please login to use profile data or select another source.");
            } else if (!currentProfileData.bodyInfo) {
                errors.push("No profile data found. Please enter profile data or select another source");
            } else {
                request.TypeFlag = 'profile';
                request.UserBodyInformation = currentProfileData.bodyInfo;
            }

        } else {
            request.TypeFlag = 'without';
        }
        
        if (errors.length > 0) {
            message.innerHTML = errors.join('<br>');
            return;
        }

        requestForPlan.Cuisine = cuisine;
        requestForPlan.Intolerances = intolerances;
        requestForPlan.BreakfastTime = document.getElementById('useBreakfastTime').checked
            ? +document.getElementById('breakfastTime').value
            : 0;
        requestForPlan.LunchDinnerTime = document.getElementById('useLunchDinnerTime').checked
            ? +document.getElementById('lunchDinnerTime').value
            : 0;
        requestForPlan.Diet = diet;

        console.log(JSON.stringify(request));

        await calculateCalories({request})
    });

    // Убираем сообщение о подсчитанных калориях и кнопку "Generate plan",
    // если пользователь меняет что-то в форме после расчёта.
    form.addEventListener('input', () => {
        clearCaloriesInfo()
    });
    form.addEventListener('change', () => {
        clearCaloriesInfo()
    });

});

/**
 * Полная генерация плана
 */
generatePlanBtn.addEventListener('click', async () => {
    
    generatePlanBtn.disabled = true;

    const messageAfterGeneration = document.getElementById("messageAfterGeneration");
    console.log(requestForPlan);

    const generateNowOrPastMessage = document.getElementById("generateNowOrPastMessage");

    let jsonResponse = await generatePlan()
    
    if (jsonResponse == null) {
        return;
    }
    
    const plan = jsonResponse.plan;

    const plannedCalories = plan.CaloriesPlan;
    const plannedProteins = plan.ProteinsPlan;
    const plannedFats = plan.FatsPlan;
    const plannedCarbs = plan.CarbsPlan;

    const recipes = plan.Recipes.map(r => new Recipe(r));

    const recipeBreakfast = recipes[0];
    const recipeLunch = recipes[1];
    const recipeDinner = recipes[2];

    const otherRecipesDiv = document.getElementById('otherRecipes');
    const otherTitle = document.getElementById('otherTitle');

    otherRecipesDiv.innerHTML = '';

    otherTitle.style.display = 'block';
    otherTitle.textContent = "Your meal plan for today";

    const isGeneratedToday = jsonResponse.isGeneratedToday;

    const generationTime = document.getElementById('generationTime');

    if (isGeneratedToday) {

    } else {
        document.getElementById('planNote').style.display = 'block';
    }
    const meals = [
        {type: 'Breakfast', recipe: recipeBreakfast},
        {type: 'Lunch', recipe: recipeLunch},
        {type: 'Dinner', recipe: recipeDinner},
    ];

    otherRecipesDiv.classList.add('recipes-grid');

    meals.forEach(({type, recipe}) => {
        const card = document.createElement('div');
        card.className = 'recipe-card enhanced-card';
        card.innerHTML = `
                    <div class="recipe-header">
                        <h3>${type}</h3>
                    </div>
                    <img src="${recipe.Image}" alt="${recipe.Title}" class="recipe-image">
                    <div class="recipe-body">
                        <h4 class="recipe-title">${recipe.Title}</h4>
                        <p><strong>Ready in:</strong> ${recipe.ReadyInMinutes} min</p>
                        <p><strong>Servings:</strong> ${recipe.Servings}</p>
                        <div class="nutrition">
                            <p><strong>Calories:</strong> ${recipe.Calories}</p>
                            <p><strong>Protein:</strong> ${recipe.Proteins} g</p>
                            <p><strong>Carbs:</strong> ${recipe.Carbs} g</p>
                            <p><strong>Fats:</strong> ${recipe.Fats} g</p>
                        </div>
                        <a href="${recipe.SourceUrl}" target="_blank" class="recipe-link">View Recipe</a>
                    </div>
                `;
        otherRecipesDiv.appendChild(card);
    });

    otherRecipesDiv.style.display = 'flex';

    const generatedCalories = recipeBreakfast.Calories + recipeLunch.Calories + recipeDinner.Calories;
    const generatedProteins = recipeBreakfast.Proteins + recipeLunch.Proteins + recipeDinner.Proteins;
    const generatesCarbs = recipeBreakfast.Carbs + recipeLunch.Carbs + recipeDinner.Carbs;
    const generatedFats = recipeBreakfast.Fats + recipeLunch.Fats + recipeDinner.Fats;

    // Создаем блок под суммарную информацию
    const summaryDiv = document.createElement('div');
    summaryDiv.className = 'daily-summary';
    summaryDiv.innerHTML = `
                <h3>Daily Summary</h3>
                <p><strong>Total Calories:</strong> ${Math.round(generatedCalories)} kcal</p>
                <p><strong>Total Protein:</strong> ${Math.round(generatedProteins)} g</p>
                <p><strong>Total Carbs:</strong> ${Math.round(generatesCarbs)} g</p>
                <p><strong>Total Fats:</strong> ${Math.round(generatedFats)} g</p>
            `;

    // Добавляем блок под карточками
    otherRecipesDiv.parentNode.appendChild(summaryDiv);


    // Вычисляем абсолютное (отклонение) заполнение в процентах
    const achievementPercent = {
        Calories: Math.round(Math.abs(generatedCalories / plannedCalories * 100)),
        Proteins: Math.round(Math.abs(generatedProteins / plannedProteins * 100)),
        Carbs: Math.round(Math.abs(generatesCarbs / (plannedCarbs) * 100)),
        Fats: Math.round(Math.abs(generatedFats / (plannedFats) * 100))
    };


    // Округляем сами значения для подписи
    const roundedValues = {
        Calories: Math.round(generatedCalories),
        Proteins: Math.round(generatedProteins),
        Carbs: Math.round(generatesCarbs),
        Fats: Math.round(generatedFats)
    };

    // Функция для выбора цвета по проценту отклонения
    function getDeviationColor(percent) {
        if (percent <= 25) return '#4caf50';
        if (percent <= 40) return '#ffb74d';
        return '#f44336';
    }

    // Вычисляем абсолютное отклонение в процентах и округляем
    const deviationPercent = {
        Calories: Math.round(Math.abs((generatedCalories - plannedCalories) / plannedCalories * 100)),
        Proteins: Math.round(Math.abs((generatedProteins - plannedProteins) / plannedProteins * 100)),
        Carbs: Math.round(Math.abs((generatesCarbs - plannedCarbs) / plannedCarbs * 100)),
        Fats: Math.round(Math.abs((generatedFats - plannedFats) / plannedFats * 100)),
    };

    // Создаем контейнер для блока отклонений, если его нет
    let deviationDiv = document.getElementById('deviationBlock');
    if (!deviationDiv) {
        deviationDiv = document.createElement('div');
        deviationDiv.id = 'deviationBlock';
        // Вставляем после блока рецептов
        otherRecipesDiv.parentNode.insertBefore(deviationDiv, otherRecipesDiv.nextSibling);
    }

    // Заполняем содержимое
    // Заполняем блок HTML с прогресс-барами
    // Заполняем блок HTML с прогресс-барами и подписью
    deviationDiv.innerHTML = `
                <h3>Achievement of the plan (%)</h3>
                
                    <div class="progress-caption">${roundedValues.Calories} kcal from ${Math.round(plannedCalories)} kcal</div>
                
                    <div class="deviation-item">
                        <span>Calories:</span>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: ${Math.min(achievementPercent.Calories, 100)}%; background-color: ${getDeviationColor(deviationPercent.Calories)};"></div>
                        </div>
                        <span>${achievementPercent.Calories}%</span>
                    </div>
                    <div class="progress-caption">${roundedValues.Proteins} g from ${Math.round(plannedProteins)} g</div>
                    <div class="deviation-item">
                        <span>Proteins:</span>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: ${Math.min(achievementPercent.Proteins, 100)}%;  background-color: ${getDeviationColor(deviationPercent.Proteins)};"></div>
                        </div>
                        <span>${achievementPercent.Proteins}%</span>
                    </div>
                    
                    <div class="progress-caption">${roundedValues.Carbs} g from ${Math.round(plannedCarbs)} g</div>
                    
                    <div class="deviation-item">
                        <span>Carbs:</span>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: ${Math.min(achievementPercent.Carbs, 100)}%;  background-color: ${getDeviationColor(deviationPercent.Carbs)};"></div>
                        </div>
                        <span>${achievementPercent.Carbs}%</span>
                    </div>
                
                     <div class="progress-caption">${roundedValues.Fats} g from ${Math.round(plannedFats)} g</div>
                    <div class="deviation-item">
                        <span>Fats:</span>
                        <div class="progress-bar">
                            <div class="progress-fill" style="width: ${Math.min(achievementPercent.Fats, 100)}%;  background-color: ${getDeviationColor(deviationPercent.Proteins)}%;"></div>
                        </div>
                        <span>${achievementPercent.Fats}%</span>
                    </div>
                `;

    generatePlanBtn.disabled = true;
    document.getElementById('nutritionForm').style.display = 'none';
});

logoutBtn.addEventListener('click', async () => {
    try {
        const response = await fetch('/user/quit', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'}
        });
        window.location.href = "/meal_plan";
    } catch (err) {
        console.error(err);
    }
});
