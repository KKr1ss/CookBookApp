﻿using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CookBookApp.Models.Services
{
    public class RecipeServices
    {
        public async Task<List<Recipe>> getRecipesJoinedByLanguage(string language)
        {
            language = language.ToUpper();
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                var recipes = await getRecipesJoined();
                var recipesBuff = new List<Recipe>();
                var languages = await App._context.Languages.getAllAsync();
                int languageID = languages.FirstOrDefault(l => l.LanguageName == language).ID;

                foreach (Recipe recipe in recipes)
                {
                    if (recipe.Languages.Any(l => l.LanguageName == language))
                    {
                        Recipe recipeBuff = new Recipe()
                        {
                            ID = recipe.ID,
                            Author = recipe.Author,
                            PreparationTime = recipe.PreparationTime,
                            Difficulty = recipe.Difficulty,
                            Price = recipe.Price,
                            Portion = recipe.Portion,
                            CreationDate = recipe.CreationDate,
                            Languages = recipe.Languages,
                            LocalizedRecipe = recipe.Localizations.FirstOrDefault(l => l.LanguageID == languageID),
                            LocalizedCategories = recipe.Categories.Where(c => c.LanguageID == languageID).ToList(),
                            Images = recipe.Images
                        };
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

        public async Task<List<Recipe>> getRecipesJoined()
        {
            List<Recipe> recipesResults = new List<Recipe>();
            try
            {
                var recipes = await App._context.Recipes.getAllAsync();
                var recipeLocalizations = await App._context.RecipeLocalizations.getAllAsync();
                var recipeCategories = await App._context.RecipeCategories.getAllAsync();
                var recipeImages = await App._context.RecipeImages.getAllAsync();
                var languages = await App._context.Languages.getAllAsync();

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
    }
}