
# Расчет калорий
### Для генерации плана реализован подсчет калорий по 3 вариантам:
- данным тела пользователя и его цели
- общим калориям и цели
- по точным значениям белков, жиров и углеводов 

### Расчёт суточной потребности в калориях (К)
Используется формула Миффлина - Сан Жеора,
рекомендованная ВО3 и Минздравом РФ для практического применения.

Для мужчин:
> BMR = 10 x веc(кг) + 6.25 x рост(см) - 5 x возраст + 5

Для женщин:
> BMR = 10 × вес(кг) + 6.25 × рост(см) - 5 × возраст - 161
---
#### Учёт физической активности

BMR умножается на коэффициент активности (PAL — Physical Activity Level):
  
Уровень активности
Описание – Коэффициент
Минимальный – сидячая работа, нет спорта – **1.2**
Лёгкий – тренировки 1–3 раза в неделю – **1.375**
Средний – 3–5 тренировок в неделю – **1.55**
Высокий – 6–7 тренировок в неделю – **1.725**
Очень высокий – тяжёлая работа, спорт ежедневно	**1.9**

> Суточная норма калорий (К) = BMR × PAL

#### Корректировка под цель

Похудение: К × 0.85 (дефицит 15%)
Поддержание веса: К × 1.0
Набор массы: К × 1.15(профицит 15%)
Поддержка рельефа - низкий % жира: K × 0.95

#### Расчет бжу по цели и калориям

| Цель              | Калории | Белки | Жиры | Углеводы |
| ----------------- | ------- | ----- | ---- | -------- |
| WeightLoss        | -15%    | 28%   | 27%  | 45%      |
| Maintenance       | 100%    | 18%   | 27%  | 55%      |
| WeightGain        | +15%    | 18%   | 27%  | 55%      |
| BodyRecomposition | -5%     | 30%   | 25%  | 45%      |

## Генерация рецептов

Сначала получаем рецепты по заданным  характеристикам(максимум 100 - в силу ограниченности запросов к API) и по формуле калорий  - 
```
int maxCaloriesForOneMeal = (int)(planRequestForPlan.Calories * 0.3 + 0.15 * planRequestForPlan.Calories); 
int minCaloriesForOneMeal = (int)(planRequestForPlan.Calories * 0.3 - 0.15 * planRequestForPlan.Calories); 
```
Собираются завтраки и ужины. С помощью функции перебора - далее находим тройку рецептов, которые не использовались уже для пользователя вчера и которые наиболее близки к сумме по бжу. 
```
foreach (var breakfast in breakfastRecipes)
        {
            // Проверка на существующий завтрак
            if (planRequestForPlan.RecipePlanExist && breakfast.Id == planRequestForPlan.BreakfastId)
                continue;

            foreach (var lunch in lunchDinnerRecipes)
            {
                // Проверка на существующий обед
                if (planRequestForPlan.RecipePlanExist && lunch.Id == planRequestForPlan.LunchId)
                    continue;

                foreach (var dinner in lunchDinnerRecipes)
                {
                    if (lunch.Id == dinner.Id)
                        continue; // не брать один и тот же рецепт на обед и ужин

                    // Проверка на существующий ужин
                    if (planRequestForPlan.RecipePlanExist && dinner.Id == planRequestForPlan.DinnerId)
                        continue;

                    // Считаем суммарные БЖУ и калории
                    double totalProtein = breakfast.Proteins + lunch.Proteins + dinner.Proteins;
                    double totalFat = breakfast.Fats + lunch.Fats + dinner.Fats;
                    double totalCarbs = breakfast.Carbs + lunch.Carbs + dinner.Carbs;
                    double totalCalories = breakfast.Calories + lunch.Calories + dinner.Calories;

                    // Считаем score как квадрат отклонений 
                    double score =
                        Math.Pow(totalProtein - targetProtein, 2) +
                        Math.Pow(totalFat - targetFat, 2) +
                        Math.Pow(totalCarbs - targetCarbs, 2);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestBreakfast = breakfast;
                        bestLunch = lunch;
                        bestDinner = dinner;
                    }
                }
            }
        }
```

