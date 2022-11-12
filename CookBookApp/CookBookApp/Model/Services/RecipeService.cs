﻿using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CookBookApp.Models.Services
{
    public class RecipeService
    {
        //Visszaadja a receptek listáját a megadott nyelvek alapján
        //üres lita esetén minden recept az alapértelmezett nyelvükkel
        //1 elem alapján a megadott nyelvel rendelkező receptek szerint
        //több elem alapján pedig ha valamelyiket teljesíti
        public async Task<List<Recipe>> getRecipesLocalizedAsync(string[] languages, string search)
        {
            for(int i =0; i< languages.Length; ++i)
            {
                languages[i] = languages[i].ToUpper();
            }
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                List<Recipe> recipes = new List<Recipe>();
                switch (languages.Length)
                {
                    case 0:
                        recipes = await getRecipesLocalizedNoLanguageAsync();
                        break;
                    case 1:
                        recipes = await getRecipesLocalizedOneLanguageAsync(languages[0]);
                        break;
                    default:
                        recipes = await getRecipesLocalizedMultipleLanguageAsync(languages);
                        break;
                }
                if(search != "")
                {
                    recipes = recipes.Where(r => (r.LocalizedRecipe.RecipeName.Contains(search)) 
                        || (r.LocalizedRecipe.Preparation.Contains(search))
                        || (r.LocalizedRecipe.Ingredients.Contains(search))
                    ).ToList();
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

        //vissza ad minden receptet az alapértelmezett nyelvük alapján
        private async Task<List<Recipe>> getRecipesLocalizedNoLanguageAsync()
        {
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                var recipes = await getRecipesJoinedAsync();
                var recipesBuff = new List<Recipe>();
                foreach (Recipe recipe in recipes)
                {
                    Recipe recipeBuff = getLocalizedRecipe(recipe, recipe.DefaultLanguageID);
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
        private async Task<List<Recipe>> getRecipesLocalizedOneLanguageAsync(string language)
        {
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                var recipes = await getRecipesJoinedAsync();
                var recipesBuff = new List<Recipe>();
                var languages = await App._context.Languages.GetAllAsync();
                int languageID = languages.FirstOrDefault(l => l.LanguageName == language).ID;

                foreach (Recipe recipe in recipes)
                {
                    if (recipe.Languages.Any(l => l.LanguageName == language))
                    {
                        Recipe recipeBuff = getLocalizedRecipe(recipe, languageID);
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
        private async Task<List<Recipe>> getRecipesLocalizedMultipleLanguageAsync(string[] languages)
        {
            List<Recipe> recipesResults = new List<Recipe>();
            
            try
            {
                var recipes = await getRecipesLocalizedNoLanguageAsync();
                var recipesBuff = new List<Recipe>();
                foreach (Recipe recipe in recipes)
                {
                    string[] recipeLanguages = recipe.Languages.Select(l=> l.LanguageName).ToArray();
                    if(recipeLanguages.Intersect(languages).Any())
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


        //átalakítja az összes lokalizációt tároló receptet a megadott nyelvet használó receptre
        private Recipe getLocalizedRecipe(Recipe joinedRecipe, int languageID)
        {
            Recipe recipe = joinedRecipe;
            recipe.LocalizedRecipe = recipe.Localizations.FirstOrDefault(l => l.LanguageID == languageID);
            recipe.LocalizedCategories = recipe.Categories.Where(c => c.LanguageID == languageID).ToList();
            recipe.Localizations = null;
            recipe.Categories = null;

            return recipe;
        }

        //vissza adja a recepteket, amelyek tárolnak minden lokalizációt, de saját lokalizációval nem rendelkezik
        public async Task<List<Recipe>> getRecipesJoinedAsync()
        {
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                var recipes = await App._context.Recipes.GetAllAsync();
                var recipeLocalizations = await App._context.RecipeLocalizations.GetAllAsync();
                var recipeCategories = await App._context.RecipeCategories.GetAllAsync();
                var recipeImages = await App._context.RecipeImages.GetAllAsync();
                var languages = await App._context.Languages.GetAllAsync();

                foreach(Recipe recipe in recipes)
                {
                    recipe.Localizations = recipeLocalizations.Where(rl => rl.RecipeID == recipe.ID).ToList();
                    recipe.Categories = recipeCategories.Where(rc => rc.RecipeID == recipe.ID).ToList();
                    recipe.Images = recipeImages.Where(ri => ri.RecipeID == recipe.ID).ToList();
                    var test = recipe.Localizations.Select(rl => rl.LanguageID).ToArray();
                    recipe.Languages = languages.Where(l => test.Contains(l.ID)).ToList();
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

        public async Task<bool> deleteRecipeAsync(Recipe recipeToDelete)
        {
            bool isDeleted = false;
            try
            {
                List<Recipe> recipes = await getRecipesJoinedAsync();
                Recipe recipe = recipes.FirstOrDefault(r => r.ID == recipeToDelete.ID);

                foreach(RecipeImage image in recipe.Images)
                {
                    await App._context.RecipeImages.RemoveAsync(image);
                }
                
                foreach(RecipeCategories category in recipe.Categories)
                {
                    await App._context.RecipeCategories.RemoveAsync(category);
                }

                foreach(RecipeLocalization recipeLocalization in recipe.Localizations)
                {
                    await App._context.RecipeLocalizations.RemoveAsync(recipeLocalization);
                }

                await App._context.Recipes.RemoveAsync(recipe);

                isDeleted = true;
            }
            catch (Exception ex)
            {
                //TODO: LOGGER CW HELYETT
                Console.WriteLine(ex.Message);
            }

            return await Task.FromResult(isDeleted);
        }
    }
}