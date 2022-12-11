﻿using CookBookApp.Helpers;
using CookBookApp.Model;
using CookBookApp.Model.Services;
using CookBookApp.Models;
using CookBookApp.Models.Services;
using CookBookApp.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookBookApp.ViewModel
{
    public class EditRecipeViewModel : BaseViewModel
    {
        RecipeCategoriesService recipeCategoriesService;
        RecipeService recipeService;
        public UserSettingsManager UserSettingsManager;


        public string[] Difficulties { get; set; }
        public string[] Prices { get; set; }
        public bool IsBusy { get; set; }
        public Recipe OriginalRecipe { get; set; }
        public Recipe EditedRecipe { get; set; }
        public Language UserLanguage { get; set; }

        int isBusyCounter;

        public ObservableCollection<RecipeCategoryNames> RecipeCategoryNames { get; set; }
        public ObservableCollection<Language> Languages { get; set; }

        public RelayCommand DeleteRecipeCommand { get; set; }


        public EditRecipeViewModel(Recipe recipe)
        {

            isBusyCounter = 0;
            recipeCategoriesService = new RecipeCategoriesService();
            recipeService = new RecipeService();
            UserSettingsManager = new UserSettingsManager();

            OriginalRecipe = recipe;
            EditedRecipe= recipe;

            UserLanguage = UserSettingsManager.getLanguage();
            Difficulties = LocalizedConstants.getDifficulties();
            Prices = LocalizedConstants.getPrices();


            loadRecipeCategories();

            DeleteRecipeCommand = new RelayCommand(deleteRecipe);
        }
        void loadRecipeCategories()
        {
            setIsBusy(true);
            Task.Run(async () =>
            {
                RecipeCategoryNames = new ObservableCollection<RecipeCategoryNames>(
                    await recipeCategoriesService.getLocalizedRecipeCategoriesAsync(UserLanguage));
                setIsBusy(false);
            });

        }
        void setIsBusy(bool toTrue)
        {
            if (toTrue)
                isBusyCounter++;
            else
                isBusyCounter--;

            if (isBusyCounter > 0)
                IsBusy = true;
            else
                IsBusy = false;
        }

        async void deleteRecipe()
        {
            await recipeService.deleteJoinedRecipeAsync(OriginalRecipe);
        }
    }
}
