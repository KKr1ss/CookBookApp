using CookBookApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookBookApp.Model.Services
{
    public class RecipesListService
    {
        RecipeContext _context;
        RecipeService recipeService;
        List<Recipe> recipesJoined;

        public RecipesListService()
        {
            _context = new RecipeContext();
            recipeService = new RecipeService();
            initializeService();
        }

        public RecipesListService(RecipeContext context)
        {
            _context = context;
            recipeService = new RecipeService(context);
            initializeService();
        }

        private void initializeService()
        {
            setJoinedRecipes();
        }

        public bool setJoinedRecipes()
        {
            bool isUpdated = false;
            try
            {
                Task.Run(async () =>
                {
                    recipesJoined = await recipeService.getJoinedRecipesAsync();
                    isUpdated = true;
                }).Wait();
            }
            catch (Exception) { }

            return isUpdated;
        }

        //Visszaadja a receptek listáját a megadott nyelvek alapján
        //üres lita esetén minden recept az alapértelmezett nyelvükkel
        //1 elem alapján a megadott nyelvel rendelkező receptek szerint
        //több elem alapján pedig ha valamelyiket teljesíti
        public async Task<List<Recipe>> getRecipesLocalizedAsync(int[] categoryNameIDs, int[] languagesIDs, string search)
        {
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                List<Recipe> recipes;

                recipes = await getRecipesLocalizedByLanguages(languagesIDs);

                if (search != "")
                {
                    recipes = await getLocalizedRecipesBySearch(recipes, search);
                }

                if (categoryNameIDs.Length > 0)
                {
                    recipes = await getLocalizedRecipesByCategories(recipes, categoryNameIDs);
                }
                recipesResults = recipes;
            }
            catch (Exception ex)
            {
                //TODO: LOGGER CW HELYETT
                Console.WriteLine(ex.Message);
            }
            return await Task.FromResult(recipesResults);
        }

        private async Task<List<Recipe>> getRecipesLocalizedByLanguages(int[] languagesIDs)
        {
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                List<Recipe> recipes = new List<Recipe>();

                switch (languagesIDs.Length)
                {
                    case 0:
                        recipes = await getRecipesLocalizedNoLanguageAsync();
                        break;
                    case 1:
                        recipes = await getRecipesLocalizedOneLanguageAsync(languagesIDs[0]);
                        break;
                    default:
                        recipes = await getRecipesLocalizedMultipleLanguageAsync(languagesIDs);
                        break;
                }
                recipesResults = recipes;
            }
            catch
            {
                throw new Exception();
            }
            return await Task.FromResult(recipesResults);
        }

        //vissza ad minden receptet az alapértelmezett nyelvük alapján
        private async Task<List<Recipe>> getRecipesLocalizedNoLanguageAsync()
        {
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                var recipes = recipesJoined.Select(r => r.Clone()).ToList();
                var recipesBuff = new List<Recipe>();
                foreach (Recipe recipe in recipes)
                {
                    Recipe recipeBuff = recipeService.getLocalizedRecipeByJoinedRecipe(recipe, recipe.DefaultLanguageID);
                    recipesBuff.Add(recipeBuff);
                }
                recipesResults = recipesBuff;
            }
            catch (Exception ex)
            {
                //TODO: LOGGER CW HELYETT
                Console.WriteLine(ex.Message);
            }

            return await Task.FromResult(recipesResults);
        }

        //vissza adja a recepteket, amelyek teljesítik az adott nyelv paraméterét
        private async Task<List<Recipe>> getRecipesLocalizedOneLanguageAsync(int languageID)
        {
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                var recipes = recipesJoined.Select(r => r.Clone()).ToList();
                var recipesBuff = new List<Recipe>();
                var languages = _context.Language.ToList();

                foreach (Recipe recipe in recipes)
                {
                    if (recipe.Languages.Any(l => l.ID == languageID))
                    {
                        Recipe recipeBuff = recipeService.getLocalizedRecipeByJoinedRecipe(recipe, languageID);
                        recipesBuff.Add(recipeBuff);
                    }
                }

                recipesResults = recipesBuff;
            }
            catch (Exception ex)
            {
                //TODO: LOGGER CW HELYETT
                Console.WriteLine(ex.Message);
            }

            return await Task.FromResult(recipesResults);
        }

        //vissza adja a recepteket, amelyek teljesítik az adott nyelveket
        private async Task<List<Recipe>> getRecipesLocalizedMultipleLanguageAsync(int[] languageIDs)
        {
            List<Recipe> recipesResults = new List<Recipe>();

            try
            {
                var recipes = await getRecipesLocalizedNoLanguageAsync();
                var recipesBuff = new List<Recipe>();
                foreach (Recipe recipe in recipes)
                {
                    int[] recipeLanguages = recipe.Languages.Select(l => l.ID).ToArray();
                    if (recipeLanguages.Intersect(languageIDs).Any())
                    {
                        recipesBuff.Add(recipe);
                    }
                }
                recipesResults = recipesBuff;
            }
            catch (Exception ex)
            {
                //TODO: LOGGER CW HELYETT
                Console.WriteLine(ex.Message);
            }

            return await Task.FromResult(recipesResults);
        }

        //a paraméterként megadott recepteket átvizsgálja a categóriák szerint
        private async Task<List<Recipe>> getLocalizedRecipesByCategories(List<Recipe> localizedRecipes, int[] categoryNameIDs)
        {
            localizedRecipes = localizedRecipes.Where(r => categoryNameIDs.Intersect(r.Categories.Select(c => c.CategoryNameID).ToArray()).Any()).ToList();
            return await Task.FromResult(localizedRecipes);
        }

        //a paraméterként megadott recepteket átvizsgálja, és visszaadja az elemeket amik megfelelnek a keresésnek
        //keresés az adott helyeken: recept neve, elkészítés, hozzávalók
        private async Task<List<Recipe>> getLocalizedRecipesBySearch(List<Recipe> localizedRecipes, string search)
        {
            search = search.ToUpper();
            localizedRecipes = localizedRecipes.Where(r =>
                        (r.LocalizedRecipe.RecipeName.ToUpper().Contains(search)) ||
                        (r.LocalizedRecipe.Preparation.ToUpper().Contains(search)) ||
                        (r.LocalizedRecipe.Ingredients.ToUpper().Contains(search))
                    ).ToList();
            return await Task.FromResult(localizedRecipes);
        }


        public async Task<bool> deleteMultipleJoinedRecipeAsync(List<Recipe> recipesToDelete)
        {
            bool isDeletesSucceeded = true;
            try
            {
                foreach(Recipe recipe in recipesToDelete)
                {
                    var isDeleted = await recipeService.deleteRecipeAsync(recipe);
                    if (!isDeleted) isDeletesSucceeded = false;
                }
            }
            catch (Exception)
            {
            }
            return isDeletesSucceeded;
        }
    }
}
