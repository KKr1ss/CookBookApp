using CookBookApp.Model;
using CookBookApp.ViewModel;
using CookBookApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CookBookApp.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class _AddOrEditRecipePage : ContentPage
    {
        public _AddOrEditRecipePage(Recipe recipe)
        {
            InitializeComponent();
            BindingContext = new _AddOrEditRecipeViewModel(recipe);
        }

        private void btnViewRecipe_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            Recipe newRecipe = (Recipe)button.CommandParameter;

            Navigation.PushAsync(new ViewRecipePage(newRecipe));
        }

        private void btnBack_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MenuPage());
        }

        private void btnRestartAddRecipe_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AddRecipe_NamesAndPictures());
        }
    }
}